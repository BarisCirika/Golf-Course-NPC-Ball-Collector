using System;

namespace GCNBC.Services
{
    public interface IScoreService
    {
        int Current { get; }
        void Add(int points);
        void Reset();
    }
}

