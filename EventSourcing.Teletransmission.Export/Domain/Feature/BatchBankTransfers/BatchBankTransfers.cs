using Baseline.Reflection;
using EventSourcing.Teletransmission.Export.Domain.Feature.BatchBankTransfers;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Teletransmission.Export.Domain.Feature.BatchBankTransfers
{
    public class BatchBankTransfers
    {
        public Guid IdTransmission { get; set; }
        public Guid IdUserTransmission { get; set; }
        public DateTimeOffset DateTransmission { get; set; }
        public ICollection<BankTransferToBatch> BankTransfers { get; set; } = new List<BankTransferToBatch>();
    }


    public class BatchBankTransfersHandler
    {
        private readonly IDocumentSession _session;

        public BatchBankTransfersHandler(IDocumentSession session)
        {
            _session = session;
        }

        public Task Handle(BatchBankTransfers command)
        {
            foreach (var batch in command.BankTransfers.GroupBy(bt => bt.BankId))
            {
                var idBatch = Guid.NewGuid();
                var batched = new BankTransferBatched(
                    idBatch,
                    command.IdTransmission,
                    batch.Key,
                    command.IdUserTransmission,
                    command.DateTransmission,
                    batch.Select(v => new BankTransfer(v.Amount, v.PayerId, v.RecipientId))
                );

                _session.Events.StartStream<BankTransferBatch>(idBatch, batched);
            }

            return _session.SaveChangesAsync();
        }
    }

    public class BankTransfer
    {
        public BankTransfer(decimal amount, Guid payerId, Guid recipientId)
        {
            Amount = amount;
            PayerId = payerId;
            RecipientId = recipientId;
        }

        public decimal Amount { get; }
        public Guid PayerId { get; }
        public Guid RecipientId { get; }
    }

    public class BankTransferBatch
    {
        public BankTransferBatch(Guid key, IEnumerable<BankTransfer> bankTransfers)
        {
            Id = key;
            BankTransfers = bankTransfers;
        }

        public Guid Id { get; }
        public IEnumerable<BankTransfer> BankTransfers { get; }

    }
}

    public record BankTransferBatched(Guid IdBatch, Guid IdTransmission, Guid BankId, Guid IdUserTransmission, DateTimeOffset DateTransmission, IEnumerable<BankTransfer> BankTransfers);