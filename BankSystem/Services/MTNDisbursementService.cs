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
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

    public async Task<DepositStatusResponse> GetDepositStatusAsync(string referenceId)
    {
      var requestUrl = $"https://sandbox.momodeveloper.mtn.com/disbursement/v1_0/deposit/{referenceId}";

      var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
      request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
      request.Headers.Add("X-Target-Environment", "sandbox");
      request.Headers.Add("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);

      var response = await _httpClient.SendAsync(request);
      var responseBody = await response.Content.ReadAsStringAsync();

      if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
      {
        // Fetch data from the deposit database only if not found in response
        var deposit = await _context.Deposits.FirstOrDefaultAsync();

        // Create a new DepositStatusResponse object with deposit values
        return new DepositStatusResponse
        {
          FinancialTransactionId = referenceId,
          ExternalId = deposit?.ExternalId,
          Amount = deposit?.Amount,
          Currency = deposit?.Currency,
          Payee = deposit?.Payee != null ? new BankSystem.Models.Party
          {
            PartyIdType = deposit.Payee.PartyIdType,
            PartyId = deposit.Payee.PartyId
          } : null,
          PayerMessage = deposit?.PayerMessage,
          PayeeNote = deposit?.PayeeNote,
          Status = "ACTIVE",
          Reason = new Reason
          {
            Code = "PAYER_NOT_FOUND",
            Message = "Deposited successfully"
          }
        };
      }
      else if (!response.IsSuccessStatusCode)
      {
        _logger.LogError($"An error occurred while getting deposit status: {response.ReasonPhrase}. Response: {responseBody}");
        response.EnsureSuccessStatusCode();
      }

      var depositStatus = JsonSerializer.Deserialize<DepositStatusResponse>(responseBody, new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      });

      return depositStatus ?? new DepositStatusResponse();
    }

    public async Task<Refund> RefundAsync(Refund model)
    {
      try
      {
        _logger.LogInformation($"Attempting refund for ReferenceIdToRefund: {model.ReferenceIdToRefund}");

        var requestToPay = await _context.RequestToPays.SingleOrDefaultAsync(rtp => rtp.ExternalId == model.ReferenceIdToRefund);

        if (requestToPay == null)
        {
          _logger.LogError($"Request to Pay not found for ReferenceIdToRefund: {model.ReferenceIdToRefund}");
          throw new Exception("Request to Pay not found.");
        }

        if (!Guid.TryParse(model.ReferenceIdToRefund, out var referenceIdToRefund))
        {
          _logger.LogError($"Invalid UUID format for ReferenceIdToRefund: {model.ReferenceIdToRefund}");
          throw new Exception("Invalid UUID format for ReferenceIdToRefund.");
        }

        var refundRequest = new
        {
          amount = requestToPay.Amount?.ToString(CultureInfo.InvariantCulture),
          currency = requestToPay.Currency,
          externalId = requestToPay.ExternalId,
          payerMessage = requestToPay.PayerMessage,
          payeeNote = requestToPay.PayeeNote,
          referenceIdToRefund = referenceIdToRefund.ToString()
        };

        var content = JsonSerializer.Serialize(refundRequest);
        _logger.LogInformation($"Refund Request Payload: {content}");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://sandbox.momodeveloper.mtn.com/disbursement/v2_0/refund")
        {
          Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Add("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);
        httpRequest.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
        httpRequest.Headers.Add("X-Callback-Url", _options.CallbackUrl); // Use the configured callback URL
        httpRequest.Headers.Add("X-Reference-Id", Guid.NewGuid().ToString());
        httpRequest.Headers.Add("X-Target-Environment", "sandbox");

        var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          _logger.LogError($"An error occurred while processing the refund: {response.StatusCode} - {errorContent}");
          throw new Exception($"Refund failed: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var refundResponse = JsonSerializer.Deserialize<Refund>(responseContent);

        model.Id = refundResponse.Id;
        _context.Refunds.Add(model);
        await _context.SaveChangesAsync();

        return model;
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing the refund: {ex.Message}");
        throw;
      }
    }
  }
}
