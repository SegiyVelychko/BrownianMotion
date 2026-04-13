using BrownianMotion.Simulation.Contracts;
using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Engine;

/// <summary>
/// Default implementation of <see cref="IParticleFactory"/>.
/// Creates <see cref="ParticleThread"/> instances with the supplied parameters.
/// </summary>
public sealed class ParticleFactory : IParticleFactory
{
    /// <inheritdoc/>
    public ParticleThread Create(
        int               id,
        int               startRow,
        int               startCol,
        ICrystal          crystal,
        SimulationConfig  cfg,
        Barrier           barrier,
        CancellationToken ct)
        => new(id, startRow, startCol, crystal, cfg, barrier, ct);
}
