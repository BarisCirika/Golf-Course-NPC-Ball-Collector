using GCNBC.Enums.Model;
using UnityEngine;
using Lean.Pool;

namespace GCNBC.Components
{
    public class BallComponent : MonoBehaviour, IPoolable
    {
        public BallLevel Level { get; private set; }
        public int Points { get; private set; }

        // Called by the spawner right after the ball is spawned from its pool.
        public void Init(BallLevel level, int points)
        {
            Level = level;
            Points = points;
        }

        void IPoolable.OnSpawn()
        {
        }

        void IPoolable.OnDespawn()
        {
            transform.SetParent(null);
            var rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = true;

            Level = BallLevel.Level0;
            Points = 0;
        }
    }
}
