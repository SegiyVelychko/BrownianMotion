using BrownianMotion.Simulation.Contracts;
using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Crystal;

/// <summary>
/// Intentionally unsynchronised 2-D crystal lattice for race-condition demonstration.
/// </summary>
/// <remarks>
/// <para>
/// On x86/x64 a plain <c>int++</c> compiles to a single <c>INC [mem]</c> instruction
/// which is effectively atomic for aligned 32-bit values — no visible race occurs.
/// To guarantee a reproducible TOCTOU window the source-cell decrement is split:
/// </para>
/// <list type="number">
///   <item><description>Read the cell value into a local variable.</description></item>
///   <item><description>Call <see cref="Thread.Yield"/> to widen the race window.</description></item>
///   <item><description>Write <c>local - 1</c> back — another thread may have already
///   decremented the same cell, so this write overwrites its result → lost update →
///   particle vanishes.</description></item>
/// </list>
/// <para>
/// Only the decrement carries the yield. The destination increment is a plain <c>++</c>
/// so that lost updates manifest as particle disappearance rather than phantom creation,
/// keeping the demonstration focused on a single failure mode.
/// </para>
/// </remarks>
public sealed class UnsafeCrystal : ICrystal
{
    private readonly int[] _cells;

    /// <inheritdoc/>
    public int Rows { get; }

    /// <inheritdoc/>
    public int Cols { get; }

    /// <summary>Initialises a new unsafe grid with all cells set to zero.</summary>
    /// <param name="rows">Number of rows. Must be &gt; 0.</param>
    /// <param name="cols">Number of columns. Must be &gt; 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rows"/> or <paramref name="cols"/> is not positive.
    /// </exception>
    public UnsafeCrystal(int rows, int cols)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cols);

        Rows   = rows;
        Cols   = cols;
        _cells = new int[checked(rows * cols)];
    }

    /// <inheritdoc/>
    /// <remarks>
    /// No yield on initial placement — we want the baseline count to equal
    /// the configured particle count before the race starts.
    /// </remarks>
    public void Place(int r, int c) => _cells[Idx(r, c)]++;

    /// <inheritdoc/>
    /// <remarks>
    /// Explicit TOCTOU race on the source cell:
    /// Thread A reads <c>cells[fi] = 3</c>, yields.
    /// Thread B reads <c>cells[fi] = 3</c>, writes <c>2</c>.
    /// Thread A resumes, also writes <c>2</c> — one decrement is lost.
    /// </remarks>
    public void Move(int fromR, int fromC, int toR, int toC)
    {
        var fi = Idx(fromR, fromC);
        var ti = Idx(toR,   toC);

        if (fi == ti) return;

        var stale = _cells[fi]; // read
        Thread.Yield();          // widen the race window
        _cells[fi] = stale - 1; // write stale → lost update

        _cells[ti]++;            // plain increment — no extra yield
    }

    /// <inheritdoc/>
    public int   Get(int r, int c)  => _cells[Idx(r, c)];

    /// <inheritdoc/>
    public int[] Snapshot() => (int[])_cells.Clone();

    /// <inheritdoc/>
    public int Total() => CrystalStats.Total(_cells);

    /// <inheritdoc/>
    public void Reset() => Array.Clear(_cells, 0, _cells.Length);

    /// <summary>
    /// Computes race-condition diagnostics over an already-taken snapshot.
    /// Accepts the snapshot array directly to avoid a redundant <c>Clone()</c>.
    /// </summary>
    /// <param name="snapshot">Array previously returned by <see cref="Snapshot"/>.</param>
    public UnsafeStats GetStats(int[] snapshot) => CrystalStats.GetUnsafeStats(snapshot);

    private int Idx(int r, int c) => r * Cols + c;
}
