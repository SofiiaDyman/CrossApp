namespace Core.Dto;

public sealed record IssueRequestDto(
    string Id,
    string ProductId,
    string RecipientId,
    int Quantity,
    DateOnly CreatedOn,
    string Status);