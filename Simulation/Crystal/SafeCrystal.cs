using BrownianMotion.Simulation.Contracts;

namespace BrownianMotion.Simulation.Crystal;

/// <summary>
/// Thread-safe 2-D crystal lattice.
/// All per-cell mutations use <see cref="Interlocked"/> operations,
/// guaranteeing that the total particle count is always conserved.
/// </summary>
/// <remarks>
/// <see cref="Move"/> uses a compare-exchange retry loop to atomically
/// decrement the source cell without allowing it to go negative.
/// The operation is not transactional across two cells: another thread may
/// briefly observe the particle removed from the source before it appears
/// in the destination, but the total count remains unchanged throughout.
/// </remarks>
public sealed class SafeCrystal : ICrystal
{
    private readonly int[] _cells;
    private int _totalParticles;

    /// <inheritdoc/>
    public int Rows { get; }

    /// <inheritdoc/>
    public int Cols { get; }

    /// <summary>Initialises a new grid with all cells set to zero.</summary>
    /// <param name="rows">Number of rows. Must be &gt; 0.</param>
    /// <param name="cols">Number of columns. Must be &gt; 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rows"/> or <paramref name="cols"/> is not positive.
    /// </exception>
    public SafeCrystal(int rows, int cols)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cols);

        Rows   = rows;
        Cols   = cols;
        _cells = new int[checked(rows * cols)];
    }

    /// <inheritdoc/>
    public void Place(int r, int c)
    {
        Interlocked.Increment(ref _cells[Idx(r, c)]);
        Interlocked.Increment(ref _totalParticles);
    }

    /// <inheritdoc/>
    public void Move(int fromR, int fromC, int toR, int toC)
    {
        var fi = Idx(fromR, fromC);
        var ti = Idx(toR,   toC);

        if (fi == ti) return;

        // CAS retry loop: decrement source only when it is still positive.
        while (true)
        {
            var current = Volatile.Read(ref _cells[fi]);
            if (current <= 0) return; // cell is empty — boundary reflection already applied

            var original = Interlocked.CompareExchange(ref _cells[fi], current - 1, current);
            if (original == current)
            {
                Interlocked.Increment(ref _cells[ti]);
                return;
            }
            // Another thread modified _cells[fi] between our read and CAS — retry.
        }
    }

    /// <inheritdoc/>
    public int Get(int r, int c) => Volatile.Read(ref _cells[Idx(r, c)]);

    /// <inheritdoc/>
    public int[] Snapshot() => (int[])_cells.Clone();

    /// <inheritdoc/>
    public int Total() => Volatile.Read(ref _totalParticles);

    /// <inheritdoc/>
    public void Reset()
    {
        Array.Clear(_cells, 0, _cells.Length);
        Volatile.Write(ref _totalParticles, 0);
    }

    private int Idx(int r, int c)
    {
        if ((uint)r >= (uint)Rows)
            throw new ArgumentOutOfRangeException(nameof(r), $"Row {r} is out of range [0..{Rows - 1}].");
        if ((uint)c >= (uint)Cols)
            throw new ArgumentOutOfRangeException(nameof(c), $"Col {c} is out of range [0..{Cols - 1}].");

        return r * Cols + c;
    }
}
