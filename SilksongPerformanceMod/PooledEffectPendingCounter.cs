using System.Threading;

namespace SilksongPerformanceMod;

/// <summary>
/// O(1) count of pooled effects waiting in per-tracker recycle queues (delayed release).
/// </summary>
internal static class PooledEffectPendingCounter
{
    private static int _pending;

    public static int Pending => Volatile.Read(ref _pending);

    public static void Increment()
    {
        Interlocked.Increment(ref _pending);
    }

    public static void Decrement()
    {
        int value = Volatile.Read(ref _pending);
        if (value > 0)
        {
            Interlocked.Decrement(ref _pending);
        }
    }

    public static void Reset()
    {
        Interlocked.Exchange(ref _pending, 0);
    }
}
