using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Teletransmission.Export.Ecritures
{
    public static class EcrituresDomainServices
    {
        public static EcritureCreated Handle(CreateEcriture command)
        {
            return new EcritureCreated(command.Id, command.Libelle, command.Montant, command.Destinataire);
        }

        
    }

   

    public record CreateEcriture(Guid Id, string Libelle, decimal Montant, string Destinataire);
}
