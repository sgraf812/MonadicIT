using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public interface IDiscreteChannel<T> where T : /* Enum, */ struct
    {
        double ChannelCapacity { get; }
        Distribution<T> GetTransitionDistribution(T symbol);
    }
}