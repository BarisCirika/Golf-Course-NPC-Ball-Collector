using UnityEngine;

namespace GCNBC.Enums.Model
{
    public enum BallLevel
    {
        Level0 = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3
    }
}

namespace GCNBC.Enums.Components
{
    public enum NpcState
    {
        Idle            = 0,
        Evaluate        = 1,
        MoveToBall      = 2,
        ReturnToCart    = 3,
        Dead            = 4
    }
}

