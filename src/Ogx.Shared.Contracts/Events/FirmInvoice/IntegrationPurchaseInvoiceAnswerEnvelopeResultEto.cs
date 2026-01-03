using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.FirmInvoice;

public sealed record IntegrationPurchaseInvoiceAnswerEnvelopeResultEto(
    Guid ReceivedQueueId,
    bool IsOperationSuccess,
    [CanBeNull] string OperationDescription,
    Guid? PurchaseInvoiceAnswerEnvelopeId,
    [NotNull] string PurchaseInvoiceAnswerEnvelopeIdentifier,
    [CanBeNull] string CustomerRegisterNumber,
    [CanBeNull] string CustomerPkAlias
) : IIntegrationEventMessage
{
    public Guid ReceivedQueueId { get; } = ReceivedQueueId;
    public bool IsOperationSuccess { get; } = IsOperationSuccess;
    [CanBeNull] public string OperationDescription { get; } = OperationDescription;
    public Guid? PurchaseInvoiceAnswerEnvelopeId { get; } = PurchaseInvoiceAnswerEnvelopeId;
    [NotNull]  public string PurchaseInvoiceAnswerEnvelopeUuid { get; } = PurchaseInvoiceAnswerEnvelopeIdentifier;
    [CanBeNull] public string CustomerRegisterNumber { get; } = CustomerRegisterNumber;
    [CanBeNull] public string CustomerPkAlias { get; } = CustomerPkAlias;
}
