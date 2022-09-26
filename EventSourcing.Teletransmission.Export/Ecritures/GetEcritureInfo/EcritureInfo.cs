using Marten.Events.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Teletransmission.Export.Ecritures.GetEcritureInfo
{
    public record EcritureInfo(Guid Id, string Libelle, decimal Montant, string Destinataire);

    public class EcritureInfoProjection : SingleStreamAggregation<EcritureInfo>
    {
        public static EcritureInfo Create(EcritureCreated @event) =>
            new(@event.Id, @event.Libelle, @event.Montant, @event.Destinataire);
    }
}
