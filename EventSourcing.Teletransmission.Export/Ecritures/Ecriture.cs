using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Teletransmission.Export.Ecritures
{
    public record EcritureCreated(Guid Id, string Libelle, decimal Montant, string Destinataire);
    
    public record Ecriture(Guid Id, string Libelle, decimal Montant, string Destinataire)
    {
        public static Ecriture Create(EcritureCreated @event) => new(@event.Id, @event.Libelle, @event.Montant, @event.Destinataire);
    }
}
