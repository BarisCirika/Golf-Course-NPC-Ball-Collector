# Golf Course NPC Ball Collector

A top-down Unity game where an AI-driven NPC autonomously collects golf balls scattered
across a course and delivers them to a stationary cart for scoring, while managing a
constantly draining health pool. Built for Unity 6.3 with URP.

This document covers the game mechanics, the decision-making algorithm, the architecture,
and the assumptions made during development.

---

## 1. Gameplay Overview

The NPC operates on its own. There is no direct player control over the NPC; the player
observes an autonomous agent making moment-to-moment decisions.

- **Balls** spawn across the course in three difficulty tiers (Level 1 = easy/low value,
  Level 2 = medium, Level 3 = hard/high value). Higher tiers are worth more points and are
  placed in harder-to-reach regions.
- The **NPC** evaluates the available balls, chooses one based on a value-vs-risk
  calculation, walks to it, picks it up, carries it to the **cart**, and delivers it to
  score points.
- The NPC has a **health pool that drains continuously over time**. Delivering a ball to
  the cart restores some health. If health reaches zero, the NPC dies and the game ends.
- The on-screen HUD shows **score**, **elapsed time** (mm:ss), and **NPC health** (as a bar).
- Pressing **ESC** pauses the game; on death the same panel appears as a "You Died" screen.
  Both offer **Replay** (reload the level) and **Close**. This pause/game-over panel is driven
  by `PauseController` (DOTween fade/scale; once dead, ESC can no longer resume).

The core tension is a survival/efficiency loop: the NPC must keep delivering balls to stay
alive, but must avoid committing to balls it cannot reach before dying.

---

## 2. Decision-Making Algorithm

The NPC's intelligence lives in its **finite state machine (FSM)**, specifically in the
evaluation state, which selects which ball to pursue. This is the heart of the assignment,
so it is described in detail.

### 2.1 The problem with naive scoring

An early version scored balls with a simple formula along the lines of
`value - distance * factor`. This breaks down on a large map (the course is ~1000×1000
units). At that scale, raw distance dominates the point value entirely: a ball worth 5
points 300 units away scores `5 - 30 = -25`, so the point value becomes irrelevant and the
NPC effectively always walks to the nearest ball. The formula is also not scale-independent;
the magic `factor` has to be re-tuned whenever the map size or speed changes.

### 2.2 The approach used: time-and-health budgeting

Instead of working in raw distance, the algorithm converts everything into **time** and then
into **health cost**, which makes it independent of map scale.

For each candidate ball, the NPC computes the **full round trip** it would actually have to
make to score: `NPC → ball → cart` (because points are only awarded on delivery, not on
pickup). That distance is converted to travel time using the agent's real speed, and travel
time is converted to a health cost using the drain rate:

```
roundTripDistance = distance(NPC, ball) + distance(ball, cart)
travelTime        = roundTripDistance / agentSpeed
healthCost        = travelTime * healthDrainPerSecond
```

**Survivability filter.** Any ball whose `healthCost * safetyMargin` is greater than or equal
to the NPC's current health is rejected outright — the NPC would die before delivering it.
The safety margin (e.g. 1.2) keeps the NPC from cutting trips dangerously close.

**Efficiency scoring.** Among the surviving (reachable) balls, each is scored by efficiency —
points earned per second of effort:

```
efficiency = ball.Points / travelTime
```

This naturally answers the "near-low-value vs far-high-value" question with a real
risk/reward trade-off. A nearby Level 1 ball can out-score a distant Level 3 ball if it is
much faster to collect.

**Low-health caution.** As health drops, a reachability bonus increasingly favours shorter,
safer trips, so a wounded NPC prefers nearby balls even at the cost of some efficiency.

### 2.3 Why this is robust

- **Scale-independent.** Because the math is in time and health, the same logic works whether
  a ball is 10 units or 1000 units away — no per-map tuning of a distance factor.
- **No suicidal trips.** The round-trip survivability filter prevents the NPC from committing
  to balls it cannot deliver before dying.
- **Data-driven core stats.** Health, drain rate, agent speed, and delivery health live in
  `NpcStatsConfig` (a ScriptableObject), so the main balance knobs are tunable in the
  inspector. The survivability `safetyMargin` (1.2) and the low-health caution weight (10)
  are currently constants inside `EvaluateState`.

---

## 3. NPC State Machine

The NPC behaviour is a custom generic FSM (`StateMachine<TOwner, TStateKey>`), with the
following states:

| State           | Responsibility                                                        |
|-----------------|-----------------------------------------------------------------------|
| **Idle**        | Standing still (transient; mostly a resting state).                    |
| **Evaluate**    | Runs the decision-making algorithm and selects the best ball.         |
| **MoveToBall**  | Navigates to the chosen ball, picks it up on arrival.                  |
| **ReturnToCart**| Carries the ball to the cart, delivers it (score + health + effects). |
| **Dead**        | Plays the death animation; the game-over flow takes over.             |

Each state only issues `SetDestination` to the NavMeshAgent; **movement speed is applied
once** from the stats config in `Awake`, not per-state, so states never fight over agent
settings. Every state guards against acting while the agent is off the NavMesh
(`if (!agent.isOnNavMesh) return;`).

State transitions are driven entirely by the FSM in code. The Animator is treated as a pure
playback device (see §5), so there is a single source of truth for behaviour rather than two
parallel state machines to keep in sync.

---

## 4. Architecture

The project is built around **dependency injection (Zenject/Extenject)**, with a clear
separation between application-level (persistent) systems and scene-level (per-play) systems.

### 4.1 Context hierarchy

- **ProjectContext** (persists for the whole app, across scene loads):
  - `AudioController` — music + pooled SFX (MonoBehaviour, needs AudioSources).
  - `AdsController`, `AnalyticsController` — placeholder startup services for future SDKs.
  - `ControllerManager` — a bootstrap orchestrator that initializes all startup services.
  - `SceneTransitionController` — centralized async scene loading.
- **SceneContext** (created/destroyed with each scene):
  - `SignalBus` and all gameplay signal declarations.
  - Gameplay services (`IScoreService`/`ScoreManager`, `IBallProvider`/`BallSpawner`,
    `ICartService`/`GolfCartComponent`).
  - `GameManager` — handles ball collection: adds score, fires `BallCollectedSignal`, and
    returns the ball to its pool.
  - `NpcFactory` — creates the NPC via `DiContainer.InstantiatePrefab` so `[Inject]` runs on
    the spawned NPC; the NPC spawner uses it.
  - `TopDownCameraFollow` (camera follow) and the `SceneAudioBinder` audio bridge.

Because a SceneContext is a child of the ProjectContext, scene-level systems can resolve
project-level dependencies (e.g. the persistent `AudioController`), but not vice versa. This
relationship is used deliberately throughout (see §6).

### 4.2 Bootstrap / startup

`ControllerManager` implements Zenject's `IInitializable`. On startup it receives
`List<IInitializableService>` — every service bound under that interface — and initializes
each one. Adding a new startup service later (e.g. push notifications, remote config) only
requires binding it; the manager needs no changes (open/closed principle).

Not everything is a MonoBehaviour. Services with no Unity dependency (manager, ads,
analytics) are plain C# classes hooked into Zenject's lifecycle via `IInitializable` /
`IDisposable`. Only systems that genuinely need Unity (audio, spawner, NPC) are
MonoBehaviours. This keeps the non-Unity logic lightweight and testable.

> **Note / assumption:** A ProjectContext is only loaded once a SceneContext triggers it.
> The first scene (`StarterScene`) therefore contains its own SceneContext so that the
> ProjectContext (and thus all startup services) initializes from the very first scene.

### 4.3 Object pooling

Balls and SFX audio sources are pooled (LeanPool for balls, a custom queue-based pool for
SFX) to avoid instantiate/destroy churn during play. Balls are pooling-aware via `IPoolable`
(reset state on spawn/despawn) rather than relying on `SendMessage`.

### 4.4 Async with UniTask

Asynchronous work (ball spawning, scene loading, SFX cleanup) uses **UniTask** rather than
coroutines, with `CancellationToken`s tied to object lifetime so nothing runs against a
destroyed object.

---

## 5. Animation

The Animator is driven by **parameters** (`Speed` float, `IsCarrying` bool, `Die` bool) with
transitions configured in the Animator Controller:

- **Speed** is fed each frame from the agent's real velocity (with damping), driving a smooth
  Idle ↔ Run blend.
- **IsCarrying** toggles an **upper-body layer** (with an avatar mask, Override blending): the
  NPC plays a "carry" pose on the upper body while the base layer keeps running. The layer's
  weight is blended in/out so the transition is smooth.
- **Die** is a bool (reset on (re)spawn since bools persist).

The decision to use parameters + transitions (rather than direct `CrossFade` calls) was made
because it gives smooth blending and keeps the "which animation" logic visible in the
Animator. The gameplay FSM only sets parameters; it never plays clips directly.

**Footsteps** are triggered by Animation Events on the run clip. The footstep clips and all
randomization (random pitch and volume per step) live in the `AudioController`; the NPC's
footstep component only reports "a step happened at this position." Random pitch/volume make
a small number of clips sound varied rather than repetitive.

---

## 6. Audio

`AudioController` (ProjectContext, persistent) owns all audio: a single looping music source
and a pool of SFX sources, routed through **AudioMixer groups** (Music / SFX / UI) so each
category can be controlled independently. It exposes only "play" methods and does **not** know
about signals.

Because audio is persistent (ProjectContext) but gameplay signals live in the SceneContext, a
**`SceneAudioBinder`** bridges the two. It lives in the SceneContext, subscribes to the
scene's signals on `Initialize`, routes each signal to the appropriate audio method, and
unsubscribes on `Dispose` (scene teardown). This keeps signals in their own scope, keeps audio
free of any SignalBus dependency, and isolates the "which signal → which sound" mapping in one
place.

`PlayMusic` does not restart a track that is already playing the same clip, so music continues
seamlessly across scene transitions instead of restarting.

---

## 7. Events / Signals

Gameplay communication uses Zenject's **SignalBus**. Key signals:

- `BallPickedUpSignal` — NPC grabs a ball off the ground (distinct "pickup" feedback).
- `BallCollectedSignal` — NPC delivers a ball to the cart; carries level and points scored.
- `ScoreChangedSignal` — score updated (HUD listens).
- `NpcHealthChangedSignal` — NPC health changed (health bar listens).
- `NpcDiedSignal` — NPC died (game-over flow listens).
- `NpcSpawnedSignal` — NPC (re)spawned (camera re-targets).

Pickup and delivery are intentionally separate events so they can have distinct feedback (a
"pop" on pickup, a "ding" plus a particle burst on delivery). **Scoring happens on delivery**,
not on pickup, matching the round-trip logic in the decision algorithm.

---

## 8. Ball Spawning

Ball placement is **designer-driven rather than fully procedural**. Difficulty is defined by
hand-placed `SpawnArea` regions in the scene (coloured gizmos: green = L1, yellow = L2,
red = L3), so the level designer decides where each tier appears (e.g. a red L3 region on a
hilltop, a green L1 region on the fairway) rather than leaving it to an automatic
distance/height heuristic.

For variety, the spawner does **not** use a fixed count per area. Instead, each level's
`BallLevelConfig` defines a **total count (min..max)**, and the spawner distributes that total
**randomly (optionally weighted) across that level's areas**. This produces a different
distribution every play (a given area may get many balls one run, few the next), and the total
itself can vary if `min < max`. There are three layers of randomness: the total count, which
area each ball lands in, and where within the area.

Placement itself raycasts down onto the ground (rejecting water and out-of-bounds), then snaps
to the NavMesh so the NPC can always reach every ball.

---

## 9. Scene Flow

```
1-StarterScene  →  2-MainMenuScene  →  3-GameplayScene
   (loading)           (menu)               (play)
```

- **StarterScene** — bootstrap/loading screen; a DOTween-animated bar fills while the next
  scene loads asynchronously, then transitions.
- **MainMenuScene** — Start / Quit, with a DOTween pulse on the Start button.
- **GameplayScene** — the game itself.

Scene loading is centralized in `SceneTransitionController` (async, with progress reported via
an event so the UI can react without the controller knowing about UI). Scene names are kept in
a single `SceneNames` constants class to avoid magic-string typos that fail silently at load
time.

---

## 10. Assumptions

- **Single NPC, single cart.** The game runs one NPC at a time; on death the game ends (no
  respawn) and the player chooses Replay or Close. The decision-making and particle/audio
  wiring assume one active NPC and one delivery cart.
- **Death is terminal.** Reaching zero health ends the run; it does not respawn mid-game.
- **One game mode / one gameplay scene.** Per-level data (points and spawn counts) lives in
  `BallLevelConfig` ScriptableObjects. If multiple difficulty modes were needed later, they
  would use separate configs.
- **Map scale ~1000×1000.** Default tuning (health, drain rate, delivery health) is calibrated
  for this scale, but the decision algorithm itself is scale-independent.
- **The course ground is raycastable and NavMesh-baked.** Spawning relies on a ground layer
  mask and a baked NavMesh; water is excluded from spawn placement.

---

## 11. Tech Stack

- **Unity 6.3**, Universal Render Pipeline (URP)
- **Extenject (Zenject)** — dependency injection, signals, lifecycle
- **UniTask** — async/await without coroutines
- **LeanPool** — object pooling
- **DOTween** — UI and feedback animation
- **AI Navigation (NavMesh)** — NPC pathfinding
