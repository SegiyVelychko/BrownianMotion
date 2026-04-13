using System.Diagnostics;
using BrownianMotion.Simulation.Contracts;
using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Demo;

/// <summary>
/// Provides runnable deadlock demonstration scenarios.
/// </summary>
/// <remarks>
/// <para>
/// <b>Unsafe scenario.</b> Thread A locks <c>_lockA</c> then tries <c>_lockB</c>;
/// Thread B locks <c>_lockB</c> then tries <c>_lockA</c>. With a small sleep between
/// the two acquisitions the circular wait is reliably produced.
/// A <see cref="Monitor.TryEnter(object, int)"/> timeout is used to detect and escape
/// the deadlock — without it the threads would hang indefinitely.
/// </para>
/// <para>
/// <b>Safe scenario.</b> Both threads always acquire locks in ascending identity order
/// (determined by <see cref="RuntimeHelpers.GetHashCode"/>). A cycle in the
/// wait-for graph is impossible, so deadlock cannot occur.
/// </para>
/// </remarks>
public sealed class DeadlockDemo : IDeadlockDemo
{
    private readonly object _lockA = new();
    private readonly object _lockB = new();

    /// <inheritdoc/>
    public DeadlockResult RunUnsafe(int timeoutMs = 2000)
    {
        string statusA   = "not started";
        string statusB   = "not started";
        var    deadlocked = false;
        var    sw         = Stopwatch.StartNew();

        var threadA = new Thread(() =>
        {
            lock (_lockA)
            {
                statusA = "holds lockA, waiting for lockB";
                Thread.Sleep(100); // give Thread B time to acquire lockB

                if (Monitor.TryEnter(_lockB, timeoutMs / 2))
                {
                    statusA = "acquired both locks (no deadlock this run)";
                    Monitor.Exit(_lockB);
                }
                else
                {
                    statusA   = "TIMED OUT waiting for lockB → DEADLOCK";
                    deadlocked = true;
                }
            }
        }) { Name = "DeadlockDemo-A", IsBackground = true };

        var threadB = new Thread(() =>
        {
            lock (_lockB)
            {
                statusB = "holds lockB, waiting for lockA";
                Thread.Sleep(100);

                if (Monitor.TryEnter(_lockA, timeoutMs / 2))
                {
                    statusB = "acquired both locks (no deadlock this run)";
                    Monitor.Exit(_lockA);
                }
                else
                {
                    statusB   = "TIMED OUT waiting for lockA → DEADLOCK";
                    deadlocked = true;
                }
            }
        }) { Name = "DeadlockDemo-B", IsBackground = true };

        threadA.Start();
        threadB.Start();
        threadA.Join(timeoutMs);
        threadB.Join(timeoutMs);

        return new DeadlockResult(
            DeadlockOccurred: deadlocked,
            Description: deadlocked
                ? "DEADLOCK detected: threads timed out waiting for each other."
                : "No deadlock this run (scheduling luck). Try again.",
            ThreadAStatus: statusA,
            ThreadBStatus: statusB,
            ElapsedMs: sw.ElapsedMilliseconds);
    }

    /// <inheritdoc/>
    public DeadlockResult RunSafe(int timeoutMs = 2000)
    {
        // Always acquire in ascending identity order — no cycle possible.
        var (first, second) = OrderedLocks(_lockA, _lockB);

        string statusA = "not started";
        string statusB = "not started";
        var    sw      = Stopwatch.StartNew();

        var threadA = new Thread(() =>
        {
            lock (first) { lock (second)
            {
                statusA = "acquired both locks in safe order";
                Thread.Sleep(50);
            }}
        }) { Name = "SafeDemo-A", IsBackground = true };

        var threadB = new Thread(() =>
        {
            lock (first) { lock (second)
            {
                statusB = "acquired both locks in safe order";
                Thread.Sleep(50);
            }}
        }) { Name = "SafeDemo-B", IsBackground = true };

        threadA.Start();
        threadB.Start();
        var finishedA = threadA.Join(timeoutMs);
        var finishedB = threadB.Join(timeoutMs);
        var timedOut  = !finishedA || !finishedB;

        return new DeadlockResult(
            DeadlockOccurred: timedOut,
            Description: timedOut
                ? "Unexpected timeout in safe scenario."
                : "Both threads completed without deadlock.",
            ThreadAStatus: statusA,
            ThreadBStatus: statusB,
            ElapsedMs: sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Returns the two objects ordered by their stable runtime identity hash,
    /// ensuring a globally consistent acquisition order across all callers.
    /// </summary>
    private static (object first, object second) OrderedLocks(object a, object b)
    {
        var ha = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(a);
        var hb = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(b);
        return ha <= hb ? (a, b) : (b, a);
    }
}
