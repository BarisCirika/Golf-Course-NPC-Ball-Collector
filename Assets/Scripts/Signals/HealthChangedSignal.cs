public class NpcHealthChangedSignal
{
    public float Current { get; }
    public float Max { get; }
    public NpcHealthChangedSignal(float current, float max)
    {
        Current = current;
        Max = max;
    }
}