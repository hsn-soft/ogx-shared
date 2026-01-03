using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.GibInvoice;

public sealed record IntegrationSysResEnvelopeReceivedEto(
    Guid ReceivedGibSysQueueId,
    Guid TenantId,
    Guid ClientId,
    [NotNull] string SystemResponseEnvelopeIdentifier,
    DateTime SystemResponseEnvelopeCreationTime,
    [NotNull] string SystemResponseDocumentIdentifier,
    bool IsCenterSystemResponse,
    [NotNull] string ReferenceEnvelopeIdentifier,
    [NotNull] string ReferenceDocumentType,
    [NotNull] string ReferenceGibStatusCode,
    [NotNull] string ReferenceGibStatusDescription,
    [CanBeNull] string ReferenceGTBRefNumber
) : IIntegrationEventMessage
{
    public Guid ReceivedGibSysQueueId { get; } = ReceivedGibSysQueueId;
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;
    [NotNull] public string SystemResponseEnvelopeIdentifier { get; } = SystemResponseEnvelopeIdentifier;
    public DateTime SystemResponseEnvelopeCreationTime { get; } = SystemResponseEnvelopeCreationTime;
    [NotNull] public string SystemResponseDocumentIdentifier { get; } = SystemResponseDocumentIdentifier;
    public bool IsCenterSystemResponse { get; } = IsCenterSystemResponse;
    [NotNull] public string ReferenceEnvelopeIdentifier { get; } = ReferenceEnvelopeIdentifier;
    [NotNull] public string ReferenceDocumentType { get; } = ReferenceDocumentType;
    [NotNull] public string ReferenceGibStatusCode { get; } = ReferenceGibStatusCode;
    [NotNull] public string ReferenceGibStatusDescription { get; } = ReferenceGibStatusDescription;
    [CanBeNull] public string ReferenceGTBRefNumber { get; } = ReferenceGTBRefNumber;
}