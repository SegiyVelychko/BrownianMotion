namespace BrownianMotion.Simulation.Models;

/// <summary>
/// Immutable point-in-time copy of the crystal state.
/// Captured inside the <see cref="System.Threading.Barrier"/> post-phase action
/// while all particle threads are suspended, guaranteeing no mid-move cell values.
/// </summary>
/// <param name="Step">Simulation step at which this snapshot was taken.</param>
/// <param name="ElapsedMs">Wall-clock milliseconds since the run started.</param>
/// <param name="Cells">
/// Flat cell array in row-major order: <c>index = row * Cols + col</c>.
/// </param>
/// <param name="Rows">Number of rows in the crystal.</param>
/// <param name="Cols">Number of columns in the crystal.</param>
/// <param name="TotalParticles">Sum of all cell values at this step.</param>
/// <param name="UnsafeStats">
/// Race-condition diagnostics; <c>null</c> when running in <see cref="SimulationMode.Safe"/> mode.
/// </param>
/// <param name="Mode">String label of the active <see cref="SimulationMode"/>.</param>
public sealed record Snapshot(
    int Step,
    double ElapsedMs,
    int[] Cells,
    int Rows,
    int Cols,
    int TotalParticles,
    UnsafeStats? UnsafeStats,
    string Mode)
{
    /// <summary>
    /// Maximum particle density across all cells.
    /// Used to normalise the heatmap colour scale.
    /// Returns 1 when the grid is empty to avoid division by zero.
    /// </summary>
    public int MaxDensity => Cells.Length > 0 ? Math.Max(Cells.Max(), 1) : 1;
}
