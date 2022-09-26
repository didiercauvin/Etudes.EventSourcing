using EventSourcing.Teletransmission.Export.BankAccount;
using EventSourcing.Teletransmission.Export.Ecritures;
using EventSourcing.Teletransmission.Export.Ecritures.GetEcritureInfo;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json.Serialization;
using Weasel.Core;
using static Microsoft.AspNetCore.Http.Results;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMarten(options =>
    {
        var schemaName = Environment.GetEnvironmentVariable("SchemaName");
        if (!string.IsNullOrEmpty(schemaName))
        {
            options.Events.DatabaseSchemaName = schemaName;
            options.DatabaseSchemaName = schemaName;
        }
        options.Connection(builder.Configuration.GetConnectionString("Marten"));
        options.UseDefaultSerialization(EnumStorage.AsString, nonPublicMembersStorage: NonPublicMembersStorage.All);

        options.Projections.Add<EcritureInfoProjection>();
        options.Projections.Add<BankAccountShortInfoProjection>();

    }).AddAsyncDaemon(DaemonMode.Solo);

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.MapPost("api/bankaccounts",
    async (
        IDocumentSession documentSession,
        OpenBankAccountRequest body,
        CancellationToken ct) =>
    {
        var accountId = Guid.NewGuid();

        var created = BankAccountDomainService.Handle(new OpenBankAccount(accountId, body.Name));
        documentSession.Events.StartStream<BankAccount>(accountId, created);

        await documentSession.SaveChangesAsync();

        return Created($"/api/bankaccounts/{accountId}", accountId);
    }
).WithTags("BankAccounts");

app.MapPost("api/bankaccounts/{accountId:guid}/credit",
    async (
        IDocumentSession documentSession,
        Guid accountId,
        CreditFundsRequest body,
        CancellationToken ct) =>
    {
        await documentSession.Events.WriteToAggregate<BankAccount>(accountId, stream =>
            stream.AppendOne(BankAccountDomainService.Handle(stream.Aggregate, new CreditFund(accountId, body.Amount))), ct);
    }
).WithTags("BankAccounts");

app.MapPost("api/bankaccounts/{accountId:guid}/debit",
    async (
        IDocumentSession documentSession,
        Guid accountId,
        DepositFundsRequest body,
        CancellationToken ct) =>
    {
        await documentSession.Events.WriteToAggregate<BankAccount>(accountId, stream =>
            stream.AppendOne(BankAccountDomainService.Handle(stream.Aggregate, new DepositFund(accountId, body.Amount))), ct);
    }
).WithTags("BankAccounts");

app.MapPost("api/bankaccounts/{accountId:guid}/close",
    async (
        IDocumentSession documentSession,
        Guid accountId,
        CancellationToken ct) =>
    {
        await documentSession.Events.WriteToAggregate<BankAccount>(accountId, stream =>
            stream.AppendOne(BankAccountDomainService.Handle(stream.Aggregate, new CloseBankAccount(accountId))), ct);
    }
).WithTags("BankAccounts");

app.MapGet("api/bankaccounts",
    (IQuerySession querySession, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
            CancellationToken ct) =>
        querySession.Query<BankAccountShortInfo>()
            .ToPagedListAsync(pageNumber ?? 1, pageSize ?? 10, ct)
).WithTags("BankAccounts");



app.MapPost("api/ecritures",
    async (
        IDocumentSession documentSession,
        CreateEcritureRequest body,
        CancellationToken ct) =>
    {
        var ecritureId = Guid.NewGuid();

        var created = EcrituresDomainServices.Handle(new CreateEcriture(ecritureId, body.Libelle, body.Montant, body.Destinataire));
        documentSession.Events.StartStream<Ecriture>(ecritureId, created);

        await documentSession.SaveChangesAsync();

        return Created($"/api/ecritures/{ecritureId}", ecritureId);
    }
).WithTags("ecritures");

app.MapGet("api/ecritures",
    (IQuerySession querySession, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
            CancellationToken ct) =>
        querySession.Query<EcritureInfo>()
            .ToPagedListAsync(pageNumber ?? 1, pageSize ?? 10, ct)
).WithTags("Ecritures");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();

public record OpenBankAccountRequest(string Name);
public record DepositFundsRequest(decimal Amount);
public record CreditFundsRequest(decimal Amount);

public record CreateEcritureRequest(string Libelle, decimal Montant, string Destinataire);
