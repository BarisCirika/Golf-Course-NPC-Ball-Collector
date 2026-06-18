// NpcSpawner.cs
using GCNBC.Components;
using GCNBC.Services;
using GCNBC.Signals;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace GCNBC.Components
{
    public class NpcSpawner : MonoBehaviour
    {
        [Header("Spawn point (e.g. next to the golf cart)")]
        [SerializeField] private Transform _spawnPoint;

        private NpcFactory _npcFactory;
        private NPCPrefabRegistry _prefabRegistry;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(NpcFactory npcFactory, NPCPrefabRegistry prefabRegistry, SignalBus signalBus)
        {
            _npcFactory = npcFactory;
            _prefabRegistry = prefabRegistry;
            _signalBus = signalBus;
        }

        private void OnEnable() => _signalBus.Subscribe<NpcDiedSignal>(OnNpcDied);
        private void OnDisable() => _signalBus.Unsubscribe<NpcDiedSignal>(OnNpcDied);

        private void Start()
        {
            SpawnNpc();
        }

        private void OnNpcDied(NpcDiedSignal _)
        {
            SpawnNpc();
        }

        private void SpawnNpc()
        {
            // Pick a random NPC prefab from the registry.
            GameObject prefab = _prefabRegistry.GetRandomNpcPrefab();
            if (prefab == null) return;

            // Create it (injected) via the factory.
            NpcComponent npc = _npcFactory.Create(prefab);

            // Place it on the NavMesh at the spawn point.
            Vector3 pos = _spawnPoint != null ? _spawnPoint.position : transform.position;
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                pos = hit.position;

            npc.transform.position = pos;

            var agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                bool warped = agent.Warp(pos);
                Debug.Log($"[NpcSpawner] Warp success: {warped}, agent.isOnNavMesh: {agent.isOnNavMesh}");
            }

            _signalBus.Fire(new NpcSpawnedSignal(npc));
        }
    }
}