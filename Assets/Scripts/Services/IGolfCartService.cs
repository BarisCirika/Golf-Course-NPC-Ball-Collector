using UnityEngine;

namespace GCNBC.Services
{
    public interface ICartService
    {
        Vector3 Position { get; }
        void OnBallDelivered();
    }
}
