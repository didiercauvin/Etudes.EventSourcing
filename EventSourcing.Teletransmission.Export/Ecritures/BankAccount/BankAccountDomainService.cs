using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Teletransmission.Export.Ecritures.BankAccount
{
    public static class BankAccountDomainService
    {
        public static BankAccountOpened Handle(OpenBankAccount @event)
         => new BankAccountOpened(@event.Id, @event.Name);

        public static AmountDebited Handle(BankAccount current, DepositFund command)
        {
            if (!current.IsOpened)
            {
                throw new Exception("Compte fermé");
            }

            return new AmountDebited(command.AccountId, command.Amount, current.Balance - command.Amount);
        }

        public static AmountCredited Handle(BankAccount current, CreditFund command)
        {
            if (!current.IsOpened)
            {
                throw new Exception("Compte fermé");
            }

            return new AmountCredited(command.AccountId, command.Amount, current.Balance + command.Amount);
        }

        public static BankAccountClosed Handle(BankAccount current, CloseBankAccount command)
            => new BankAccountClosed(current.Id);

    }

    public record OpenBankAccount(Guid Id, string Name);
    public record DepositFund(Guid AccountId, decimal Amount);
    public record CreditFund(Guid AccountId, decimal Amount);
    public record CloseBankAccount(Guid AccountId);
}
