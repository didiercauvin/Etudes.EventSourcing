using Marten.Events.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Teletransmission.Export.BankAccount
{
    public record BankAccountShortInfo(Guid Id, string Name, decimal Balance, string Status);

    public class BankAccountShortInfoProjection : SingleStreamAggregation<BankAccountShortInfo>
    {
        public static BankAccountShortInfo Create(BankAccountOpened @event) =>
            new(@event.Id, @event.Name, 0.0M, "Open");

        public BankAccountShortInfo Apply(FundDebited @event, BankAccountShortInfo current) =>
            current with { Balance = current.Balance - @event.Amount };

        public BankAccountShortInfo Apply(AmountDebited @event, BankAccountShortInfo current) =>
            current with { Balance = @event.Balance };

        public BankAccountShortInfo Apply(AmountCredited @event, BankAccountShortInfo current) =>
            current with { Balance = @event.Balance };

        public BankAccountShortInfo Apply(BankAccountClosed @event, BankAccountShortInfo current) =>
            current with { Id = @event.Id, Status = "Close" };
    }
}
