namespace BrownianMotion.Simulation.Models;

/// <summary>
/// Immutable value object carrying all parameters for a single simulation run.
/// Validation is performed at construction time; an invalid config cannot exist.
/// </summary>
public sealed record SimulationConfig
{
    /// <summary>Number of rows in the crystal grid. Must be &gt; 0.</summary>
    public required int Rows { get; init; }

    /// <summary>Number of columns in the crystal grid. Must be &gt; 0.</summary>
    public required int Cols { get; init; }

    /// <summary>Number of particles (and threads) to simulate. Must be &gt; 0.</summary>
    public required int ParticleCount { get; init; }

    /// <summary>Total number of simulation steps to execute. Must be &gt; 0.</summary>
    public required int TotalSteps { get; init; }

    /// <summary>A snapshot is taken every this many steps. Must be &gt; 0.</summary>
    public required int SnapshotEvery { get; init; }

    /// <summary>
    /// Artificial delay in milliseconds between steps, used to slow down
    /// visualisation. Zero disables the delay.
    /// </summary>
    public required int StepDelayMs { get; init; }

    /// <summary>
    /// Optional RNG seed. When set, each particle derives a deterministic seed
    /// from this value and its own ID, making the run fully reproducible.
    /// <c>null</c> produces a non-reproducible run.
    /// </summary>
    public int? Seed { get; init; }

    /// <summary>Probability of moving up. Must be in [0, 1].</summary>
    public required double ProbUp { get; init; }

    /// <summary>Probability of moving down. Must be in [0, 1].</summary>
    public required double ProbDown { get; init; }

    /// <summary>Probability of moving left. Must be in [0, 1].</summary>
    public required double ProbLeft { get; init; }

    /// <summary>Probability of moving right. Must be in [0, 1].</summary>
    public required double ProbRight { get; init; }

    /// <summary>Determines which crystal implementation the engine instantiates.</summary>
    public required SimulationMode Mode { get; init; }

    /// <summary>
    /// Validates all properties and throws <see cref="ArgumentException"/>
    /// describing every violated constraint.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more properties are invalid.
    /// </exception>
    public SimulationConfig Validate()
    {
        var errors = new List<string>();

        if (Rows          <= 0) errors.Add($"{nameof(Rows)} must be > 0.");
        if (Cols          <= 0) errors.Add($"{nameof(Cols)} must be > 0.");
        if (ParticleCount <= 0) errors.Add($"{nameof(ParticleCount)} must be > 0.");
        if (TotalSteps    <= 0) errors.Add($"{nameof(TotalSteps)} must be > 0.");
        if (SnapshotEvery <= 0) errors.Add($"{nameof(SnapshotEvery)} must be > 0.");
        if (StepDelayMs   <  0) errors.Add($"{nameof(StepDelayMs)} must be >= 0.");

        if (ProbUp    < 0 || ProbUp    > 1) errors.Add($"{nameof(ProbUp)} must be in [0,1].");
        if (ProbDown  < 0 || ProbDown  > 1) errors.Add($"{nameof(ProbDown)} must be in [0,1].");
        if (ProbLeft  < 0 || ProbLeft  > 1) errors.Add($"{nameof(ProbLeft)} must be in [0,1].");
        if (ProbRight < 0 || ProbRight > 1) errors.Add($"{nameof(ProbRight)} must be in [0,1].");

        var probSum = ProbUp + ProbDown + ProbLeft + ProbRight;
        if (Math.Abs(probSum - 1.0) > 1e-9)
            errors.Add($"Transition probabilities must sum to 1.0 (got {probSum:F6}).");

        if (errors.Count > 0)
            throw new ArgumentException(
                "Invalid simulation configuration:\n" + string.Join("\n", errors));

        return this;
    }
}
