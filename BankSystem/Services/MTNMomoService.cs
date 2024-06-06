using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace BankSystem.Services
{
  public class MtnMomoService
  {
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    public MtnMomoService(ApplicationDbContext context, HttpClient httpClient)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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
      _context.Invoices.Add(model);
      await _context.SaveChangesAsync();

      return $"Invoice created successfully with external ID: {model.ExternalId}";
    }

    public async Task<string> GetAccountBalanceAsync(string accessToken, string targetEnvironment, string subscriptionKey)
    {
    
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
      _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", targetEnvironment);
      var response = await _httpClient.GetAsync("https://sandbox.momodeveloper.mtn.com/collection/v1_0/account/balance");
      var responseBody = await response.Content.ReadAsStringAsync();
      return responseBody;
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
      // Set the authorization header
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      // Set the subscription key header
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

      // Set the X-Target-Environment header
      _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", targetEnvironment);

      // Construct the request URL with the accountHolderMSISDN parameter
      string requestUrl = $"https://sandbox.momodeveloper.mtn.com/collection/v1_0/accountholder/msisdn/{accountHolderMSISDN}/basicuserinfo";

      // Make the GET request
      var response = await _httpClient.GetAsync(requestUrl);

      // Check if the request was successful
      if (response.IsSuccessStatusCode)
      {
        // Deserialize the response JSON to BasicUserInfo object
        var responseBody = await response.Content.ReadAsStringAsync();
        var basicUserInfo = JsonSerializer.Deserialize<GetBasicUserInfo>(responseBody);

        // Check if basicUserInfo is null
        if (basicUserInfo != null)
        {
          // Return the BasicUserInfo object
          return basicUserInfo;
        }
        else
        {
          // Handle the case where deserialization fails or response is unexpected
          // You can log a warning or return null
          return null;
        }
      }
      else
      {
        // If the request was not successful, throw an HttpRequestException with a descriptive message
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
        return paymentStatusResponse;
      }
      else
      {
        throw new HttpRequestException($"Failed to get payment status: {response.StatusCode} - {response.ReasonPhrase}");
      }
    }
  }
}
