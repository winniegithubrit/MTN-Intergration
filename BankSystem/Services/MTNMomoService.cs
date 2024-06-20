using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;
using BankSystem.Models;

namespace BankSystem.Services
{
  public class MTNMomoService
  {
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MTNMomoService> _logger;
    private readonly string _apiUser;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _subscriptionKey;
    private readonly string _defaultConnection;
    private readonly string _targetEnvironment;

    public MTNMomoService(IMemoryCache cache, HttpClient httpClient, ILogger<MTNMomoService> logger, IConfiguration configuration)
    {
      _cache = cache ?? throw new ArgumentNullException(nameof(cache));
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _apiUser = configuration["MoMoApiOptions:ApiUser"] ?? throw new ArgumentNullException("MoMoApiOptions:ApiUser");
      _apiKey = configuration["MoMoApiOptions:ApiKey"] ?? throw new ArgumentNullException("MoMoApiOptions:ApiKey");
      _subscriptionKey = configuration["MoMoApiOptions:SubscriptionKey"] ?? throw new ArgumentNullException("MoMoApiOptions:SubscriptionKey");
      _baseUrl = configuration["MoMoApiOptions:BaseUrl"] ?? throw new ArgumentNullException("MoMoApiOptions:BaseUrl");
      _targetEnvironment = configuration["MoMoApiOptions:TargetEnvironment"] ?? throw new ArgumentNullException("MoMoApiOptions:TargetEnvironment");
      _defaultConnection = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("ConnectionStrings:DefaultConnection");
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
      var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/collection/token/");
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

    public async Task<string> RequestToPayAsync(RequestToPay model)
    {
      var accessToken = await GetAccessTokenAsync();

      try
      {
        var requestPayload = new
        {
          amount = model.Amount,
          currency = model.Currency,
          externalId = model.ExternalId,
          payer = new
          {
            partyIdType = model.Payer.PartyIdType,
            partyId = model.Payer.PartyId
          },
          payerMessage = model.PayerMessage,
          payeeNote = model.PayeeNote
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/collection/v1_0/requesttopay")
        {
          Content = requestContent
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Reference-Id", Guid.NewGuid().ToString());
        request.Headers.Add("X-Target-Environment", "sandbox");
        request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          _logger.LogError($"Failed to request to pay. Status code: {response.StatusCode}, Content: {errorContent}");
          throw new Exception($"Failed to request to pay. Status code: {response.StatusCode}, Content: {errorContent}");
        }

        await SaveRequestToPayToDatabaseAsync(model);

        return "Request to pay successfully processed.";
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing request to pay: {ex.Message}");
        throw;
      }
    }

    private async Task SaveRequestToPayToDatabaseAsync(RequestToPay model)
    {
      try
      {
        using (var connection = new MySqlConnection(_defaultConnection))
        {
          await connection.OpenAsync();

          using (var command = new MySqlCommand("RequestToPayProcedure", connection))
          {
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@pAmount", MySqlDbType.VarChar).Value = model.Amount;
            command.Parameters.Add("@pCurrency", MySqlDbType.VarChar).Value = model.Currency;
            command.Parameters.Add("@pExternalId", MySqlDbType.VarChar).Value = model.ExternalId;
            command.Parameters.Add("@pPayerPartyIdType", MySqlDbType.VarChar).Value = model.Payer.PartyIdType;
            command.Parameters.Add("@pPayerPartyId", MySqlDbType.VarChar).Value = model.Payer.PartyId;
            command.Parameters.Add("@pPayerMessage", MySqlDbType.VarChar).Value = model.PayerMessage;
            command.Parameters.Add("@pPayeeNote", MySqlDbType.VarChar).Value = model.PayeeNote;

            await command.ExecuteNonQueryAsync();
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while executing RequestToPayProcedure: {ex.Message}");
        throw;
      }
    }

    // CREATE PAYMENT FUNCTIONALITY
    public async Task<string> CreatePaymentAsync(CreatePayment model)
    {
      var accessToken = await GetAccessTokenAsync();

      try
      {
        var paymentPayload = new
        {
          externalTransactionId = model.ExternalTransactionId,
          money = new
          {
            amount = model.Money.Amount,
            currency = model.Money.Currency
          },
          customerReference = model.CustomerReference,
          serviceProviderUserName = model.ServiceProviderUserName,
          couponId = model.CouponId,
          productId = model.ProductId,
          productOfferingId = model.ProductOfferingId,
          receiverMessage = model.ReceiverMessage,
          senderNote = model.SenderNote,
          maxNumberOfRetries = model.MaxNumberOfRetries,
          includeSenderCharges = model.IncludeSenderCharges,
          paymentStatus = model.PaymentStatus 
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(paymentPayload), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/collection/v2_0/payment")
        {
          Content = requestContent
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Reference-Id", Guid.NewGuid().ToString());
        request.Headers.Add("X-Target-Environment", "sandbox");
        request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          _logger.LogError($"Failed to create payment. Status code: {response.StatusCode}, Content: {errorContent}");
          throw new Exception($"Failed to create payment. Status code: {response.StatusCode}, Content: {errorContent}");
        }

        await SaveCreatePaymentDatabaseAsync(model);

        return "Payment created successfully.";
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing created payment: {ex.Message}");
        throw;
      }
    }

  
    private async Task SaveCreatePaymentDatabaseAsync(CreatePayment model)
    {
      try
      {
        using (var connection = new MySqlConnection(_defaultConnection))
        {
          await connection.OpenAsync();

          using (var command = new MySqlCommand("CreatePaymentProcedure", connection))
          {
            command.CommandType = CommandType.StoredProcedure;

            
            command.Parameters.Add("@pId", MySqlDbType.VarChar).Value = model.Id;
            command.Parameters.Add("@pAmount", MySqlDbType.VarChar).Value = model.Money.Amount;
            command.Parameters.Add("@pCurrency", MySqlDbType.VarChar).Value = model.Money.Currency;
            command.Parameters.Add("@pExternalTransactionId", MySqlDbType.VarChar).Value = model.ExternalTransactionId;
            command.Parameters.Add("@pCustomerReference", MySqlDbType.VarChar).Value = model.CustomerReference;
            command.Parameters.Add("@pServiceProviderUserName", MySqlDbType.VarChar).Value = model.ServiceProviderUserName;
            command.Parameters.Add("@pCouponId", MySqlDbType.VarChar).Value = model.CouponId;
            command.Parameters.Add("@pProductId", MySqlDbType.VarChar).Value = model.ProductId;
            command.Parameters.Add("@pProductOfferingId", MySqlDbType.VarChar).Value = model.ProductOfferingId;
            command.Parameters.Add("@pReceiverMessage", MySqlDbType.VarChar).Value = model.ReceiverMessage;
            command.Parameters.Add("@pSenderNote", MySqlDbType.VarChar).Value = model.SenderNote;
            command.Parameters.Add("@pMaxNumberOfRetries", MySqlDbType.Int32).Value = model.MaxNumberOfRetries;
            command.Parameters.Add("@pIncludeSenderCharges", MySqlDbType.Bit).Value = model.IncludeSenderCharges;
            command.Parameters.Add("@pPaymentStatus", MySqlDbType.VarChar).Value = model.PaymentStatus;

            await command.ExecuteNonQueryAsync();
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while executing CreatePaymentProcedure: {ex.Message}");
        throw;
      }
    }
    // GET ACCOUNT BALANCE
    public async Task<GetAccountBalance> GetAccountBalanceAsync()
    {
      var requestUrl = $"{_baseUrl}/collection/v1_0/account/balance";
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
    // GET PAYMENT STATUS
    public async Task<PaymentResult> GetPaymentStatusAsync(string accessToken, string targetEnvironment, string referenceId)
    {
      try
      {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
        _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", _targetEnvironment);

        string requestUrl = $"{_baseUrl}/collection/v2_0/payment/{referenceId}";
        var response = await _httpClient.GetAsync(requestUrl);

        if (response.IsSuccessStatusCode)
        {
          var responseBody = await response.Content.ReadAsStringAsync();
          var paymentStatusResponse = JsonConvert.DeserializeObject<PaymentResult>(responseBody);
          return paymentStatusResponse;
        }
        else
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          throw new Exception($"Failed to get payment status. Status code: {response.StatusCode}, Content: {errorContent}");
        }
      }
      catch (Exception ex)
      {
        throw new Exception($"An error occurred while getting payment status: {ex.Message}");
      }
    }
    public async Task<BasicUserInfo?> GetBasicUserInfoAsync(string accessToken, string targetEnvironment, string accountHolderMSISDN)
    {
      try
      {
        var requestUrl = $"{_baseUrl}/collection/v1_0/accountholder/msisdn/{accountHolderMSISDN}/basicuserinfo";
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
    // CREATE INVOICE FUNCTIONALITY

    public async Task<string> CreateInvoiceAsync(CreateInvoiceModel model)
    {
      var accessToken = await GetAccessTokenAsync();

      try
      {
        var invoicePayload = new
        {
          amount = model.Amount,
          currency = model.Currency,
          externalId = model.ExternalId,
          payer = new
          {
            partyIdType = model.IntendedPayer.PartyIdType,
            partyId = model.IntendedPayer.PartyId
          },
          payerMessage = model.Description,
          payeeNote = model.Description
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(invoicePayload), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/collection/v2_0/invoice")
        {
          Content = requestContent
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Reference-Id", Guid.NewGuid().ToString());
        request.Headers.Add("X-Target-Environment", "sandbox");
        request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          _logger.LogError($"Failed to request to pay. Status code: {response.StatusCode}, Content: {errorContent}");
          throw new Exception($"Failed to request to pay. Status code: {response.StatusCode}, Content: {errorContent}");
        }

        await SaveCreateInvoiceToDatabaseAsync(model);

        return "invoice created successfully.";
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing created invoice: {ex.Message}");
        throw;
      }
    }

    public async Task SaveCreateInvoiceToDatabaseAsync(CreateInvoiceModel model)
    {
      try
      {
        using (var connection = new MySqlConnection(_defaultConnection))
        {
          await connection.OpenAsync();

          using (var command = new MySqlCommand("CreateInvoiceProcedure", connection))
          {
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@pAmount", MySqlDbType.VarChar).Value = model.Amount;
            command.Parameters.Add("@pCurrency", MySqlDbType.VarChar).Value = model.Currency;
            command.Parameters.Add("@pExternalId", MySqlDbType.VarChar).Value = model.ExternalId;
            command.Parameters.Add("@pIntendedPayerPartyIdType", MySqlDbType.VarChar).Value = model.IntendedPayer.PartyIdType;
            command.Parameters.Add("@pIntendedPayerPartyId", MySqlDbType.VarChar).Value = model.IntendedPayer.PartyId;
            command.Parameters.Add("@pDescription", MySqlDbType.VarChar).Value = model.Description;
            command.Parameters.Add("@pValidityDuration", MySqlDbType.VarChar).Value = model.ValidityDuration;

            await command.ExecuteNonQueryAsync();
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while executing CreateInvoiceProcedure: {ex.Message}");
        throw;
      }
    }


  }
}