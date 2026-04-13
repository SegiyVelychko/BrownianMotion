using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Contracts;

/// <summary>
/// Orchestrates a Brownian-motion simulation: spawns particle threads,
/// drives the step barrier, fires snapshots, and reports final statistics.
/// </summary>
public interface ISimulationEngine : IDisposable
{
    /// <summary>
    /// Raised on the thread-pool after every snapshot step while the simulation
    /// is running. Subscribers must marshal to the UI thread if needed.
    /// </summary>
    event Action<Snapshot> SnapshotReady;

    /// <summary>
    /// Raised once on the thread-pool when the simulation finishes or is stopped.
    /// </summary>
    event Action<SimulationStats> Completed;

    /// <summary><c>true</c> between a <see cref="RunAsync"/> call and its completion.</summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts the simulation and returns when it has fully completed.
    /// Throws <see cref="InvalidOperationException"/> if already running.
    /// </summary>
    Task RunAsync();

    /// <summary>
    /// Requests cooperative cancellation. The simulation stops at the next
    /// barrier phase boundary; <see cref="RunAsync"/> will return shortly after.
    /// </summary>
    void Stop();
}
