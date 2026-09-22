namespace Core.Dto;

public record SupplierDto(
    string Id,
    string Name,
    string? Phone = null);