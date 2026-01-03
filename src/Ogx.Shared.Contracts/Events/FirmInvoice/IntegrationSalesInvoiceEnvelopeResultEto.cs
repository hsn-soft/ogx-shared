using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.FirmInvoice;

public sealed record IntegrationSalesInvoiceEnvelopeResultEto(
    Guid ReceivedQueueId,
    bool IsOperationSuccess,
    [CanBeNull] string OperationDescription,
    Guid? SalesInvoiceEnvelopeId,
    [NotNull] string  SalesInvoiceEnvelopeIdentifier,
    [CanBeNull] string CustomerRegisterNumber,
    [CanBeNull] string CustomerPkAlias
) : IIntegrationEventMessage
{
    public Guid ReceivedQueueId { get; } = ReceivedQueueId;
    public bool IsOperationSuccess { get; } = IsOperationSuccess;
    [CanBeNull] public string OperationDescription { get; } = OperationDescription;
    public Guid? SalesInvoiceEnvelopeId { get; } = SalesInvoiceEnvelopeId;
    [NotNull] public string SalesInvoiceEnvelopeIdentifier { get; } = SalesInvoiceEnvelopeIdentifier;
    [CanBeNull] public string CustomerRegisterNumber { get; } = CustomerRegisterNumber;
    [CanBeNull] public string CustomerPkAlias { get; } = CustomerPkAlias;
}