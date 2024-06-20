using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankSystem.Services;
using BankSystem.Models;
using Microsoft.Extensions.Options;
using BankSystem.Options;
using System.Net;
using BankSystem.Services;



namespace BankSystem.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MTNMomoController : ControllerBase
  {
    private readonly MTNMomoService _momoService;

    public MTNMomoController(MTNMomoService momoService)
    {
      _momoService = momoService;
    }

    [HttpPost("token")]
    public async Task<IActionResult> CreateAccessToken()
    {
      try
      {
        var token = await _momoService.GetAccessTokenAsync();
        return Ok(new { access_token = token });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
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
        return BadRequest(new { message = "Invalid request parameters." });
      }

      try
      {
        var result = await _momoService.RequestToPayAsync(model);
        return Ok(new { message = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while creating the request to pay: {ex.Message}" });
      }
    }
    // create payment 
    [HttpPost("create-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePayment model)
    {
      if (model == null ||
          model.Money == null ||
          string.IsNullOrEmpty(model.Money.Amount) ||
          string.IsNullOrEmpty(model.Money.Currency) ||
          string.IsNullOrEmpty(model.ExternalTransactionId) ||
          string.IsNullOrEmpty(model.CustomerReference) ||
          string.IsNullOrEmpty(model.ServiceProviderUserName) ||
          model.MaxNumberOfRetries < 0)
      {
        return BadRequest(new { message = "Invalid request parameters." });
      }

      try
      {
        var result = await _momoService.CreatePaymentAsync(model);
        return Ok(new { message = "Payment created successfully.", data = result });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while creating the payment: {ex.Message}" });
      }
    }

    // GET ACCOUNT BALANCE

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
      try
      {
        var balance = await _momoService.GetAccountBalanceAsync();
        return Ok(balance);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while getting account balance: {ex.Message}" });
      }
    }
    // PAYMENT STATUS
    [HttpGet("payment-status/{referenceId}")]
    public async Task<IActionResult> GetPaymentStatus(string referenceId)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(referenceId))
        {
          return BadRequest(new { message = "ReferenceId parameter is required." });
        }

        string accessToken = await _momoService.GetAccessTokenAsync();
        string targetEnvironment = "sandbox";

        var paymentResult = await _momoService.GetPaymentStatusAsync(accessToken, targetEnvironment, referenceId);

        if (paymentResult != null)
        {
          return Ok(paymentResult);
        }
        else
        {
          return NotFound(new { message = "Payment not found." });
        }
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while getting payment status: {ex.Message}" });
      }
    }
    [HttpGet("basicuserinfo/{accountHolderMSISDN}")]
    public async Task<IActionResult> GetBasicUserInfo(string accountHolderMSISDN)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(accountHolderMSISDN))
        {
          return BadRequest(new { message = "Account holder MSISDN parameter is required." });
        }

        var accessToken = await _momoService.GetAccessTokenAsync();
        var basicUserInfo = await _momoService.GetBasicUserInfoAsync(accessToken, "sandbox", accountHolderMSISDN);

        if (basicUserInfo != null)
        {
          var response = new
          {
            GivenName = basicUserInfo.GivenName,
            FamilyName = basicUserInfo.FamilyName,
            Birthdate = basicUserInfo.Birthdate,
            Locale = basicUserInfo.Locale,
            Gender = basicUserInfo.Gender,
            Status = basicUserInfo.Status
           
          };

          return Ok(response);
        }
        else
        {
          return NotFound(new { message = "Basic user information not found." });
        }
      }
      catch (Exception ex)
      {
      
        return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred. Please try again later." });
      }
    }
    [HttpPost("create-invoice")]
    public async Task<IActionResult> CreateInvoiceModel([FromBody] CreateInvoiceModel model)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        await _momoService.CreateInvoiceAsync(model);
        return Ok(new { message = "Invoice created successfully." });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while creating the invoice: {ex.Message}" });
      }
    }

  }
}
