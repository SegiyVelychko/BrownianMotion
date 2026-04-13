namespace BrownianMotion.Simulation.Models;

/// <summary>
/// Determines which crystal implementation the engine uses.
/// </summary>
public enum SimulationMode
{
    /// <summary>All mutations go through <see cref="System.Threading.Interlocked"/> — particle count is always conserved.</summary>
    Safe,

    /// <summary>Mutations use an explicit TOCTOU window to guarantee a visible race condition.</summary>
    Unsafe,
}
