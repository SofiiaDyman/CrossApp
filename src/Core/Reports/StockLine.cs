namespace Core.Reports;

//описує один рядок звіту про товар
public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);
