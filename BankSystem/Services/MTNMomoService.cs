using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

  }
}
