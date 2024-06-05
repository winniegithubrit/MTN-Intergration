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
      // Save the data directly from the model
      _dbContext.CreatePayments.Add(model);
      await _dbContext.SaveChangesAsync();

      return $"Payment created successfully with external transaction ID: {model.ExternalTransactionId}";
    }

    public async Task<string> RequestToPayAsync(RequestToPay model)
    {
      // Save the data directly from the model
      _dbContext.RequestToPays.Add(model);
      await _dbContext.SaveChangesAsync();

      // Assuming you want to return some response indicating success
      return "Request to pay created successfully!";
    }

    public async Task<string> GetAccountBalanceAsync(string accessToken, string targetEnvironment, string subscriptionKey)
    {
      // Set the authorization header
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      // Set the subscription key header
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

      // Set the X-Target-Environment header
      _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", targetEnvironment);

      // Make the GET request
      var response = await _httpClient.GetAsync("https://sandbox.momodeveloper.mtn.com/collection/v1_0/account/balance");

      // Read the response content
      var responseBody = await response.Content.ReadAsStringAsync();

      // Return the response body
      return responseBody;
    }


    public async Task<string> GetAccountBalanceInSpecificCurrencyAsync(string accessToken, string targetEnvironment, string subscriptionKey, string currency)
    {
      // Set the authorization header
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      // Set the subscription key header
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

      // Set the X-Target-Environment header
      _httpClient.DefaultRequestHeaders.Add("X-Target-Environment", targetEnvironment);

      // Construct the request URL with the currency parameter
      string requestUrl = $"https://sandbox.momodeveloper.mtn.com/collection/v1_0/account/balance/{currency}";

      // Make the GET request
      var response = await _httpClient.GetAsync(requestUrl);

      // Check if the request was successful
      if (response.IsSuccessStatusCode)
      {
        // Read the response content
        var responseBody = await response.Content.ReadAsStringAsync();

        // Return the response body
        return responseBody;
      }
      else
      {
        // If the request was not successful, throw an HttpRequestException with a descriptive message
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



  }
}
