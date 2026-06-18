using GCNBC.Components;

namespace GCNBC.Signals
{
    public class NpcSpawnedSignal
    {
        public NpcComponent Npc { get; }
        public NpcSpawnedSignal(NpcComponent npc) => Npc = npc;
    }
}
