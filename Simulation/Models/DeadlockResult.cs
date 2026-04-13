namespace BrownianMotion.Simulation.Models;

/// <summary>
/// Result produced by a deadlock demonstration scenario.
/// </summary>
/// <param name="DeadlockOccurred">
/// <c>true</c> when at least one thread timed out waiting for a lock,
/// indicating a circular-wait deadlock.
/// </param>
/// <param name="Description">Human-readable summary of the outcome.</param>
/// <param name="ThreadAStatus">Final status message from Thread A.</param>
/// <param name="ThreadBStatus">Final status message from Thread B.</param>
/// <param name="ElapsedMs">Wall-clock duration of the scenario in milliseconds.</param>
public sealed record DeadlockResult(
    bool DeadlockOccurred,
    string Description,
    string ThreadAStatus,
    string ThreadBStatus,
    long ElapsedMs);
