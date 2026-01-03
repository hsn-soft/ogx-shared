using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.FirmInvoice;

public sealed record IntegrationPurchaseInvoiceAnswerPreparedEto(
    Guid ReceivedQueueId,
    Guid TenantId,
    Guid ClientId,
    [NotNull] string PreparedFilePath,
    [NotNull] string PreparedFileName,
    [NotNull] string  PurchaseInvoiceAnswerIdentifier,
    bool SendWithoutApprove,
    [CanBeNull] string CustomerPkAlias,
    [CanBeNull] string UniqueIntegrationCode
) : IIntegrationEventMessage
{
    public Guid ReceivedQueueId { get; } = ReceivedQueueId;
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;
    [NotNull] public string PreparedFilePath { get; } = PreparedFilePath;
    [NotNull] public string PreparedFileName { get; } = PreparedFileName;
    [NotNull] public string PurchaseInvoiceAnswerIdentifier { get; } = PurchaseInvoiceAnswerIdentifier;
    public bool SendWithoutApprove { get; } = SendWithoutApprove;
    [CanBeNull] public string CustomerPkAlias { get; } = CustomerPkAlias;
    [CanBeNull] public string UniqueIntegrationCode { get; } = UniqueIntegrationCode;
}