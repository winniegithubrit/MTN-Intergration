using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BankSystem.Models;
using BankSystem.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BankSystem.Services
{
  public class MTNDisbursementService
  {
    private readonly HttpClient _httpClient;
    private readonly MoMoDisbursementOptions _options;
    private readonly ApplicationDbContext _dbContext;

    public MTNDisbursementService(HttpClient httpClient, IOptions<MoMoDisbursementOptions> options, ApplicationDbContext dbContext)
    {
      _httpClient = httpClient;
      _options = options.Value;
      _dbContext = dbContext;
    }

    public async Task<Deposit> DepositAsync(Deposit deposit)
    {
      var requestUrl = "https://sandbox.momodeveloper.mtn.com/disbursement/v2_0/deposit";
      var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

      request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
      request.Headers.Add("X-Reference-Id", deposit.ExternalId);
      request.Headers.Add("X-Target-Environment", "sandbox");
      request.Headers.Add("Ocp-Apim-Subscription-Key", "4afffe75a55b4cfca8d08c8e20b95263");


      var json = JsonSerializer.Serialize(deposit);
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      // Save deposit to database
      _dbContext.Deposits.Add(deposit);
      await _dbContext.SaveChangesAsync();

      return deposit;
    }

    public async Task<Refund> RefundAsync(Refund refund)
    {
      var requestUrl = "https://sandbox.momodeveloper.mtn.com/disbursement/v2_0/refund";
      var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

      request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
      request.Headers.Add("X-Reference-Id", refund.ExternalId);
      request.Headers.Add("X-Target-Environment", "sandbox");
      request.Headers.Add("Ocp-Apim-Subscription-Key", "4afffe75a55b4cfca8d08c8e20b95263");

      var json = JsonSerializer.Serialize(refund);
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      // Save refund to database
      _dbContext.Refunds.Add(refund);
      await _dbContext.SaveChangesAsync();

      return refund;
    }

    public async Task<Transfer> TransferAsync(Transfer transfer)
    {
      var requestUrl = "https://sandbox.momodeveloper.mtn.com/disbursement/v1_0/transfer";
      var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

      request.Headers.Add("Authorization", $"Bearer {_options.AccessToken}");
      request.Headers.Add("X-Reference-Id", transfer.ExternalId);
      request.Headers.Add("X-Target-Environment", "sandbox");
      request.Headers.Add("Ocp-Apim-Subscription-Key", "4afffe75a55b4cfca8d08c8e20b95263");

      var json = JsonSerializer.Serialize(transfer);
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      // Save transfer to database
      _dbContext.Transfers.Add(transfer);
      await _dbContext.SaveChangesAsync();

      return transfer;
    }



  }
}
