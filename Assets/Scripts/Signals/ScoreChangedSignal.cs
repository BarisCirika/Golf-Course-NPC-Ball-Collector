

namespace GCNBC.Signals
{
    public class ScoreChangedSignal
    {
        public int NewScore { get; }
        public ScoreChangedSignal(int newScore) => NewScore = newScore;
    }
}


