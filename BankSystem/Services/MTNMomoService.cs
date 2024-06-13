using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using BankSystem.Options;

namespace BankSystem.Services
{
  public class MtnMomoService
  {
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    private readonly ILogger<MtnMomoService> _logger;
    private readonly MoMoApiOptions _moMoApiOptions;

    public MtnMomoService(ApplicationDbContext context, HttpClient httpClient, ILogger<MtnMomoService> logger, IOptions<MoMoApiOptions> optionsAccessor)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _moMoApiOptions = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));

    }

    public async Task<string> CreatePaymentAsync(CreatePayment model)
    {
      _context.CreatePayments.Add(model);
      await _context.SaveChangesAsync();

      return $"Payment created successfully with external transaction ID: {model.ExternalTransactionId}";
    }

    public async Task<RequestToPay> RequestToPayAsync(RequestToPay model)
    {
      _context.RequestToPays.Add(model);
      await _context.SaveChangesAsync();
      return model;
    }

    public async Task<string> CreateInvoiceAsync(CreateInvoiceModel model)
    {
      
      if (model.IntendedPayer == null || string.IsNullOrEmpty(model.IntendedPayer.PartyIdType) || string.IsNullOrEmpty(model.IntendedPayer.PartyId) ||
          model.Payee == null || string.IsNullOrEmpty(model.Payee.PartyIdType) || string.IsNullOrEmpty(model.Payee.PartyId))
      {
        throw new ArgumentException("IntendedPayer and Payee must be fully specified.");
      }

      _context.Invoices.Add(model);
      await _context.SaveChangesAsync();

      return $"Invoice created successfully with external ID: {model.ExternalId}";
    }



    public async Task UpdateInvoiceStatusAsync(string externalId, string newStatus)
    {
      var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.ExternalId == externalId);
      if (invoice != null)
      {
        invoice.Status = newStatus;
        await _context.SaveChangesAsync();
      }
    }
    public async Task<GetAccountBalance> GetAccountBalanceAsync()
    {
      var requestUrl = "https://sandbox.momodeveloper.mtn.com/collection/v1_0/account/balance";
      var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

      request.Headers.Add("Authorization", $"Bearer {_moMoApiOptions.AccessToken}");
      request.Headers.Add("X-Target-Environment", _moMoApiOptions.TargetEnvironment);
      request.Headers.Add("Ocp-Apim-Subscription-Key", _moMoApiOptions.SubscriptionKey);

      _logger.LogInformation($"Sending request to {requestUrl} to get account balance");

      var response = await _httpClient.SendAsync(request);

      var responseBody = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError($"An error occurred while getting account balance: {response.ReasonPhrase}. Response: {responseBody}");
        response.EnsureSuccessStatusCode();
      }

      var balanceResponse = JsonSerializer.Deserialize<GetAccountBalance>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

      if (balanceResponse == null)
      {
        throw new InvalidOperationException("Failed to retrieve balance. The response is null.");
      }

      return balanceResponse;
    }

    public async Task<string> GetAccountBalanceInSpecificCurrencyAsync(string accessToken, string targetEnvironment, string subscriptionKey, string currency)
    {
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
      _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", targetEnvironment);
      string requestUrl = $"https://sandbox.momodeveloper.mtn.com/collection/v1_0/account/balance/{currency}";
      var response = await _httpClient.GetAsync(requestUrl);
      if (response.IsSuccessStatusCode)
      {
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
      }
      else
      {
        throw new HttpRequestException($"Failed to get account balance: {response.StatusCode} - {response.ReasonPhrase}");
      }
    }

    public async Task<GetBasicUserInfo?> GetBasicUserInfoAsync(string accessToken, string targetEnvironment, string subscriptionKey, string accountHolderMSISDN)
    {
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
      _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", targetEnvironment);
      string requestUrl = $"https://sandbox.momodeveloper.mtn.com/collection/v1_0/accountholder/msisdn/{accountHolderMSISDN}/basicuserinfo";
      var response = await _httpClient.GetAsync(requestUrl);
      if (response.IsSuccessStatusCode)
      {
        var responseBody = await response.Content.ReadAsStringAsync();
        var basicUserInfo = JsonSerializer.Deserialize<GetBasicUserInfo>(responseBody);
        if (basicUserInfo != null)
        {
          return basicUserInfo;
        }
        else
        {
          return null;
        }
      }
      else
      {
        throw new HttpRequestException($"Failed to get basic user information: {response.StatusCode} - {response.ReasonPhrase}");
      }
    }

    public async Task<GetPaymentStatus?> GetPaymentStatusAsync(string accessToken, string targetEnvironment, string subscriptionKey, string xReferenceId)
    {
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
      _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", targetEnvironment);
      string requestUrl = $"https://sandbox.momodeveloper.mtn.com/collection/v2_0/payment/{xReferenceId}";
      var response = await _httpClient.GetAsync(requestUrl);
      if (response.IsSuccessStatusCode)
      {
        var responseBody = await response.Content.ReadAsStringAsync();
        var paymentStatusResponse = JsonSerializer.Deserialize<GetPaymentStatus>(responseBody);

        var payment = await _context.CreatePayments
            .FirstOrDefaultAsync(p => p.ExternalTransactionId == xReferenceId);

        if (payment != null)
        {
          if (paymentStatusResponse != null) 
          {
            if (payment.PaymentStatus == null)
            {
              payment.PaymentStatus = paymentStatusResponse;
              paymentStatusResponse.CreatePaymentId = payment.Id;
              _context.PaymentStatuses.Add(paymentStatusResponse);
            }
            else
            {
              payment.PaymentStatus.Status = paymentStatusResponse.Status;
              _context.PaymentStatuses.Update(payment.PaymentStatus);
            }

            await _context.SaveChangesAsync();
          }
        }

        return paymentStatusResponse;
      }
      else
      {
        throw new HttpRequestException($"Failed to get payment status: {response.StatusCode} - {response.ReasonPhrase}");
      }
    }

    public async Task CancelInvoiceAsync(string externalId)
    {
      var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.ExternalId == externalId);
      if (invoice != null)
      {
        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
      }
      else
      {
        throw new ArgumentException("Invoice not found.");
      }
    }
}
}