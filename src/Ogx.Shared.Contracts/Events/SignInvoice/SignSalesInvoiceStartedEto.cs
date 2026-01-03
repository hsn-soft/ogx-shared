using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.SignInvoice;

public sealed record SignSalesInvoiceStartedEto(
    Guid SalesInvoiceId,
    Guid TenantId,
    Guid ClientId,
    [NotNull] string UnsignedFilePath,
    [NotNull] string UnsignedFileName,
    [NotNull] string SalesInvoiceIdentifier,
    [NotNull] string SalesInvoiceNumber
) : IIntegrationEventMessage
{
    public Guid SalesInvoiceId { get; } = SalesInvoiceId;
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;
    [NotNull] public string UnsignedFilePath { get; } = UnsignedFilePath;
    [NotNull] public string UnsignedFileName { get; } = UnsignedFileName;
    [NotNull] public string SalesInvoiceIdentifier { get; } = SalesInvoiceIdentifier;
    [NotNull] public string SalesInvoiceNumber { get; } = SalesInvoiceNumber;
}