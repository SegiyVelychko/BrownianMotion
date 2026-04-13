using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Crystal;

/// <summary>
/// Pure statistical functions over a flat cell array.
/// All methods operate on an already-taken snapshot so no locking is required.
/// </summary>
public static class CrystalStats
{
    /// <summary>Returns the algebraic sum of all cell values, including negatives.</summary>
    public static int Total(int[] cells) => cells.Sum();

    /// <summary>
    /// Returns the number of cells whose value is below zero.
    /// Any positive result is physical proof of a lost update.
    /// </summary>
    public static int NegativeCellCount(int[] cells) => cells.Count(v => v < 0);

    /// <summary>Returns the minimum cell value, showing worst-case underflow depth.</summary>
    public static int MinCell(int[] cells) => cells.Length > 0 ? cells.Min() : 0;

    /// <summary>Returns the maximum cell value, used to normalise heatmap colours.</summary>
    public static int MaxCell(int[] cells) => cells.Length > 0 ? cells.Max() : 0;

    /// <summary>
    /// Computes all unsafe-mode diagnostics over <paramref name="snapshot"/> in a single pass.
    /// </summary>
    public static UnsafeStats GetUnsafeStats(int[] snapshot) => new(
        RawSum: Total(snapshot),
        NegativeCells: NegativeCellCount(snapshot),
        MinCell: MinCell(snapshot));
}
