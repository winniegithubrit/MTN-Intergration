using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BankSystem.Models;



namespace BankSystem.Services
{
  public class MtnMomoService
  {
    private readonly HttpClient _httpClient;

    public MtnMomoService(HttpClient httpClient)
    {
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> CreatePaymentsAsync(CreatePayment model)
    {
      var requestUri = "https://sandbox.momodeveloper.mtn.com/collection/v2_0/payment";
      var requestId = Guid.NewGuid().ToString();

      var jsonContent = JsonSerializer.Serialize(model);
      var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

      _httpClient.DefaultRequestHeaders.Remove("X-Reference-Id");
      _httpClient.DefaultRequestHeaders.Remove("Authorization");

      _httpClient.DefaultRequestHeaders.Add("X-Reference-Id", requestId);
      // Assuming you have a way to obtain the Bearer token from your configuration
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YourBearerTokenHere");

      // Send the HTTP POST request
      HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content);

      if (!response.IsSuccessStatusCode)
      {
        var responseBody = await response.Content.ReadAsStringAsync();
        throw new Exception($"Request failed with status code {response.StatusCode} and response: {responseBody}");
      }

      return await response.Content.ReadAsStringAsync();
    }

  }
}
