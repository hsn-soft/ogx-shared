using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Ogx.Shared.Contracts.Events.GibInvoice;

public sealed record SendGibSysQueueResultEto(
    Guid ReceivedGibDocQueueId,
    Guid SendGibSysQueueId,
    [NotNull] string RefEnvelopeGibResultCode,
    [NotNull] string RefEnvelopeGibResultDescription,
    bool IsOperationSuccess,
    [CanBeNull] string OperationDescription
) : IIntegrationEventMessage
{
    public Guid ReceivedGibDocQueueId { get; } = ReceivedGibDocQueueId;
    public Guid SendGibSysQueueId { get; } = SendGibSysQueueId;
    [NotNull]  public string RefEnvelopeGibResultCode { get; } = RefEnvelopeGibResultCode;
    [NotNull]  public string RefEnvelopeGibResultDescription { get; } = RefEnvelopeGibResultDescription;
    public bool IsOperationSuccess { get; } = IsOperationSuccess;
    [CanBeNull] public string OperationDescription { get; } = OperationDescription;
}