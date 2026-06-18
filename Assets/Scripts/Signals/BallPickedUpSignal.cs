using GCNBC.Enums.Model;

namespace GCNBC.Signals{
    public class BallPickedUpSignal
    {
        public BallLevel Level { get; }
        public BallPickedUpSignal(BallLevel level) => Level = level;
    }
}
