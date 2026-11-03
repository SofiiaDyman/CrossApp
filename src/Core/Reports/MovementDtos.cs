namespace Core.Reports;

public sealed record MovementLine(string Sku, string Name, int Delta, DateOnly Date);
public sealed record MovementSummary(string Sku, string Name, int MovementsCount, int NetDelta);
