using GCNBC.Enums.Components;
using GCNBC.NPCStates;
using GCNBC.Services;
using GCNBC.Signals;
using GCNBC.SOs;
using GCNBC.ViewControllers.UI;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace GCNBC.Components
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NpcComponent : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private NpcStatsConfig _stats;

        [Header("Animation")]
        [SerializeField] private NpcAnimationController _animation;

        [Header("Carry")]
        [Tooltip("Where the carried ball is attached (e.g. an empty child in front of/above the NPC).")]
        [SerializeField] private Transform _carryPoint;

        [Header("Health Bar")]
        [SerializeField] private NpcHealthBar _healthBar;
        public Transform CarryPoint => _carryPoint;

        // --- Injected services ---
        private IBallProvider _ballProvider;
        private ICartService _cart;
        private IScoreService _scoreManager;
        private SignalBus _signalBus;

        // --- Runtime ---
        private NavMeshAgent _agent;
        private StateMachine<NpcComponent, NpcState> _fsm;
        private float _currentHealth;
        private bool _isDead;
        private Vector3 _lastPos;
        private bool _hasLastPos;
        // Exposed for states to use:
        public NavMeshAgent Agent => _agent;
        public IBallProvider BallProvider => _ballProvider;
        public ICartService Cart => _cart;
        public IScoreService ScoreManager => _scoreManager;
        public SignalBus SignalBus => _signalBus;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _stats != null ? _stats.maxHealth : 100f;
        public float HealthDrainPerSecond => _stats != null ? _stats.healthDrainPerSecond : 0.5f;
        public float HealthPerDelivery => _stats != null ? _stats.healthPerDelivery : 10f;
        public float PointsMultiplier => _stats != null ? _stats.pointsMultiplier : 1f;
        public float ArriveDistance => _stats != null ? _stats.arriveDistance : 1f;
        public float Speed => _stats != null ? _stats.speed : _agent.speed;

        public bool IsDead => _isDead;
        public NpcAnimationController Animation => _animation;

        // The ball the NPC is currently carrying (collected, not yet delivered).
        public BallComponent CarriedBall { get; set; }

        [Inject]
        private void Construct(IBallProvider ballProvider, ICartService cart,
                               IScoreService scoreService, SignalBus signalBus)
        {
            _ballProvider = ballProvider;
            _cart = cart;
            _scoreManager = scoreService;
            _signalBus = signalBus;
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_animation == null) _animation = GetComponent<NpcAnimationController>();
            ApplyStats();
            BuildStateMachine();
        }

        private void ApplyStats()
        {
            if (_stats == null)
            {
                Debug.LogWarning("[NpcComponent] No stats config assigned, using agent defaults.");
                return;
            }

            _agent.speed = _stats.speed;
            _agent.acceleration = _stats.acceleration;
            _agent.angularSpeed = _stats.angularSpeed;
            _agent.stoppingDistance = _stats.stoppingDistance;   // istersen config'e ekle
        }


        private void OnEnable()
        {
            _currentHealth = MaxHealth;
            _isDead = false;
            CarriedBall = null;
            _signalBus.Fire(new NpcSpawnedSignal(this));
            
            if (_healthBar != null)
                _healthBar.SetHealth(_currentHealth, MaxHealth);
        }

        private void Start()
        {
            _fsm.Initialize(NpcState.Evaluate);   // start by deciding what to do
        }

        private void Update()
        {
            if (_hasLastPos)
            {
                float jump = Vector3.Distance(transform.position, _lastPos);
                if (jump > 5f)
                    Debug.LogError($"[NPC] TELEPORT! jumped {jump:F1}m from {_lastPos} to {transform.position}");
            }
            _lastPos = transform.position;
            _hasLastPos = true;
            if (_isDead) return;

            DrainHealth();
            _fsm.Tick();
        }

        private void BuildStateMachine()
        {
            _fsm = new StateMachine<NpcComponent, NpcState>(this);
            _fsm.AddState(NpcState.Idle, new IdleState());
            _fsm.AddState(NpcState.Evaluate, new EvaluateState());
            _fsm.AddState(NpcState.MoveToBall, new MoveToBallState());
            _fsm.AddState(NpcState.ReturnToCart, new ReturnToCartState());
            _fsm.AddState(NpcState.Dead, new DeadState());
        }

        private void DrainHealth()
        {
            _currentHealth -= HealthDrainPerSecond * Time.deltaTime;
            Debug.Log($"[NPC] Health: {_currentHealth:F1} / {MaxHealth}");
            if (_healthBar != null)
                _healthBar.SetHealth(_currentHealth, MaxHealth);

            _signalBus.Fire(new NpcHealthChangedSignal(_currentHealth, MaxHealth));
            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
                Die();
            }
        }

        // Called by states to transition.
        public void ChangeState(NpcState state) => _fsm.ChangeState(state);

        // Restores some health (e.g. when delivering a ball to the cart).
        public void AddHealth(float amount)
        {
            _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
            if (_healthBar != null)
                _healthBar.SetHealth(_currentHealth, MaxHealth);
        }

        private void Die()
        {
            _isDead = true;
            if (_agent.isOnNavMesh) _agent.isStopped = true;
            _fsm.ChangeState(NpcState.Dead);

            Destroy(gameObject);
        }

    }
}