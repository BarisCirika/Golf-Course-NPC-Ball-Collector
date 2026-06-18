// NpcAnimationController.cs
using UnityEngine;

namespace GCNBC.Components
{
    [RequireComponent(typeof(Animator))]
    public class NpcAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
        }

        // States call these. The controller is the single place that touches the Animator.
        public void SetSpeed(float value)
        {
            _animator.SetFloat(NpcAnimations.Speed, value);
        }

        public void SetCarrying(bool carrying)
        {
            _animator.SetBool(NpcAnimations.IsCarrying, carrying);
        }

        public void SetDead(bool dead)
        {
            _animator.SetBool(NpcAnimations.Die, dead);
        }
    }
}