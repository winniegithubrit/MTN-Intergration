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
      if (model == null ||
          string.IsNullOrEmpty(model.Amount) ||
          string.IsNullOrEmpty(model.Currency) ||
          string.IsNullOrEmpty(model.ExternalId) ||
          model.Payer == null ||
          string.IsNullOrEmpty(model.Payer.PartyIdType) ||
          string.IsNullOrEmpty(model.Payer.PartyId))
      {
        return BadRequest("Invalid request parameters.");
      }

      try
      {
        var result = await _mtnMomoService.RequestToPayAsync(model);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while creating the request to pay: {ex.Message}");
        return StatusCode(500, $"An error occurred while creating the request to pay: {ex.Message}");
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
        
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTA3VDExOjQ5OjI2LjcyNCIsInNlc3Npb25JZCI6ImI5MmMwYjk1LTY1ZDQtNGQxOS1hYjZlLTA2OWExZjYyZDQ3MiJ9.MEh-sw8CbozaRHqSpaBB6-JsytaYCNmY9IReQb-NxROD8E8pD0gd_gqaoesq5ZI_PosKeL4Gl4gVCVZtlCSdrLKMCN2HheRbSAelJWL0kipcvBzzSvcCuZqIDG67OvTyGeKqOf3HY6H4Sj3hCBjxDaNRFYPprsDT-LX2g5UZ-F3DgheIjifiW0_IhQi0RjclXwS2eQMCWHVjbRWwsI3w-GEuADYM8bTgO2qV0mIPK00FSrZ2z99_ej5M84ifV1ku4HnehS2tgknSriYdbln9Cv4Gj6neom0V4b9SSwqLImyvkjHKmLLThygELGg99XdYET2hAmQW2vO5oR0lDBub5A";
        string targetEnvironment = "sandbox";
        string subscriptionKey = "184789bdc53f4c05870da82d1c307b63";

        var result = await _mtnMomoService.GetAccountBalanceAsync(accessToken, targetEnvironment, subscriptionKey);
        return Ok(result);
      }
      catch (Exception ex)
      {
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
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTA2VDExOjI0OjE2LjkxMyIsInNlc3Npb25JZCI6Ijg3NTUyNDUzLTU2NDQtNGM0Mi1hYzVlLTg4MTJhMmJlODViZiJ9.ZLKX1k8MJlp0psvRgVqpqbEfilLhHe7eJIGywIuonNp1aAcppHa-zBgZyGfc_YGWbiWF5Sv1xQDMzjFUidFDp5YwTn-dvrCdy74xqNe5piHDdXQkO-BPKQVVeN_psYdL-N2BrolxxK5YyxXEdrX2_tcN3vUZD9ln2iqbi2TK_q23O65miSwMr6KYNLbdgn7bTC8Tk_LAzIL3-QQoC-PfHiFrDChvtm4phpXZpOL9whUDxDR4G31lq638uWXTRYyGxHItIcEHCIWTcb7clowyNWYL32Mmq4e_imDeQjA4O6JiRNf7vxSV0ZGcu6oWYAWl28yNqdEPI9toW63ZD2LYmg";
        string targetEnvironment = "sandbox";
        string subscriptionKey = "184789bdc53f4c05870da82d1c307b63";
        var result = await _mtnMomoService.GetAccountBalanceInSpecificCurrencyAsync(accessToken, targetEnvironment, subscriptionKey, currency);
        return Ok(result);
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError($"HTTP request failed: {ex.Message}");
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
        _logger.LogError($"An error occurred: {ex.Message}");
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
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTA3VDExOjQ5OjI2LjcyNCIsInNlc3Npb25JZCI6ImI5MmMwYjk1LTY1ZDQtNGQxOS1hYjZlLTA2OWExZjYyZDQ3MiJ9.MEh-sw8CbozaRHqSpaBB6-JsytaYCNmY9IReQb-NxROD8E8pD0gd_gqaoesq5ZI_PosKeL4Gl4gVCVZtlCSdrLKMCN2HheRbSAelJWL0kipcvBzzSvcCuZqIDG67OvTyGeKqOf3HY6H4Sj3hCBjxDaNRFYPprsDT-LX2g5UZ-F3DgheIjifiW0_IhQi0RjclXwS2eQMCWHVjbRWwsI3w-GEuADYM8bTgO2qV0mIPK00FSrZ2z99_ej5M84ifV1ku4HnehS2tgknSriYdbln9Cv4Gj6neom0V4b9SSwqLImyvkjHKmLLThygELGg99XdYET2hAmQW2vO5oR0lDBub5A";
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

    [HttpGet("payment-status/{xReferenceId}")]
    public async Task<IActionResult> GetPaymentStatus(string xReferenceId)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(xReferenceId))
        {
          return BadRequest("X-Reference-Id parameter is required.");
        }

       
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTA2VDExOjQ0OjQyLjI2NyIsInNlc3Npb25JZCI6ImJhY2MzZjZhLTQxZTktNDZiNy1iYTJmLTI2NGZjMjM3NjNiZiJ9.OX02ZBvn7AXZwkfwdgUZg8mjMz4iYKqWua7O8__ehO0d5IBlB86LaM3ZM-LZCWp2S9hA_4ZUQk5mxr707aHzVlAXNMBx79_uhFdvC8ParkFtJ2-e6kY_nt9k4g8AGVI5CSSwdIcv9DIX964OYqxnKtof4bnc_bOeo6dcoQAlsOvscPDpJJJs2TtBLZEkksIwUYDDVeQDl69RCv1P96VHaN7cmgeE5GDY03maExFiwez366Sv99rFdCcHKfQKQd5-ax49wamx2DM8PxxVMLqhhCZdiZ324HERLvley4jtb3A5GejUVtTr9FMITg4OvID7tykJojF9tBJ2FC0Nv6dX-g";
        string targetEnvironment = "sandbox";
        string subscriptionKey = "184789bdc53f4c05870da82d1c307b63";
        var result = await _mtnMomoService.GetPaymentStatusAsync(accessToken, targetEnvironment, subscriptionKey, xReferenceId);
        return Ok(result);
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError($"HTTP request failed: {ex.Message}");
        if (ex.StatusCode.HasValue)
        {
          return StatusCode((int)ex.StatusCode, ex.Message);
        }
        else
        {
          return StatusCode(500, "An error occurred while getting payment status. Please try again later.");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred: {ex.Message}");
        return StatusCode(500, $"An error occurred while getting payment status: {ex.Message}");
      }
    }

    [HttpPost("create-invoice")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceModel model)
    {
      if (model == null ||
          string.IsNullOrEmpty(model.Amount) ||
          string.IsNullOrEmpty(model.Currency) ||
          string.IsNullOrEmpty(model.ExternalId) ||
          model.IntendedPayer == null ||
          string.IsNullOrEmpty(model.IntendedPayer.PartyIdType) ||
          string.IsNullOrEmpty(model.IntendedPayer.PartyId))
      {
        return BadRequest("Invalid request parameters.");
      }

      try
      {
        var result = await _mtnMomoService.CreateInvoiceAsync(model);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while creating the invoice: {ex.Message}");
        return StatusCode(500, $"An error occurred while creating the invoice: {ex.Message}");
      }
    }


  }
}
