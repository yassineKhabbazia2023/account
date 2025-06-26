// <copyright file="StatisticsService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using ClosedXML.Excel;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepository _statisticsRepository;

        public StatisticsService(IStatisticsRepository statisticsRepository)
        {
            _statisticsRepository = statisticsRepository;
        }

        public async Task<Statistics> GetStatisticsAsync(int contactId)
        {
            return await _statisticsRepository.GetStatisticsAsync(contactId);
        }

        public async Task<double> GetAccountPercentageCustomerRelationAsync()
        {
            return await _statisticsRepository.GetAccountPercentageCustomerRelationAsync();
        }

        public async Task<byte[]> GetAccountAndClientIndicatorsAsync()
        {
            var clientsPerAccountCount = await _statisticsRepository.GetClientsPerAccountCountAsync();
            var accountsPerClientCount = await _statisticsRepository.GetAccountsPerClientCountAsync();

            using var workbook = new XLWorkbook();
            var ws1 = workbook.Worksheets.Add("Clients par dossiers");
            var table1 = ws1.Cell(1, 1).InsertTable(clientsPerAccountCount.Select(x => new
            {
                AccountCount = x.AccountCount,
                ClientCount = x.ClientCount,
                Description = $"{x.AccountCount} dossier(s) ont {x.ClientCount} client(s) rattaché(s)"
            }).ToList());

            table1.Theme = XLTableTheme.TableStyleMedium9;
            table1.ShowAutoFilter = false;
            table1.Field("AccountCount").Name = "Nombre de dossiers";
            table1.Field("ClientCount").Name = "Nombre de clients";
            ws1.Columns().AdjustToContents();

            var ws2 = workbook.Worksheets.Add("Entités par clients");
            var table2 = ws2.Cell(1, 1).InsertTable(accountsPerClientCount.Select(x => new
            {
                ClientCount = x.ClientCount,
                AccountCount = x.AccountCount,
                Description = $"{x.ClientCount} utilisateur(s) ont {x.AccountCount} entité(s)"
            }).ToList());

            table2.Theme = XLTableTheme.TableStyleMedium9;
            table2.ShowAutoFilter = false;
            table2.Field("ClientCount").Name = "Nombre de clients";
            table2.Field("AccountCount").Name = "Nombre d'entités";
            ws2.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}
