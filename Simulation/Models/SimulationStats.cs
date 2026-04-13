namespace BrownianMotion.Simulation.Models;

/// <summary>
/// Final statistics produced at the end of a simulation run.
/// </summary>
/// <param name="TotalSteps">Number of steps that were executed.</param>
/// <param name="ParticleCount">Number of particle threads that participated.</param>
/// <param name="ElapsedMs">Total wall-clock duration in milliseconds.</param>
/// <param name="FinalTotal">
/// Particle count as reported by the crystal at the end of the run.
/// In safe mode this always equals <see cref="ExpectedTotal"/>.
/// In unsafe mode it may differ due to lost updates.
/// </param>
/// <param name="UnsafeStats">
/// Extended diagnostics for unsafe runs; <c>null</c> in safe mode.
/// </param>
/// <param name="ExpectedTotal">
/// The number of particles that were placed at the start — the invariant
/// a correct implementation must preserve.
/// </param>
/// <param name="Seed">RNG seed used, or <c>null</c> for a non-reproducible run.</param>
/// <param name="Mode">String label of the active <see cref="SimulationMode"/>.</param>
public sealed record SimulationStats(
    int          TotalSteps,
    int          ParticleCount,
    double       ElapsedMs,
    int          FinalTotal,
    UnsafeStats? UnsafeStats,
    int          ExpectedTotal,
    int?         Seed,
    string       Mode)
{
    /// <summary>
    /// Number of particles missing at the end of the run.
    /// Zero in safe mode; positive when race conditions caused lost updates.
    /// </summary>
    public int Lost => ExpectedTotal - FinalTotal;
}
