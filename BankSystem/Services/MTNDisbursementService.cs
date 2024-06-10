using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BankSystem.Models;
using BankSystem.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BankSystem.Services
{
  public class MTNDisbursementService
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<MTNDisbursementService> _logger;
    private readonly MoMoDisbursementOptions _options;
    private readonly ApplicationDbContext _context;

    public MTNDisbursementService(HttpClient httpClient, ILogger<MTNDisbursementService> logger, IOptions<MoMoDisbursementOptions> options, ApplicationDbContext context)
    {
      _httpClient = httpClient;
      _logger = logger;
      _options = options.Value;
      _context = context;
    }

    public async Task<Deposit> DepositAsync(Deposit deposit)
    {
      // Save the deposit to the database before sending the request
      _context.Deposits.Add(deposit);
      await _context.SaveChangesAsync();

      // Retrieve user information using MSISDN from the deposit
      var userInfo = await GetBasicUserInfoAsync(deposit.Payee.PartyId);
      _logger.LogInformation($"Retrieved user info for MSISDN {deposit.Payee.PartyId}: {JsonSerializer.Serialize(userInfo)}");
      _context.BasicUserInfomation.Add(userInfo);
      await _context.SaveChangesAsync();

     
      var requestUrl = "https://sandbox.momodeveloper.mtn.com/disbursement/v2_0/deposit";
      var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

      request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
      request.Headers.Add("X-Reference-Id", Guid.NewGuid().ToString());
      request.Headers.Add("X-Target-Environment", "sandbox");
      request.Headers.Add("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);

      var depositPayload = new
      {
        amount = deposit.Amount,
        currency = deposit.Currency,
        externalId = deposit.ExternalId,
        payee = new
        {
          partyIdType = deposit.Payee?.PartyIdType,
          partyId = deposit.Payee?.PartyId
        },
        payerMessage = deposit.PayerMessage,
        payeeNote = deposit.PayeeNote
      };

      var json = JsonSerializer.Serialize(depositPayload, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");

      _logger.LogInformation($"Sending request to {requestUrl} with content: {json}");

      var response = await _httpClient.SendAsync(request);

      var responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError($"An error occurred while processing deposit: {response.ReasonPhrase}. Response: {responseBody}");
        response.EnsureSuccessStatusCode();
      }

      return deposit;
    }

    // get account balance
    public async Task<BalanceResponse> GetAccountBalanceAsync()
    {
      var requestUrl = "https://sandbox.momodeveloper.mtn.com/disbursement/v1_0/account/balance";
      var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

      request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
      request.Headers.Add("X-Target-Environment", "sandbox");
      request.Headers.Add("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);

      _logger.LogInformation($"Sending request to {requestUrl} to get account balance");

      var response = await _httpClient.SendAsync(request);

      var responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError($"An error occurred while getting account balance: {response.ReasonPhrase}. Response: {responseBody}");
        response.EnsureSuccessStatusCode();
      }

      var balanceResponse = JsonSerializer.Deserialize<BalanceResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

      if (balanceResponse == null)
      {
        throw new InvalidOperationException("Failed to retrieve balance. The response is null.");
      }

      return balanceResponse;
    }
    public async Task<BasicUserInfoResponse> GetBasicUserInfoAsync(string? msisdn)
    {
      if (msisdn == null)
      {
        throw new ArgumentNullException(nameof(msisdn), "MSISDN cannot be null.");
      }

      var requestUrl = $"https://sandbox.momodeveloper.mtn.com/disbursement/v1_0/accountholder/msisdn/{msisdn}/basicuserinfo";
      var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

      request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
      request.Headers.Add("X-Target-Environment", "sandbox");
      request.Headers.Add("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);

      _logger.LogInformation($"Sending request to {requestUrl} to get basic user info for MSISDN: {msisdn}");

      var response = await _httpClient.SendAsync(request);

      var responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError($"An error occurred while getting basic user info for MSISDN {msisdn}: {response.ReasonPhrase}. Response: {responseBody}");
        response.EnsureSuccessStatusCode();
      }

      var userInfoResponse = JsonSerializer.Deserialize<BasicUserInfoResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

      if (userInfoResponse == null)
      {
        throw new InvalidOperationException("Failed to retrieve user info. The response is null.");
      }

      return userInfoResponse;
    }

  }
}
