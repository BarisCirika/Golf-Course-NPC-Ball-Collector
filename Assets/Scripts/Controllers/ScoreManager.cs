using GCNBC.Signals;
using Zenject;


namespace GCNBC.Services
{
    public class ScoreManager : IScoreService
    {
        private readonly SignalBus _signalBus;
        public int Current { get; private set; }

        public ScoreManager(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Add(int points)
        {
            Current += points;
            _signalBus.Fire(new ScoreChangedSignal(Current));
        }

        public void Reset()
        {
            Current = 0;
            _signalBus.Fire(new ScoreChangedSignal(Current));
        }
    }
}
