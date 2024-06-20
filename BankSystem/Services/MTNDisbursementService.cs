using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BankSystem.Models;

namespace BankSystem.Services
{
  public class MTNDisbursementService
  {
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MTNDisbursementService> _logger;
    private readonly string _apiUser;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _subscriptionKey;
    private readonly string _targetEnvironment;
    private readonly string _defaultConnection;

    public MTNDisbursementService(IMemoryCache cache, HttpClient httpClient, ILogger<MTNDisbursementService> logger, IConfiguration configuration)
    {
      _cache = cache ?? throw new ArgumentNullException(nameof(cache));
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      _apiUser = configuration["MoMoDisbursementOptions:ApiUser"] ?? throw new ArgumentNullException("MoMoDisbursementOptions:ApiUser");
      _apiKey = configuration["MoMoDisbursementOptions:ApiKey"] ?? throw new ArgumentNullException("MoMoDisbursementOptions:ApiKey");
      _subscriptionKey = configuration["MoMoDisbursementOptions:SubscriptionKey"] ?? throw new ArgumentNullException("MoMoDisbursementOptions:SubscriptionKey");
      _baseUrl = configuration["MoMoDisbursementOptions:BaseUrl"] ?? throw new ArgumentNullException("MoMoDisbursementOptions:BaseUrl");
      _targetEnvironment = configuration["MoMoDisbursementOptions:TargetEnvironment"] ?? throw new ArgumentNullException("MoMoDisbursementOptions:TargetEnvironment");
      _defaultConnection = configuration["ConnectionStrings:DefaultConnection"] ?? throw new ArgumentNullException("ConnectionStrings:DefaultConnection");
    }

    public async Task<string> GetAccessTokenAsync()
    {
      if (!_cache.TryGetValue("AccessToken", out string? accessToken))
      {
        _logger.LogInformation("Access token not found in cache, creating new one.");
        accessToken = await CreateAccessTokenAsync();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(50));

        _cache.Set("AccessToken", accessToken, cacheEntryOptions);
      }
      else
      {
        _logger.LogInformation("Access token found in cache.");
      }

      return accessToken!;
    }

    private async Task<string> CreateAccessTokenAsync()
    {
      var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/disbursement/token/");
      var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_apiUser}:{_apiKey}"));

      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

      HttpResponseMessage response = await _httpClient.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
      {
        _logger.LogError("Unauthorized: Invalid credentials or subscription key");
        throw new UnauthorizedAccessException("Invalid credentials or subscription key");
      }

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError($"Failed to create access token. Status code: {response.StatusCode}, Content: {errorContent}");
        throw new Exception($"Failed to create access token. Status code: {response.StatusCode}, Content: {errorContent}");
      }

      var content = await response.Content.ReadAsStringAsync();
      var json = JObject.Parse(content);

      return json["access_token"]?.ToString() ?? throw new Exception("Access token not found in response.");
    }

    public async Task<string> DepositAsync(Deposit model)
    {
      var accessToken = await GetAccessTokenAsync();

      try
      {
        var requestPayload = new
        {
          amount = model.Amount,
          currency = model.Currency,
          externalId = model.ExternalId,
          payee = new
          {
            partyIdType = model.Payee.PartyIdType,
            partyId = model.Payee.PartyId
          },
          payerMessage = model.PayerMessage,
          payeeNote = model.PayeeNote
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/disbursement/v2_0/deposit")
        {
          Content = requestContent
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Reference-Id", Guid.NewGuid().ToString());
        request.Headers.Add("X-Target-Environment", _targetEnvironment);
        request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          _logger.LogError($"Failed to request deposit. Status code: {response.StatusCode}, Content: {errorContent}");
          throw new Exception($"Failed to request deposit. Status code: {response.StatusCode}, Content: {errorContent}");
        }

        await SaveDepositToDatabaseAsync(model);

        return "Deposit successfully processed.";
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing deposit: {ex.Message}");
        throw;
      }
    }

    private async Task SaveDepositToDatabaseAsync(Deposit model)
    {
      try
      {
        using (var connection = new MySqlConnection(_defaultConnection))
        {
          await connection.OpenAsync();

          using (var command = new MySqlCommand("DepositProcedure", connection))
          {
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@pAmount", MySqlDbType.VarChar).Value = model.Amount;
            command.Parameters.Add("@pCurrency", MySqlDbType.VarChar).Value = model.Currency;
            command.Parameters.Add("@pExternalId", MySqlDbType.VarChar).Value = model.ExternalId;
            command.Parameters.Add("@pPayeePartyIdType", MySqlDbType.VarChar).Value = model.Payee.PartyIdType;
            command.Parameters.Add("@pPayeePartyId", MySqlDbType.VarChar).Value = model.Payee.PartyId;
            command.Parameters.Add("@pPayerMessage", MySqlDbType.VarChar).Value = model.PayerMessage;
            command.Parameters.Add("@pPayeeNote", MySqlDbType.VarChar).Value = model.PayeeNote;

            await command.ExecuteNonQueryAsync();
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while executing DepositProcedure: {ex.Message}");
        throw;
      }
    }
    // BALANCE
    public async Task<GetAccountBalance> GetAccountBalanceAsync()
    {
      var requestUrl = $"{_baseUrl}/disbursement/v1_0/account/balance";
      var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

      var accessToken = await GetAccessTokenAsync();

      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      request.Headers.Add("X-Target-Environment", "sandbox");
      request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

      var response = await _httpClient.SendAsync(request);
      var responseBody = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"An error occurred while getting account balance: {response.ReasonPhrase}. Response: {responseBody}");
      }

      var balanceResponse = JsonConvert.DeserializeObject<GetAccountBalance>(responseBody);
      return balanceResponse ?? throw new Exception("Failed to deserialize account balance response.");
    }
    // BASIC USER INFO
    public async Task<BasicUserInfo?> GetBasicUserInfoAsync(string accessToken, string targetEnvironment, string accountHolderMSISDN)
    {
      try
      {
        var requestUrl = $"{_baseUrl}/disbursement/v1_0/accountholder/msisdn/{accountHolderMSISDN}/basicuserinfo";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Target-Environment", targetEnvironment);
        request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
          var content = await response.Content.ReadAsStringAsync();
          var basicUserInfo = JsonConvert.DeserializeObject<BasicUserInfo>(content);
          return basicUserInfo;
        }
        else
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          throw new HttpRequestException($"HTTP request failed with status code {response.StatusCode}. Content: {errorContent}");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while getting basic user info: {ex.Message}");
        throw;
      }
    }
    // DEPOSIT STATUS

    public async Task<DepositResult> GetDepositStatusAsync(string accessToken, string targetEnvironment, string referenceId)
    {
      try
      {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
        _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", _targetEnvironment);

        string requestUrl = $"{_baseUrl}/disbursement/v1_0/deposit/{referenceId}";
        var response = await _httpClient.GetAsync(requestUrl);

        if (response.IsSuccessStatusCode)
        {
          var responseBody = await response.Content.ReadAsStringAsync();
          var depositStatusResponse = JsonConvert.DeserializeObject<DepositResult>(responseBody);
          return depositStatusResponse;
        }
        else
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          throw new Exception($"Failed to get deposit status. Status code: {response.StatusCode}, Content: {errorContent}");
        }
      }
      catch (Exception ex)
      {
        throw new Exception($"An error occurred while getting deposit status: {ex.Message}");
      }
    }
  }
}
