using GCNBC.Enums.Model;

namespace GCNBC.Signals
{
    public class BallCollectedSignal
    {
        public BallLevel Level { get; }
        public int Points { get; }
        public BallCollectedSignal(BallLevel level, int points)
        {
            Level = level;
            Points = points;
        }
    }
}

