using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Contracts;

/// <summary>
/// Provides runnable deadlock demonstration scenarios.
/// </summary>
public interface IDeadlockDemo
{
    /// <summary>
    /// Runs two threads that acquire two locks in opposite order,
    /// reliably producing a circular-wait deadlock detected via timeout.
    /// </summary>
    /// <param name="timeoutMs">
    /// Per-thread lock-wait timeout in milliseconds.
    /// A thread that cannot acquire the second lock within this window is
    /// considered deadlocked.
    /// </param>
    DeadlockResult RunUnsafe(int timeoutMs = 2000);

    /// <summary>
    /// Runs two threads that acquire the same two locks in a globally
    /// consistent order, preventing circular wait.
    /// </summary>
    /// <param name="timeoutMs">
    /// Safety timeout; should never be hit in the safe scenario.
    /// </param>
    DeadlockResult RunSafe(int timeoutMs = 2000);
}
