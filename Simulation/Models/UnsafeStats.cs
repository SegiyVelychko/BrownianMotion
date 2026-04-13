namespace BrownianMotion.Simulation.Models;

/// <summary>
/// Race-condition diagnostics captured from an <c>UnsafeCrystal</c> snapshot.
/// Any <see cref="NegativeCells"/> value greater than zero is physical proof
/// of a lost update: a cell cannot hold fewer than zero particles in reality.
/// </summary>
/// <param name="RawSum">
/// Algebraic sum of all cell values, including negatives.
/// Differs from the initial particle count when updates have been lost.
/// </param>
/// <param name="NegativeCells">
/// Number of cells whose value is below zero — direct evidence of underflow
/// caused by concurrent unsynchronised decrements.
/// </param>
/// <param name="MinCell">
/// The most negative cell value encountered, showing the worst-case
/// lost-update depth.
/// </param>
public sealed record UnsafeStats(int RawSum, int NegativeCells, int MinCell);
