// <copyright file="RegistryInvoiceRemovedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class RegistryInvoiceRemovedEventHandler : IEventHandler
{
    private readonly ILogger<RegistryInvoiceRemovedEventHandler> _logger;
    private readonly IInvoiceRepository _invoiceRepository;

    public RegistryInvoiceRemovedEventHandler(
        ILogger<RegistryInvoiceRemovedEventHandler> logger,
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
                return;
            }

            RegistryInvoiceRemovedEvent? @event;

            try
            {
                @event = JsonConvert.DeserializeObject<RegistryInvoiceRemovedEvent>(message);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Erreur lors de la désérialisation du message RegistryInvoiceRemovedEvent", ex);
            }

            if (@event?.Data == null || string.IsNullOrWhiteSpace(@event.Data.InvoiceNumber))
            {
                return;
            }

            // Suppression idempotente par InvoiceNumber
            await this._invoiceRepository.RemoveByInvoiceNumberAsync(@event.Data.InvoiceNumber);

            this._logger.LogInformation(
                "Facture supprimée ou ignorée (idempotence) avec InvoiceNumber: {InvoiceNumber}",
                @event.Data.InvoiceNumber);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Erreur inattendue dans RegistryInvoiceRemovedEventHandler.HandleAsync");
            throw;
        }
    }
}
