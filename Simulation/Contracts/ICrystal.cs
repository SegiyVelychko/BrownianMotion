namespace BrownianMotion.Simulation.Contracts;

/// <summary>
/// Represents a 2-D crystal lattice that stores per-cell particle counts.
/// Implementations decide whether mutations are thread-safe or deliberately racy.
/// </summary>
public interface ICrystal
{
    /// <summary>Number of rows in the grid.</summary>
    int Rows { get; }

    /// <summary>Number of columns in the grid.</summary>
    int Cols { get; }

    /// <summary>
    /// Places a particle into the specified cell.
    /// </summary>
    /// <param name="r">Zero-based row index.</param>
    /// <param name="c">Zero-based column index.</param>
    void Place(int r, int c);

    /// <summary>
    /// Moves a particle from the source cell to the destination cell.
    /// When <c>fromR/fromC == toR/toC</c> the call is a no-op.
    /// </summary>
    /// <param name="fromR">Source row.</param>
    /// <param name="fromC">Source column.</param>
    /// <param name="toR">Destination row.</param>
    /// <param name="toC">Destination column.</param>
    void Move(int fromR, int fromC, int toR, int toC);

    /// <summary>Returns the current particle count of the specified cell.</summary>
    /// <param name="r">Zero-based row index.</param>
    /// <param name="c">Zero-based column index.</param>
    int Get(int r, int c);

    /// <summary>
    /// Returns a point-in-time copy of all cell values in row-major order.
    /// </summary>
    int[] Snapshot();

    /// <summary>Returns the total number of particles across all cells.</summary>
    int Total();

    /// <summary>
    /// Resets all cells to zero.
    /// Must not be called concurrently with <see cref="Place"/> or <see cref="Move"/>.
    /// </summary>
    void Reset();
}
