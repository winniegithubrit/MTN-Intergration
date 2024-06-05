using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using BankSystem.Services;
using BankSystem.Models;
using Pomelo.EntityFrameworkCore.MySql;

namespace BankSystem.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MTNMomoController : ControllerBase
  {
    private readonly MtnMomoService _mtnMomoService;

    public MTNMomoController(MtnMomoService mtnMomoService)
    {
      _mtnMomoService = mtnMomoService ?? throw new ArgumentNullException(nameof(mtnMomoService));
      
    }

    [HttpPost("request-to-pay")]
    public async Task<IActionResult> RequestToPay([FromBody] RequestToPay model)
    {
      if (model == null)
      {
        return BadRequest("Invalid request payload.");
      }

      try
      {
        var result = await _mtnMomoService.RequestToPayAsync(model);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred: {ex.Message}");
      }
    }

    [HttpPost("create-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePayment model)
    {
      if (model == null ||
          string.IsNullOrEmpty(model.ExternalTransactionId) ||
          model.Money == null ||
          string.IsNullOrEmpty(model.Money.Amount) ||
          string.IsNullOrEmpty(model.Money.Currency) ||
          string.IsNullOrEmpty(model.CustomerReference) ||
          string.IsNullOrEmpty(model.ServiceProviderUserName))
      {
        return BadRequest("Invalid request parameters.");
      }

      try
      {
        var result = await _mtnMomoService.CreatePaymentAsync(model);
        // Assuming CreatePaymentAsync returns the ID or some result after storing data in the database
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred while creating the payment: {ex.Message}");
      }
    }

    [HttpGet("account-balance")]
    public async Task<IActionResult> GetAccountBalance()
    {
      try
      {
        // Replace 'accessToken', 'targetEnvironment', and 'subscriptionKey' with actual values
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTA1VDEyOjM3OjQ4LjY2MCIsInNlc3Npb25JZCI6Ijk3YWZlYzdhLTk3YmYtNGNjZS05OTI2LWI5NGUyY2Q4MGU2ZSJ9.YdgMAGLGMjwISTI-nSxmxaluv7k5ZiNOimMvahx3qqaUrbtRNKZmNEZcNpDzBBw-otlEKTzKX3VD9pb9paZWQ2GmcTXVDIEx5fTEM0NXpqYJFiVcug_tZzZD3N4eP4I_IYKUJQFPeJ0JxjvkTu6EOKsa7pmVmd7hP6Am4-QB3TiTK1H3W2gNnd6AQyRn4wnbx5ko2t7LZjaAwwN8bzD3LPxD6rTIzebrIJXj5ciOqmWZn6YLHVpq2s3seT3kS-K8qc4cqXwX9v5Lrc7x0LT5Bc2SKAfgcrVjaPsko23up8JvqrNTygTx7w75wXLWsYbhuQGztluCr0cKH0oQQIdiiA";
        string targetEnvironment = "sandbox";
        string subscriptionKey = "184789bdc53f4c05870da82d1c307b63";

        // Call the service to get the account balance
        var result = await _mtnMomoService.GetAccountBalanceAsync(accessToken, targetEnvironment, subscriptionKey);

        // Return the account balance as JSON
        return Ok(result);
      }
      catch (Exception ex)
      {
        // Handle any errors and return an error response
        return StatusCode(500, $"An error occurred while fetching account balance: {ex.Message}");
      }
    }

  }
}
