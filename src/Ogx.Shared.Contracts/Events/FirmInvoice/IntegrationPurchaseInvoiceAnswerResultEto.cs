using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.FirmInvoice;

public sealed record IntegrationPurchaseInvoiceAnswerResultEto(
    Guid ReceivedQueueId,
    bool IsOperationSuccess,
    [CanBeNull] string OperationDescription,
    Guid? PurchaseInvoiceAnswerId,
    [NotNull] string  PurchaseInvoiceAnswerIdentifier,
    [CanBeNull] string CustomerRegisterNumber,
    [CanBeNull] string CustomerPkAlias
) : IIntegrationEventMessage
{
    public Guid ReceivedQueueId { get; } = ReceivedQueueId;
    public bool IsOperationSuccess { get; } = IsOperationSuccess;
    [CanBeNull] public string OperationDescription { get; } = OperationDescription;
    public Guid? PurchaseInvoiceAnswerId { get; } = PurchaseInvoiceAnswerId;
    [NotNull] public string PurchaseInvoiceAnswerIdentifier { get; } = PurchaseInvoiceAnswerIdentifier;
    [CanBeNull] public string CustomerRegisterNumber { get; } = CustomerRegisterNumber;
    [CanBeNull] public string CustomerPkAlias { get; } = CustomerPkAlias;
}