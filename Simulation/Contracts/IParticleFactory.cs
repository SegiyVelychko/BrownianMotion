using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Contracts;

/// <summary>
/// Creates <see cref="Engine.ParticleThread"/> instances for a simulation run.
/// Exists as a seam for unit-testing the engine independently of thread creation.
/// </summary>
public interface IParticleFactory
{
    /// <summary>
    /// Creates a particle thread that will operate on <paramref name="crystal"/>
    /// and synchronise with the engine via <paramref name="barrier"/>.
    /// </summary>
    /// <param name="id">Zero-based particle identifier; used to derive a per-particle RNG seed.</param>
    /// <param name="startRow">Initial row position.</param>
    /// <param name="startCol">Initial column position.</param>
    /// <param name="crystal">The shared crystal lattice.</param>
    /// <param name="cfg">Configuration for this run.</param>
    /// <param name="barrier">Barrier synchronising all particle threads with the engine.</param>
    /// <param name="ct">Token used to request cooperative cancellation.</param>
    Engine.ParticleThread Create(
        int              id,
        int              startRow,
        int              startCol,
        ICrystal         crystal,
        SimulationConfig cfg,
        Barrier          barrier,
        CancellationToken ct);
}
