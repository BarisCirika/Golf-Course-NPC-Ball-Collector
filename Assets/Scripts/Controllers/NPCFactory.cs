using GCNBC.Components;
using UnityEngine;
using Zenject;

namespace GCNBC.Services
{

    public class NpcFactory
    {
        private readonly DiContainer _container;
        public NpcFactory(DiContainer container) => _container = container;

        public NpcComponent Create(GameObject prefab)
        {
            // InstantiatePrefab ensures [Inject] runs on the new NPC.
            GameObject go = _container.InstantiatePrefab(prefab);
            return go.GetComponent<NpcComponent>();
        }
    }
}