using Core.Dto;

namespace Core.Domain;

public sealed class IssueRequest
{
    public string Id { get; }
    public Product Product { get; }
    public string RecipientId { get; }
    public int Quantity { get; }
    public DateOnly CreatedOn { get; }
    public IssueStatus Status { get; private set; }

    private IssueRequest(string id, Product product, string recipientId,
                         int quantity, DateOnly createdOn)
    {
        Id = id;
        Product = product;
        RecipientId = recipientId;
        Quantity = quantity;
        CreatedOn = createdOn;
        Status = IssueStatus.Draft;
    }

    public static IssueRequest Create(string id, Product product, string recipientId,
                                      int quantity, DateOnly createdOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Номер заявки обов'язковий", nameof(id));
        ArgumentNullException.ThrowIfNull(product);
        if (string.IsNullOrWhiteSpace(recipientId))
            throw new ArgumentException("Одержувач обов'язковий", nameof(recipientId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Кількість у заявці має бути більшою за нуль");

        return new IssueRequest(id.Trim(), product, recipientId.Trim(), quantity, createdOn);
    }

    public void Confirm()
    {
        EnsureTransition(IssueStatus.Confirmed);
        Product.Issue(Quantity);
        Status = IssueStatus.Confirmed;
    }

    public void Cancel()
    {
        EnsureTransition(IssueStatus.Cancelled);
        Status = IssueStatus.Cancelled;
    }

    private void EnsureTransition(IssueStatus target)
    {
        bool allowed = (Status, target) switch
        {
            (IssueStatus.Draft, IssueStatus.Confirmed) => true,
            (IssueStatus.Draft, IssueStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Заявка {Id}: перехід {Status} → {target} неможливий");
    }

    public IssueRequestDto ToDto() =>
        new(Id, Product.Id, RecipientId, Quantity, CreatedOn, Status.ToString());

    public static IssueRequest FromDto(IssueRequestDto dto, Product product)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(product);

        if (product.Id != dto.ProductId)
            throw new ArgumentException(
                $"Заявка {dto.Id} стосується товару {dto.ProductId}, а передано {product.Id}",
                nameof(product));
        if (!Enum.TryParse<IssueStatus>(dto.Status, out var status) || !Enum.IsDefined(status))
            throw new ArgumentException($"Невідомий статус заявки '{dto.Status}'", nameof(dto));

        var request = Create(dto.Id, product, dto.RecipientId, dto.Quantity, dto.CreatedOn);
        request.Status = status;
        return request;
    }
}