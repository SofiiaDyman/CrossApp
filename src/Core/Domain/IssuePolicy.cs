namespace Core.Domain;

public static class IssuePolicy
{
    public const int MaxOpenPerRecipient = 5;

    public static void EnsureRecipientCanOpen(IEnumerable<IssueRequest> existing, string recipientId)
    {
        ArgumentNullException.ThrowIfNull(existing);
        if (string.IsNullOrWhiteSpace(recipientId))
            throw new ArgumentException("Одержувач обов'язковий", nameof(recipientId));

        int open = existing.Count(r => r.RecipientId == recipientId.Trim()
                                    && r.Status == IssueStatus.Draft);
        if (open >= MaxOpenPerRecipient)
            throw new InvalidOperationException(
                $"Одержувач {recipientId} уже має {open} відкритих заявок (максимум {MaxOpenPerRecipient})");
    }
}