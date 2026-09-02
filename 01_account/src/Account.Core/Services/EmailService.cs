// <copyright file="EmailService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Options;
using Notifications.Commons.WebApi.QueryParams;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Email;
using Pulse.Account.Core.Options;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Core.Services;

public class EmailService(INotificationManager notificationManager, IOptions<EmailOptions> options, IContactRepository contactRepository, IAccountRepository accountRepository) : IEmailService
{
    private readonly INotificationManager _notificationManager = notificationManager;
    private readonly EmailOptions _options = options.Value;
    private readonly IContactRepository _contactRepository = contactRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task SendDelegationRequestEmailsAsync(IEnumerable<int> recipientIds, int requestorId, int accountId)
    {
        if (!recipientIds.Any())
        {
            return;
        }

        var requestor = await _contactRepository.GetContactByIdAsync(requestorId);
        var account = await _accountRepository.GetAccountAsync(accountId);

        foreach (var recipientId in recipientIds)
        {
            var contact = await _contactRepository.GetContactByIdAsync(recipientId);
            var context = new RequestEmailContext
            {
                RecipientEmail = contact.Email,
                UserFirstName = contact.FirstName,
                UserLastName = contact.LastName,
                RequestorFirstName = requestor.FirstName,
                RequestorLastName = requestor.LastName,
                RequestorEmail = requestor.Email,
                AccountNumber = account.AccountNumber,
                LegalName = account.Legal.LegalName,
                Date = DateTime.UtcNow,
            };

            var variables = BuildRequestVariables(context);
            var request = BuildEmailRequest(context.RecipientEmail, _options.DelegationRequestEmailTemplate, variables);

            await PublishEmailAsync(request);
        }
    }

    private Dictionary<string, object> BuildRequestVariables(RequestEmailContext context)
    {
        var userName = BuildUserName(context.UserFirstName, context.UserLastName);
        var requestorName = BuildUserName(context.RequestorFirstName, context.RequestorLastName);

        return new Dictionary<string, object>
        {
            { "userName", userName },
            { "requestorName", requestorName },
            { "requestorEmail", context.RequestorEmail },
            { "legalName", context.LegalName },
            { "accountNumber", context.AccountNumber },
            { "date", context.Date.ToString("dd/MM/yyyy") },
            { "time", context.Date.ToString("HH:mm") },
            { "delegationRequestUrl", $"{_options.WalletBaseUrl}/redirect?action=open-delegation-drawer" },
        };
    }

    private static string BuildUserName(string? firstName, string? lastName)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            parts.Add(firstName.Trim().ToCapitalize());
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            parts.Add(lastName.Trim().ToCapitalize());
        }

        return parts.Count > 0 ? string.Join(" ", parts) : string.Empty;
    }

    private EmailRequest BuildEmailRequest(string recipientEmail, string templateName, Dictionary<string, object> variables)
    {
        return new EmailRequest
        {
            From = _options.SenderEmail,
            To = new List<string> { recipientEmail },
            TemplateName = templateName,
            Variables = variables
        };
    }

    private async Task PublishEmailAsync(EmailRequest emailRequest)
    {
        var emailEvent = new EmailOnlySenderEvent(emailRequest);
        await _notificationManager.PublishAsync(emailEvent, _options.ServiceBusTopic);
    }
}
