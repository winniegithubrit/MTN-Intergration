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
    private readonly ILogger<MTNMomoController> _logger;

    public MTNMomoController(MtnMomoService mtnMomoService, ILogger<MTNMomoController> logger)
    {
      _mtnMomoService = mtnMomoService ?? throw new ArgumentNullException(nameof(mtnMomoService));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
    [HttpGet("account-balance/{currency}")]
    public async Task<IActionResult> GetAccountBalance(string currency)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(currency))
        {
          return BadRequest("Currency parameter is required.");
        }

        // Replace 'accessToken', 'targetEnvironment', and 'subscriptionKey' with actual values
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTA1VDEyOjU3OjU4LjA1MyIsInNlc3Npb25JZCI6IjdlZDNkNzRhLWZlNDQtNDBhMy1hNDdlLTdlYTg2NzkzOTU0MiJ9.FDLjRwWV-pYa5Mn3AS6nXDsc_YrJ8h4zyM0S14-tLTR-AKCR2vt-I0S0vt7oww5INpVHiMFue5wMWkyEOWpo2kp1Lqs3yJCmJAStuBfPC_1y4Xp_7ixC5wlcmkdVcgSmI85swu_t0wFpzTB_jdFdy7d1W7NQKn_H0dFAqdyi7o_YfRyS52f0sVDJEtQh0mV1tSiFI4w9w4xSr-HqoOjfbNp15yHVOvuIQpz-jgCm9Wb5_bLFoZmd8TIIcngYV9RQ1ZSkoMkMcljYObJRwwBaLlf-mH_5WRyFBWMHAAAkLQWU8a9U1rGIEnzmmcLJQoggP1RQklUwxBLIYebpO6WP9A";
        string targetEnvironment = "sandbox";
        string subscriptionKey = "184789bdc53f4c05870da82d1c307b63";

        // Call the service to get the account balance in the specified currency
        var result = await _mtnMomoService.GetAccountBalanceInSpecificCurrencyAsync(accessToken, targetEnvironment, subscriptionKey, currency);

        // Return the account balance as JSON
        return Ok(result);
      }
      catch (HttpRequestException ex)
      {
        // Log the exception
        _logger.LogError($"HTTP request failed: {ex.Message}");

        // Handle specific HTTP request exceptions
        if (ex.StatusCode.HasValue)
        {
          return StatusCode((int)ex.StatusCode, ex.Message);
        }
        else
        {
          return StatusCode(500, "An error occurred while fetching account balance. Please try again later.");
        }
      }
      catch (Exception ex)
      {
        // Log the exception
        _logger.LogError($"An error occurred: {ex.Message}");

        // Handle any other exceptions
        return StatusCode(500, $"An error occurred while fetching account balance: {ex.Message}");
      }
    }

    [HttpGet("basic-user-info/{accountHolderMSISDN}")]
    public async Task<IActionResult> GetBasicUserInfo(string accountHolderMSISDN)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(accountHolderMSISDN))
        {
          return BadRequest("Account holder MSISDN parameter is required.");
        }

        // Replace 'accessToken', 'targetEnvironment', and 'subscriptionKey' with actual values
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTA1VDEzOjMyOjI1LjA1NyIsInNlc3Npb25JZCI6IjZiZTRjOTg1LWVkOGEtNDNjYy1hYzI3LWVkOTBmM2E4Yzk3MyJ9.DIfkb-a5-Sxp0p6h0rYQLYmTKKMQZuhTZkbJ54CxEOKvwvigyinPb7QQR06HMqOtrQMErPdSYspF-dFHlSL5uGUkhLc7aCw_Cv9jrnsm-Uu1_NYHLyMpDeDbZUK-zO84jbTAEXn2KIfQhEVSsxDvUCHBD18S9xiq7LpCwbzonVQIfOhKSIuEqQyU01LhbTsNLNYHbbF_MA_hJ3UsxVqFTSbVXTOZZGPvfh8J7MT3PGMpHRlK2Yhte1rL-4edwxzLB3XOQRrCpfYYYwcA9WQkULWqDrs509hsEx_q_0GSwKA3cdqkpygxx_s1dOKonYev5D9ncuPGVm9G7MEBA-t8WA";
        string targetEnvironment = "sandbox";
        string subscriptionKey = "184789bdc53f4c05870da82d1c307b63";

        // Call the service to get the basic user information
        var result = await _mtnMomoService.GetBasicUserInfoAsync(accessToken, targetEnvironment, subscriptionKey, accountHolderMSISDN);

        // Return the basic user information as JSON
        return Ok(result);
      }
      catch (HttpRequestException ex)
      {
        // Log the exception
        _logger.LogError($"HTTP request failed: {ex.Message}");

        // Handle specific HTTP request exceptions
        if (ex.StatusCode.HasValue)
        {
          return StatusCode((int)ex.StatusCode, ex.Message);
        }
        else
        {
          return StatusCode(500, "An error occurred while fetching basic user information. Please try again later.");
        }
      }
      catch (Exception ex)
      {
        // Log the exception
        _logger.LogError($"An error occurred: {ex.Message}");

        // Handle any other exceptions
        return StatusCode(500, $"An error occurred while fetching basic user information: {ex.Message}");
      }
    }


  }
}
