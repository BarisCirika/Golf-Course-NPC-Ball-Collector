using GCNBC.Enums.Model;
using UnityEngine;

namespace GCNBC.SOs
{
    [CreateAssetMenu(menuName = "GCNBC/BallSpawnConfig")]
    public class BallLevelConfig : ScriptableObject
    {
        [Tooltip("Which level this config represents.")]
        public BallLevel level;

        [Tooltip("Points awarded when a ball of this level is collected.")]
        public int points = 1;

        [Header("Spawn Count")]
        [Tooltip("Minimum total balls for this level.")]
        public int minCount = 15;
        [Tooltip("Maximum total balls. Set equal to min for a fixed count.")]
        public int maxCount = 25;

        [Header("Difficulty / Placement")]
        [Tooltip("Minimum distance from spawn center (higher = harder to reach).")]
        public float minDistance = 0f;
        [Tooltip("Maximum distance from spawn center.")]
        public float maxDistance = 30f;
        [Tooltip("Max ground height allowed for this level (higher levels can allow hills).")]
        public float maxSpawnHeight = 20f;
        [Tooltip("Min ground height (e.g. force high levels onto hills).")]
        public float minSpawnHeight = 0f;

        public int RollTotal() => Random.Range(minCount, maxCount + 1);
    }
}
