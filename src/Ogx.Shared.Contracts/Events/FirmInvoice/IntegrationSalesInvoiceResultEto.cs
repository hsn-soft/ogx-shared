using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.FirmInvoice;

public sealed record IntegrationSalesInvoiceResultEto(
    Guid ReceivedQueueId,
    bool IsOperationSuccess,
    [CanBeNull] string OperationDescription,
    Guid? SalesInvoiceId,
    [NotNull] string SalesInvoiceIdentifier,
    [CanBeNull] string CustomerRegisterNumber,
    [CanBeNull] string CustomerPkAlias
) : IIntegrationEventMessage
{
    public Guid ReceivedQueueId { get; } = ReceivedQueueId;
    public bool IsOperationSuccess { get; } = IsOperationSuccess;
    [CanBeNull] public string OperationDescription { get; } = OperationDescription;
    public Guid? SalesInvoiceId { get; } = SalesInvoiceId;
    [NotNull] public string SalesInvoiceIdentifier { get; } = SalesInvoiceIdentifier;
    [CanBeNull] public string CustomerRegisterNumber { get; } = CustomerRegisterNumber;
    [CanBeNull] public string CustomerPkAlias { get; } = CustomerPkAlias;
}