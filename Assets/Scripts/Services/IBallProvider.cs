using System.Collections.Generic;
using GCNBC.Components;

namespace GCNBC.Services
{
    // Abstracts "what balls are available" away from the concrete spawner.
    public interface IBallProvider
    {
        IReadOnlyCollection<BallComponent> ActiveBalls { get; }
        void Release(BallComponent ball);   // return a collected ball to its pool
    }
}