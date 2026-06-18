using UnityEngine;

namespace GCNBC.SOs
{
    [CreateAssetMenu(fileName = "NpcStatsConfig", menuName = "GCNBC/NPC Stats Config")]
    public class NpcStatsConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("NavMeshAgent speed.")]
        public float speed = 5f;
        [Tooltip("NavMeshAgent acceleration.")]
        public float acceleration = 8f;
        [Tooltip("NavMeshAgent angular speed (turn rate).")]
        public float angularSpeed = 120f;
        [Tooltip("NavMeshAgent angular speed (turn rate).")]
        public float stoppingDistance = 1f;

        [Header("Health")]
        public float maxHealth = 100f;
        public float healthDrainPerSecond = 2f;
        [Tooltip("Health restored when delivering a ball to the cart.")]
        public float healthPerDelivery = 10f;

        [Header("Scoring")]
        [Tooltip("Multiplier applied to the points of each delivered ball (1 = normal).")]
        public float pointsMultiplier = 1f;


        [Header("Behavior")]
        public float arriveDistance = 1f;
    }
}
