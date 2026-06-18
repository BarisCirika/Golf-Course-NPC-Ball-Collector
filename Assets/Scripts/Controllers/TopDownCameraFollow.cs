using GCNBC.Signals;
using UnityEngine;
using Zenject;

namespace GCNBC.Services
{
    public class TopDownCameraFollow : MonoBehaviour
    {
        [Header("Offset & Angle")]
        [Tooltip("Position offset from the target (high Y for top-down, some Z for slight angle).")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 25f, -10f);

        [Tooltip("How sharply the camera looks down. Higher X angle = more straight-down.")]
        [SerializeField] private Vector3 _lookAngle = new Vector3(60f, 0f, 0f);

        [Header("Smoothing")]
        [Tooltip("Higher = snappier follow, lower = smoother lag.")]
        [SerializeField] private float _followSpeed = 5f;

        private Transform _target;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void OnEnable() => _signalBus.Subscribe<NpcSpawnedSignal>(OnNpcSpawned);
        private void OnDisable() => _signalBus.Unsubscribe<NpcSpawnedSignal>(OnNpcSpawned);

        // When a new NPC spawns, follow it.
        private void OnNpcSpawned(NpcSpawnedSignal signal)
        {
            _target = signal.Npc != null ? signal.Npc.transform : null;
        }

        // LateUpdate so the camera moves AFTER the NPC has moved this frame (no jitter).
        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPos = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, _followSpeed * Time.deltaTime);

            // Keep a fixed top-down angle (don't rotate with the NPC).
            transform.rotation = Quaternion.Euler(_lookAngle);
        }
    }
}