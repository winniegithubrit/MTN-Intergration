using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Pomelo.EntityFrameworkCore.MySql;
using System.Net.Http.Headers;

namespace BankSystem.Services
{
  public class MtnMomoService
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public MtnMomoService(ApplicationDbContext dbContext, HttpClient httpClient)
    {
      _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> CreatePaymentAsync(CreatePayment model)
    {
      // Saving the data directly from the model
      _dbContext.CreatePayments.Add(model);
      await _dbContext.SaveChangesAsync();

      return $"Payment created successfully with external transaction ID: {model.ExternalTransactionId}";
    }

    public async Task<string> RequestToPayAsync(RequestToPay model)
    {
      _dbContext.RequestToPays.Add(model);
      await _dbContext.SaveChangesAsync();
      return "Request to pay created successfully!";
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
        return paymentStatusResponse;
      }
      else
      {
        throw new HttpRequestException($"Failed to get payment status: {response.StatusCode} - {response.ReasonPhrase}");
      }
    }



  }
}
