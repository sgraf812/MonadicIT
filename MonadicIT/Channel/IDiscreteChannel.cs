using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public interface IDiscreteChannel<T> where T : /* Enum, */ struct
    {
        Distribution<T> GetTransitionDistribution(T symbol);
        double ChannelCapacity { get; }
    }
}