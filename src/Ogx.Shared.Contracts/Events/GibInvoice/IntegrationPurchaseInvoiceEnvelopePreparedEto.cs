using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.GibInvoice;

public sealed record IntegrationPurchaseInvoiceEnvelopePreparedEto(
    Guid ReceivedGibDocQueueId,
    Guid TenantId,
    Guid ClientId,
    [NotNull] string PreparedFilePath,
    [NotNull] string PreparedFileName,
    [NotNull] string  PurchaseInvoiceEnvelopeIdentifier,
    [NotNull] string  SenderRegisterNumber,
    [NotNull] string  SenderAlias,
    [NotNull] string  SenderTitle,
    [NotNull] string  ReceiverRegisterNumber,
    [NotNull] string  ReceiverAlias,
    [NotNull] string  ReceiverTitle

) : IIntegrationEventMessage
{
    public Guid ReceivedGibDocQueueId { get; } = ReceivedGibDocQueueId;
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;
    [NotNull] public string PreparedFilePath { get; } = PreparedFilePath;
    [NotNull] public string PreparedFileName { get; } = PreparedFileName;
    [NotNull] public string PurchaseInvoiceEnvelopeIdentifier { get; } = PurchaseInvoiceEnvelopeIdentifier;
    [NotNull] public string SenderRegisterNumber { get; } = SenderRegisterNumber;
    [NotNull] public string SenderAlias { get; } = SenderAlias;
    [NotNull] public string SenderTitle { get; } = SenderTitle;
    [NotNull] public string ReceiverRegisterNumber { get; } = ReceiverRegisterNumber;
    [NotNull] public string ReceiverAlias { get; } = ReceiverAlias;
    [NotNull] public string ReceiverTitle { get; } = ReceiverTitle;
}