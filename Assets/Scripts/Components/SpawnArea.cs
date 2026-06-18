using GCNBC.Enums.Model;
using UnityEngine;

namespace GCNBC.Components
{
    // A hand-placed spawn region. Put these in the scene, size them in the editor,
    // and assign which ball level spawns here.
    public class SpawnArea : MonoBehaviour
    {
        [Tooltip("Which ball level spawns in this area.")]
        public BallLevel level;

        [Tooltip("Relative weight — higher = more likely to get balls. 1 = normal.")]
        public float weight = 1f;

        [SerializeField] private Vector3 _size = new Vector3(20f, 0f, 20f);

        public Vector3 GetRandomPoint()
        {
            Vector3 half = _size * 0.5f;
            float x = Random.Range(-half.x, half.x);
            float z = Random.Range(-half.z, half.z);
            return transform.TransformPoint(new Vector3(x, 0f, z));
        }

        // Draw the area in the Scene view so you can place/size it visually.
        private void OnDrawGizmos()
        {
            // Color per level so you can tell areas apart at a glance.
            Gizmos.color = level switch
            {
                BallLevel.Level1 => new Color(0f, 1f, 0f, 0.25f),   // green = easy
                BallLevel.Level2 => new Color(1f, 1f, 0f, 0.25f),   // yellow = medium
                BallLevel.Level3 => new Color(1f, 0f, 0f, 0.25f),   // red = hard
                _ => new Color(1f, 1f, 1f, 0.25f)
            };

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, new Vector3(_size.x, 1f, _size.z));
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_size.x, 1f, _size.z));
        }
    }
}