using Marten.Events.Aggregation;

namespace EventSourcing.Teletransmission.Export.Domain.Feature.BatchBankTransfers
{
    public record BankTransferToBatch(decimal Amount, Guid BankId, Guid PayerId, Guid RecipientId);
}