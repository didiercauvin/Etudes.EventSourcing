using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Teletransmission.Export.BankAccount
{
    public record BankAccountOpened(Guid Id, string Name);
    public record FundDebited(Guid Id, decimal Amount);
    public record AmountDebited(Guid Id, decimal Amount, decimal Balance);
    public record AmountCredited(Guid Id, decimal Amount, decimal Balance);
    public record BankAccountClosed(Guid Id);

    public record BankAccount(
        Guid Id,
        string Name,
        decimal Balance = 0.0M,
        bool IsOpened = true)
    {
        public static BankAccount Create(BankAccountOpened @event) =>
            new(@event.Id, @event.Name);

        public BankAccount Apply(FundDebited @event) =>
            this with { Balance = Balance - @event.Amount };

        public BankAccount Apply(AmountDebited @event) =>
            this with { Balance = @event.Balance };

        public BankAccount Apply(AmountCredited @event) =>
            this with { Balance = @event.Balance };

        public BankAccount Apply(BankAccountClosed @event) =>
            this with { IsOpened = false };
    }
}
