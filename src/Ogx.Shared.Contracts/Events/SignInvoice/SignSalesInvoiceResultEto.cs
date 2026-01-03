using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.SignInvoice;

public sealed record SignSalesInvoiceResultEto(
    Guid SalesInvoiceId,
    bool IsOperationSuccess,
    [CanBeNull] string OperationDescription,
    [NotNull] string SalesInvoiceIdentifier,
    long SignedFileSize
) : IIntegrationEventMessage
{
    public Guid SalesInvoiceId { get; } = SalesInvoiceId;
    public bool IsOperationSuccess { get; } = IsOperationSuccess;
    [CanBeNull] public string OperationDescription { get; } = OperationDescription;
    [NotNull] public string SalesInvoiceIdentifier { get; } = SalesInvoiceIdentifier;
    public long SignedFileSize { get; } = SignedFileSize;
}