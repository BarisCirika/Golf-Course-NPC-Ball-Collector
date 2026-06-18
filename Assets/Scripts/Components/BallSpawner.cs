// BallSpawner.cs
using GCNBC.Enums.Model;
using GCNBC.Services;
using GCNBC.SOs;
using Lean.Pool;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace GCNBC.Components
{
    public class BallSpawner : MonoBehaviour, IBallProvider
    {
        // Maps a level to its object pool.
        [System.Serializable]
        public class LevelPool
        {
            public BallLevel level;
            public LeanGameObjectPool pool;
        }

        [Header("Pools - one per level")]
        [SerializeField] private List<LevelPool> _levelPools = new();

        [Header("Spawn Areas - hand-placed regions")]
        [Tooltip("Leave empty to auto-find all SpawnAreas in the scene.")]
        [SerializeField] private List<SpawnArea> _spawnAreas = new();

        [Header("Placement")]
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _raycastHeight = 200f;
        [SerializeField] private float _navMeshSampleDistance = 5f;
        [SerializeField] private float _heightOffset = 0.1f;
        [SerializeField] private int _maxAttemptsPerBall = 20;

        [Header("Async")]
        [SerializeField] private int _spawnBatchSize = 5;

        // Per-level data (points + spawn count) comes from injected configs.
        private List<BallLevelConfig> _configs;

        // Tracks spawned balls and their pools (for IBallProvider + release).
        private readonly Dictionary<BallComponent, LeanGameObjectPool> _ballToPool = new();
        public IReadOnlyCollection<BallComponent> ActiveBalls => _ballToPool.Keys;

        [Inject]
        private void Construct(List<BallLevelConfig> configs)
        {
            _configs = configs;
        }

        private void Start()
        {
            // Auto-find areas in the scene if none were assigned in the inspector.
            if (_spawnAreas == null || _spawnAreas.Count == 0)
                _spawnAreas = new List<SpawnArea>(FindObjectsByType<SpawnArea>(FindObjectsSortMode.None));

            SpawnAllAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // Spawns all balls: iterates configs, and for each level rolls a total
        // (from config) and distributes it randomly across that level's areas.
        public async UniTask SpawnAllAsync(CancellationToken ct)
        {
            ReleaseAll();
            int sinceYield = 0;

            // Each config carries everything for its level: points + spawn count.
            foreach (var config in _configs)
            {
                if (config == null) continue;

                // Areas hand-placed for this level.
                List<SpawnArea> areas = GetAreasForLevel(config.level);
                if (areas.Count == 0)
                {
                    Debug.LogWarning($"[BallSpawner] No areas for level {config.level}, skipping.");
                    continue;
                }

                // Pool for this level.
                LeanGameObjectPool pool = GetPoolForLevel(config.level);
                if (pool == null)
                {
                    Debug.LogWarning($"[BallSpawner] No pool for level {config.level}, skipping.");
                    continue;
                }

                // Random total for this run (min..max from config).
                int total = config.RollTotal();

                // Spawn each ball in a randomly chosen area of this level.
                for (int i = 0; i < total; i++)
                {
                    SpawnArea area = PickRandomArea(areas);
                    SpawnOneInArea(area, pool, config.level, config.points);

                    // Yield periodically so spawning doesn't block a frame.
                    if (++sinceYield >= _spawnBatchSize)
                    {
                        sinceYield = 0;
                        await UniTask.Yield(ct);
                    }
                }
            }

            Debug.Log($"[BallSpawner] Spawned {_ballToPool.Count} balls total.");
        }

        // Places one ball in an area: random point -> ground raycast -> NavMesh snap.
        private void SpawnOneInArea(SpawnArea area, LeanGameObjectPool pool, BallLevel level, int points)
        {
            for (int attempt = 0; attempt < _maxAttemptsPerBall; attempt++)
            {
                Vector3 xz = area.GetRandomPoint();
                Vector3 rayStart = new Vector3(xz.x, _raycastHeight, xz.z);

                // Drop onto the ground.
                if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit groundHit, _raycastHeight * 2f, _groundMask))
                    continue;

                // Snap to NavMesh so the NPC can reach it.
                if (!NavMesh.SamplePosition(groundHit.point, out NavMeshHit navHit, _navMeshSampleDistance, NavMesh.AllAreas))
                    continue;

                Vector3 pos = navHit.position;
                pos.y += _heightOffset;

                GameObject go = pool.Spawn(pos, Quaternion.identity);
                BallComponent ball = go.GetComponent<BallComponent>();
                if (ball == null) ball = go.AddComponent<BallComponent>();
                ball.Init(level, points);
                _ballToPool[ball] = pool;
                return;   // success
            }

            Debug.LogWarning($"[BallSpawner] Could not place a ball in area '{area.name}' after {_maxAttemptsPerBall} tries.");
        }

        // Picks a random area, weighted by area.weight (higher = more likely).
        private SpawnArea PickRandomArea(List<SpawnArea> areas)
        {
            float totalWeight = 0f;
            foreach (var a in areas) totalWeight += Mathf.Max(0f, a.weight);

            // All weights zero -> equal chance.
            if (totalWeight <= 0f)
                return areas[Random.Range(0, areas.Count)];

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (var a in areas)
            {
                cumulative += Mathf.Max(0f, a.weight);
                if (roll <= cumulative) return a;
            }
            return areas[areas.Count - 1];   // floating-point fallback
        }

        private List<SpawnArea> GetAreasForLevel(BallLevel level)
        {
            var result = new List<SpawnArea>();
            foreach (var a in _spawnAreas)
                if (a != null && a.level == level) result.Add(a);
            return result;
        }

        private LeanGameObjectPool GetPoolForLevel(BallLevel level)
        {
            foreach (var lp in _levelPools)
                if (lp.level == level) return lp.pool;
            return null;
        }

        // --- IBallProvider ---

        public void Release(BallComponent ball)
        {
            if (ball == null) return;
            if (_ballToPool.TryGetValue(ball, out var pool))
            {
                pool.Despawn(ball.gameObject);
                _ballToPool.Remove(ball);
            }
            else
            {
                LeanPool.Despawn(ball.gameObject);
            }
        }

        public void ReleaseAll()
        {
            foreach (var pair in _ballToPool)
                if (pair.Key != null) pair.Value.Despawn(pair.Key.gameObject);
            _ballToPool.Clear();
        }
    }
}