using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.GibInvoice;

public sealed record CreationSendGibSysQueueEto(
    Guid ReceivedGibDocQueueId,
    Guid TenantId,
    Guid ClientId,
    [NotNull] string  RefEnvelopeDirectionType,
    [NotNull] string  RefEnvelopeIdentifier,
    [NotNull] string  RefEnvelopeSenderRegisterNumber,
    [NotNull] string  RefEnvelopeSenderAlias,
    [NotNull] string  RefEnvelopeSenderTitle,
    [NotNull] string  RefEnvelopeReceiverRegisterNumber,
    [NotNull] string  RefEnvelopeReceiverAlias,
    [NotNull] string  RefEnvelopeReceiverTitle,
    Guid CreationSendGibSysQueueId,
    [NotNull] string CreationSendGibSysQueueResponseCode,
    [NotNull] string CreationSendGibSysQueueResponseDescription

) : IIntegrationEventMessage
{
    public Guid ReceivedGibDocQueueId { get; } = ReceivedGibDocQueueId;
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;
    [NotNull] public string RefEnvelopeDirectionType { get; } = RefEnvelopeDirectionType;
    [NotNull] public string RefEnvelopeIdentifier { get; } = RefEnvelopeIdentifier;
    [NotNull] public string RefEnvelopeSenderRegisterNumber { get; } = RefEnvelopeSenderRegisterNumber;
    [NotNull] public string RefEnvelopeSenderAlias { get; } = RefEnvelopeSenderAlias;
    [NotNull] public string RefEnvelopeSenderTitle { get; } = RefEnvelopeSenderTitle;
    [NotNull] public string RefEnvelopeReceiverRegisterNumber { get; } = RefEnvelopeReceiverRegisterNumber;
    [NotNull] public string RefEnvelopeReceiverAlias { get; } = RefEnvelopeReceiverAlias;
    [NotNull] public string RefEnvelopeReceiverTitle { get; } = RefEnvelopeReceiverTitle;
    public Guid CreationSendGibSysQueueId { get; } = CreationSendGibSysQueueId;
    [NotNull] public string CreationSendGibSysQueueResponseCode { get; } = CreationSendGibSysQueueResponseCode;
    [NotNull] public string CreationSendGibSysQueueResponseDescription { get; } = CreationSendGibSysQueueResponseDescription;
}