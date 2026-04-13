using BrownianMotion.Simulation.Contracts;
using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Engine;

/// <summary>
/// Represents a single impurity particle running on its own dedicated <see cref="Thread"/>.
/// </summary>
/// <remarks>
/// <para>
/// The particle owns its current position exclusively — no other thread reads or writes
/// <c>_row</c>/<c>_col</c>, so no synchronisation is needed for those fields.
/// </para>
/// <para>
/// <b>Reproducibility.</b> Each particle derives its RNG seed deterministically from the
/// global seed and its own ID using the Knuth multiplicative hash constant
/// (<c>2^32 / φ</c>), so identical seeds produce identical trajectories regardless of
/// OS thread scheduling.
/// </para>
/// <para>
/// <b>Boundary strategy.</b> A move that would leave the grid is clamped to the nearest
/// valid cell (reflection at the boundary).
/// </para>
/// <para>
/// <b>Barrier protocol.</b> After each move the thread calls
/// <see cref="Barrier.SignalAndWait(CancellationToken)"/>.  The engine participates as the
/// final barrier member; its post-phase action captures a consistent snapshot while all
/// particle threads are suspended.
/// </para>
/// </remarks>
public sealed class ParticleThread
{
    // Knuth multiplicative hash: 2^32 / φ (golden ratio), cast to int.
    // Spreads sequential IDs uniformly across the int space.
    private const int KnuthFactor = unchecked((int)2_654_435_761u);

    private readonly ICrystal _crystal;
    private readonly SimulationConfig _cfg;
    private readonly Barrier _barrier;
    private readonly CancellationToken _ct;
    private readonly Random _rng;
    private readonly Thread _thread;

    // Owned exclusively by this thread — no locking required.
    private int _row;
    private int _col;

    /// <summary>Zero-based particle identifier.</summary>
    public int Id { get; }

    /// <summary><c>true</c> while the underlying thread is alive.</summary>
    public bool IsAlive => _thread.IsAlive;

    /// <summary>
    /// Initialises the particle but does not start the thread.
    /// Call <see cref="Start"/> to begin execution.
    /// </summary>
    /// <param name="id">Zero-based particle identifier.</param>
    /// <param name="startRow">Initial row position.</param>
    /// <param name="startCol">Initial column position.</param>
    /// <param name="crystal">Shared crystal lattice.</param>
    /// <param name="cfg">Simulation configuration for this run.</param>
    /// <param name="barrier">Barrier shared with all other particles and the engine.</param>
    /// <param name="ct">Token used to request cooperative cancellation.</param>
    public ParticleThread(
        int id,
        int startRow,
        int startCol,
        ICrystal crystal,
        SimulationConfig cfg,
        Barrier barrier,
        CancellationToken ct)
    {
        Id = id;
        _row = startRow;
        _col = startCol;
        _crystal = crystal;
        _cfg = cfg;
        _barrier = barrier;
        _ct = ct;

        var seed = cfg.Seed.HasValue
            ? cfg.Seed.Value ^ (id * KnuthFactor)
            : Random.Shared.Next();

        _rng = new Random(seed);

        _thread = new Thread(Run)
        {
            Name = $"Particle-{id}",
            IsBackground = true,
        };
    }

    /// <summary>Starts the particle thread.</summary>
    public void Start() => _thread.Start();

    /// <summary>Blocks until the particle thread has exited.</summary>
    public void Join() => _thread.Join();

    // ── Thread entry point ───────────────────────────────────────────────

    private void Run()
    {
        _crystal.Place(_row, _col);

        for (var step = 0; step < _cfg.TotalSteps; step++)
        {
            if (_ct.IsCancellationRequested) break;

            Step();

            try
            {
                _barrier.SignalAndWait(_ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (BarrierPostPhaseException ex)
                when (ex.InnerException is OperationCanceledException)
            {
                // Snapshot action was cancelled — treat as graceful stop.
                break;
            }
        }
    }

    private void Step()
    {
        var (newRow, newCol) = ChooseDestination();

        // ICrystal.Move encapsulates whether the operation is safe or not;
        // ParticleThread has no knowledge of the concrete implementation.
        _crystal.Move(_row, _col, newRow, newCol);

        _row = newRow;
        _col = newCol;
    }

    /// <summary>
    /// Samples a direction from the configured probability distribution
    /// and applies boundary reflection (clamping).
    /// </summary>
    private (int row, int col) ChooseDestination()
    {
        var roll = _rng.NextDouble();

        (int dr, int dc) = roll switch
        {
            _ when roll < _cfg.ProbUp => (-1,  0),
            _ when roll < _cfg.ProbUp + _cfg.ProbDown => ( 1,  0),
            _ when roll < _cfg.ProbUp + _cfg.ProbDown + _cfg.ProbLeft => ( 0, -1),
            _ => ( 0,  1),
        };

        return (
            Math.Clamp(_row + dr, 0, _crystal.Rows - 1),
            Math.Clamp(_col + dc, 0, _crystal.Cols - 1)
        );
    }
}
