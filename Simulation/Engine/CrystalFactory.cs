using BrownianMotion.Simulation.Contracts;
using BrownianMotion.Simulation.Crystal;
using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Engine;

/// <summary>
/// Creates the appropriate <see cref="ICrystal"/> implementation
/// based on the requested <see cref="SimulationMode"/>.
/// Centralises the mode → implementation mapping so the engine
/// does not contain any branching on mode after construction.
/// </summary>
public static class CrystalFactory
{
    /// <summary>
    /// Returns a new crystal instance for the given <paramref name="mode"/>.
    /// </summary>
    /// <param name="rows">Number of rows. Must be &gt; 0.</param>
    /// <param name="cols">Number of columns. Must be &gt; 0.</param>
    /// <param name="mode">Desired simulation mode.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown for an unrecognised <paramref name="mode"/> value.
    /// </exception>
    public static ICrystal Create(int rows, int cols, SimulationMode mode)
        => mode switch
        {
            SimulationMode.Safe   => new SafeCrystal(rows, cols),
            SimulationMode.Unsafe => new UnsafeCrystal(rows, cols),
            _                     => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
        };
}
