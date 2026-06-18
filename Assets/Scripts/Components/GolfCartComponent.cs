using GCNBC.Services;
using UnityEngine;

namespace GCNBC.Components
{
    // Stationary golf cart that serves as the scoring base.
    public class GolfCartComponent : MonoBehaviour, ICartService
    {
        [Header("Delivery Effect")]
        [SerializeField] private ParticleSystem _deliveryEffect;
        public Vector3 Position => transform.position;

        public void OnBallDelivered()
        {
            if (_deliveryEffect != null)
                _deliveryEffect.Play();
        }
    }
}