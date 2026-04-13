using System.Diagnostics;
using BrownianMotion.Simulation.Contracts;
using BrownianMotion.Simulation.Crystal;
using BrownianMotion.Simulation.Models;

namespace BrownianMotion.Simulation.Engine;

/// <summary>
/// Default implementation of <see cref="ISimulationEngine"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Threading model.</b> One <see cref="ParticleThread"/> is created per particle.
/// The engine itself participates in the <see cref="Barrier"/> as one additional member.
/// After every step all threads converge at the barrier; the post-phase action fires while
/// every participant is suspended, guaranteeing a consistent snapshot with no mid-move values.
/// </para>
/// <para>
/// <b>Snapshot consistency.</b> <see cref="ICrystal.Snapshot"/> is called exclusively inside
/// the post-phase action, so the returned array is always fully settled.
/// </para>
/// </remarks>
public sealed class SimulationEngine : ISimulationEngine
{
    /// <inheritdoc/>
    public event Action<Snapshot>? SnapshotReady;

    /// <inheritdoc/>
    public event Action<SimulationStats>? Completed;

    private readonly SimulationConfig _cfg;
    private readonly ICrystal _crystal;
    private readonly IParticleFactory _factory;
    private readonly Stopwatch _sw = new();

    private Barrier? _barrier;
    private CancellationTokenSource _cts = new();
    private IReadOnlyList<ParticleThread> _particles = [];

    /// <inheritdoc/>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Initialises the engine with a validated configuration.
    /// The crystal implementation is chosen based on <see cref="SimulationConfig.Mode"/>.
    /// </summary>
    /// <param name="cfg">Validated simulation configuration.</param>
    /// <param name="factory">
    /// Particle factory. Pass <c>null</c> to use the default <see cref="ParticleFactory"/>.
    /// </param>
    public SimulationEngine(SimulationConfig cfg, IParticleFactory? factory = null)
    {
        _cfg = cfg;
        _crystal = CrystalFactory.Create(cfg.Rows, cfg.Cols, cfg.Mode);
        _factory = factory ?? new ParticleFactory();
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the engine is already running.</exception>
    public async Task RunAsync()
    {
        if (IsRunning)
            throw new InvalidOperationException("Simulation is already running.");

        IsRunning = true;

        _crystal.Reset();
        _cts = new CancellationTokenSource();

        var placementRng = _cfg.Seed.HasValue
            ? new Random(_cfg.Seed.Value)
            : new Random();

        var starts = Enumerable
            .Range(0, _cfg.ParticleCount)
            .Select(_ => (r: placementRng.Next(_cfg.Rows), c: placementRng.Next(_cfg.Cols)))
            .ToList();

        _barrier = new Barrier(
            participantCount: _cfg.ParticleCount + 1,
            postPhaseAction:  b => OnBarrierPhaseComplete(b.CurrentPhaseNumber));

        _particles = starts
            .Select((s, i) => _factory.Create(i, s.r, s.c, _crystal, _cfg, _barrier, _cts.Token))
            .ToList();

        _sw.Restart();
        foreach (var p in _particles) p.Start();

        await Task.Run(EngineLoop);

        foreach (var p in _particles) p.Join();

        _sw.Stop();
        IsRunning = false;

        Completed?.Invoke(BuildStats());
    }

    /// <inheritdoc/>
    public void Stop() => _cts.Cancel();

    // ── Engine barrier loop (runs on thread-pool) ────────────────────────

    private void EngineLoop()
    {
        for (var step = 0; step < _cfg.TotalSteps; step++)
        {
            if (_cts.Token.IsCancellationRequested) break;

            if (_cfg.StepDelayMs > 0)
                Thread.Sleep(_cfg.StepDelayMs);

            try
            {
                _barrier!.SignalAndWait(_cts.Token);
            }
            catch (OperationCanceledException) { break; }
        }
    }

    // ── Post-phase action (all particle threads paused here) ─────────────

    private void OnBarrierPhaseComplete(long phase)
    {
        var step = (int)phase + 1;
        if (step % _cfg.SnapshotEvery != 0 && step != _cfg.TotalSteps) return;

        var cells = _crystal.Snapshot();
        var total = _crystal.Total();
        var unsafeStats = (_crystal as UnsafeCrystal)?.GetStats(cells);

        SnapshotReady?.Invoke(new Snapshot(
            Step: step,
            ElapsedMs: _sw.Elapsed.TotalMilliseconds,
            Cells: cells,
            Rows: _cfg.Rows,
            Cols: _cfg.Cols,
            TotalParticles: total,
            UnsafeStats: unsafeStats,
            Mode: _cfg.Mode.ToString()));
    }

    private SimulationStats BuildStats()
    {
        var cells = _crystal.Snapshot();
        var unsafeStats = (_crystal as UnsafeCrystal)?.GetStats(cells);

        return new SimulationStats(
            TotalSteps: _cfg.TotalSteps,
            ParticleCount: _cfg.ParticleCount,
            ElapsedMs: _sw.Elapsed.TotalMilliseconds,
            FinalTotal: _crystal.Total(),
            UnsafeStats: unsafeStats,
            ExpectedTotal: _cfg.ParticleCount,
            Seed: _cfg.Seed,
            Mode: _cfg.Mode.ToString());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _barrier?.Dispose();
    }
}
