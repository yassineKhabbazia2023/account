using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using Account.Api.Tests.Configurations;
using Kpmg.Account.API;
using Kpmg.Account.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Pulse.Account.Core.Models.Utils;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Account.Api.Tests.Controllers
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public AccountControllerTests(WebApplicationFactory<Startup> factory)
        {
            this._client = factory.CreateClientWithTestAuth();
        }

        [Fact]
        public async Task Should_GetAccountList_ReturnsOkResultAsync()
        {
            // Arrange
            var url = "api/accounts";
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
            var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();

            // Act
            var response = await this._client.GetAsync(url + "?search&page=1&limit=999");

            // Assert
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //Assert.Equal(JsonSerializer.Serialize(accountList, _jsonOptions), responseString);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            var url = "api/accounts/93012CC8-77B9-4161-8DBD-61915D935E21";
            string accountJson = File.ReadAllText(@"./MockedResponses/AccountDetailMocked.json");
            var accountDetail = JsonSerializer.Deserialize<AccountDetail>(accountJson, _jsonOptions) ?? new AccountDetail();

            // Act
            var response = await this._client.GetAsync(url);

            // Assert
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(JsonSerializer.Serialize(accountDetail, _jsonOptions), responseString);
        }

        [Fact]
        public async Task Should_UpdateAccount_ReturnsOkResultAsync()
        {
            // Arrange
            var url = "api/accounts/93012CC8-77B9-4161-8DBD-61915D935E21";
            string accountJson = File.ReadAllText(@"./MockedResponses/AccountDetailMocked.json");
            var accountDetail = JsonSerializer.Deserialize<AccountDetail>(accountJson, _jsonOptions) ?? new AccountDetail();
            var accountString = JsonSerializer.Serialize(accountDetail, _jsonOptions);
            var content = new StringContent(accountString, Encoding.UTF8, "application/json");

            // Act
            var response = await this._client.PatchAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_GetAccountFavoriteList_ReturnsOkResultAsync()
        {
            // Arrange
            var url = "api/favorites/8696B9E3-41D9-4B92-A541-714B6E69B998";
            var accountJson = new AccountFavorite()
            {
                AccountId = "93012CC8-77B9-4161-8DBD-61915D935E21",
                LegalName = "JEAN LEVAGE",
                Icon = "jeanlevage"
            };

            // Act
            var response = await this._client.GetAsync(url);

            // Assert
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(JsonSerializer.Serialize(accountJson, _jsonOptions), responseString);
        }

        [Fact]
        public async Task Should_SetFavorite_ReturnsOkResultAsync()
        {
            // Arrange
            var url = "api/favorites/93012CC8-77B9-4161-8DBD-61915D935E21/8696B9E3-41D9-4B92-A541-714B6E69B998/true";
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            // Act
            var response = await this._client.PatchAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
