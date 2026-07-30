// <copyright file="RegistryInvoiceCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class RegistryInvoiceCreatedEventHandler : IEventHandler
{
    private readonly ILogger<RegistryInvoiceCreatedEventHandler> _logger;
    private readonly IInvoiceRepository _invoiceRepository;

    public RegistryInvoiceCreatedEventHandler(
        ILogger<RegistryInvoiceCreatedEventHandler> logger,
        IInvoiceRepository invoiceRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(invoiceRepository);
        this._logger = logger;
        this._invoiceRepository = invoiceRepository;
    }

    public async Task HandleAsync(string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                this._logger.LogWarning("Message est null ou vide");
                return;
            }

            RegistryInvoiceCreatedEvent? @event;

            try
            {
                @event = JsonConvert.DeserializeObject<RegistryInvoiceCreatedEvent>(message);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Erreur lors de la désérialisation du message RegistryInvoiceCreatedEvent", ex);
            }

            // Validation détaillée avec logs
            if (@event?.Data == null)
            {
                this._logger.LogError("Event data is null - cannot process message");
                return;
            }

            if (string.IsNullOrWhiteSpace(@event.Data.InvoiceNumber))
            {
                this._logger.LogError("InvoiceNumber is null or empty - cannot process message");
                return;
            }

            if (string.IsNullOrWhiteSpace(@event.Data.AccountNumber))
            {
                this._logger.LogError(
                    "AccountNumber is null or empty - cannot process message. InvoiceNumber: {InvoiceNumber}",
                    @event.Data.InvoiceNumber);
                return;
            }

            // Vérifier l'idempotence par InvoiceNumber
            if (await this._invoiceRepository.ExistsByInvoiceNumberAsync(@event.Data.InvoiceNumber))
            {
                this._logger.LogInformation(
                    "La facture avec le numéro suivant: {InvoiceNumber} existe déjà (idempotence).",
                    @event.Data.InvoiceNumber);
                return;
            }

            // Résoudre AccountNumber en AccountId
            var accountId = await this._invoiceRepository.GetAccountIdByAccountNumberAsync(@event.Data.AccountNumber);

            if (accountId == null)
            {
                this._logger.LogWarning(
                    "Compte introuvable pour AccountNumber: {AccountNumber}, InvoiceNumber: {InvoiceNumber}",
                    @event.Data.AccountNumber,
                    @event.Data.InvoiceNumber);
                return;
            }

            // Insérer la facture
            var request = new CreateInvoiceRequest
            {
                InvoiceNumber = @event.Data.InvoiceNumber,
                Name = @event.Data.Name ?? string.Empty,
                InvoiceDate = @event.Data.InvoiceDate,
                DepositDate = @event.Data.DepositDate,
                AccountId = accountId.Value
            };

            await this._invoiceRepository.AddAsync(request);

            this._logger.LogInformation(
                "Facture créée avec InvoiceNumber: {InvoiceNumber}",
                @event.Data.InvoiceNumber);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Erreur inattendue dans RegistryInvoiceCreatedEventHandler.HandleAsync");
            throw;
        }
    }
}
