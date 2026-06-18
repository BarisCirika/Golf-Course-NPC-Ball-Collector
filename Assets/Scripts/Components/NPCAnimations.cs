using UnityEngine;

namespace GCNBC.Components
{
    // Animator PARAMETER hashes — single source of truth.
    public static class NpcAnimations
    {
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int IsCarrying = Animator.StringToHash("IsCarrying");
        public static readonly int Die = Animator.StringToHash("Die");
    }
}