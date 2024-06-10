using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using BankSystem.Services;
using BankSystem.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;


namespace BankSystem.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Produces("application/json")] 
  public class MTNMomoController : ControllerBase
  {
    private readonly MtnMomoService _mtnMomoService;
    private readonly ILogger<MTNMomoController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;


    public MTNMomoController(MtnMomoService mtnMomoService, ILogger<MTNMomoController> logger, ApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
      _mtnMomoService = mtnMomoService ?? throw new ArgumentNullException(nameof(mtnMomoService));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _context = context ?? throw new ArgumentNullException(nameof(context));
      _httpClientFactory = httpClientFactory;
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
        var result = await _mtnMomoService.RequestToPayAsync(model);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while creating the request to pay: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while creating the request to pay: {ex.Message}" });
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
        return BadRequest(new { message = "Invalid request parameters." });
      }

      try
      {
        _context.CreatePayments.Add(model);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Payment created successfully.", externalTransactionId = model.ExternalTransactionId });
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while creating the payment: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while creating the payment: {ex.Message}" });
      }
    }

    [HttpGet("payment-status/{xReferenceId}")]
    public async Task<IActionResult> GetPaymentStatus(string xReferenceId)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(xReferenceId))
        {
          return BadRequest(new { message = "X-Reference-Id parameter is required." });
        }

        string accessToken = "your_access_token";
        string targetEnvironment = "sandbox";
        string subscriptionKey = "your_subscription_key";
        var paymentStatus = await _mtnMomoService.GetPaymentStatusAsync(accessToken, targetEnvironment, subscriptionKey, xReferenceId);

        return Ok(paymentStatus);
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError($"HTTP request failed: {ex.Message}");
        if (ex.StatusCode.HasValue)
        {
          return StatusCode((int)ex.StatusCode, new { message = ex.Message });
        }
        else
        {
          return StatusCode(500, new { message = "An error occurred while getting payment status. Please try again later." });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while getting payment status: {ex.Message}" });
      }
    }


    [HttpGet("account-balance")]
    public async Task<IActionResult> GetAccountBalance()
    {
      try
      {
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTEwVDA5OjQ5OjA1LjczMiIsInNlc3Npb25JZCI6IjVlY2U1NjUyLWE3ZWMtNDE5NC04ODA5LWNjNDZjNjE0Mjk4MiJ9.GyXrf6GhVhSBCPm1xdA_xWs_QPXJYHDyRNcMyKu6u5PDjNMbUKh34AAESWfGVbGwaGp93R8IzFLYiCdRNS8lpPravGkaJeDAupvQUnQlFRo-Ectux9IAW8Y1HE4V7C_F3_pz9A_PzTHRhrFe_5SHcHtkJWVEiFo3wBRalYFk04gKIoPSMy1vW9RFAlKYGZ97UWd8-YbiVWUC-7GNId4CeTIYv2vm2WDXQOr1MNy45yfdeP2-zucI6a1OXGiQKSOn-wXV6cK7liUAjRbAFBlSOxcLg9kb1Rjxmu81xM6U-yTmjqJdzxBHPyJbap8plz6CbnRto4MhqaQD8Emr6taA1w";
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
          return BadRequest(new { message = "Currency parameter is required." });
        }
        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTEwVDA5OjQ5OjA1LjczMiIsInNlc3Npb25JZCI6IjVlY2U1NjUyLWE3ZWMtNDE5NC04ODA5LWNjNDZjNjE0Mjk4MiJ9.GyXrf6GhVhSBCPm1xdA_xWs_QPXJYHDyRNcMyKu6u5PDjNMbUKh34AAESWfGVbGwaGp93R8IzFLYiCdRNS8lpPravGkaJeDAupvQUnQlFRo-Ectux9IAW8Y1HE4V7C_F3_pz9A_PzTHRhrFe_5SHcHtkJWVEiFo3wBRalYFk04gKIoPSMy1vW9RFAlKYGZ97UWd8-YbiVWUC-7GNId4CeTIYv2vm2WDXQOr1MNy45yfdeP2-zucI6a1OXGiQKSOn-wXV6cK7liUAjRbAFBlSOxcLg9kb1Rjxmu81xM6U-yTmjqJdzxBHPyJbap8plz6CbnRto4MhqaQD8Emr6taA1w";
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
          return StatusCode((int)ex.StatusCode, new { message = ex.Message });
        }
        else
        {
          return StatusCode(500, new { message = "An error occurred while fetching account balance. Please try again later." });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while fetching account balance: {ex.Message}" });
      }
    }

    [HttpGet("basic-user-info/{accountHolderMSISDN}")]
    public async Task<IActionResult> GetBasicUserInfo(string accountHolderMSISDN)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(accountHolderMSISDN))
        {
          return BadRequest(new { message = "Account holder MSISDN parameter is required." });
        }

        string accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSMjU2In0.eyJjbGllbnRJZCI6IjRhYmQ5Y2YzLTk4YTUtNDhmMy1iMmZhLWJkNmIzMjZjYjYzNSIsImV4cGlyZXMiOiIyMDI0LTA2LTEwVDE3OjA3OjQ4Ljk2MCIsInNlc3Npb25JZCI6Ijg1NDY2MzEyLTczNTMtNDBiOS1iYTZjLTg2MjU0MjY5MjNjMyJ9.T-7zTfEPg_nDEjPidvdRkEK1dpyOzVrbj4vsUGah_JPBUbK6xej1lD4pH1UiEUpnvOsSalpi_dKXU3bUUW9__A1Zyo9u7UhN4rxIQCbSl8FAyoYyvBPMZTwh4r37me-m-0a2MC_CV53WED5Z7VC-CUVEu7sSWB9h6nUcy-6sKRy0ZPlr1dDPh0sKGesXHNgAWesWDC-6ua0pR-WORa4VNS6sg_oRO8W-LUWqmhmORWyRUKrSvasoaCUYT_h6HjxPefiHHntR1N0S7yNHEieWUREZOlY1V2xy7lhAlFoq8sh4OH4ymG9wro2hI3-NKZqgFLt4vfz-RKLa1oqWSYBZjQ";
        string targetEnvironment = "sandbox";
        string subscriptionKey = "184789bdc53f4c05870da82d1c307b63";
        var result = await _mtnMomoService.GetBasicUserInfoAsync(accessToken, targetEnvironment, subscriptionKey, accountHolderMSISDN);
        return Ok(result);
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError($"HTTP request failed: {ex.Message}");

        if (ex.StatusCode.HasValue)
        {
          return StatusCode((int)ex.StatusCode, new { message = ex.Message });
        }
        else
        {
          return StatusCode(500, new { message = "An error occurred while fetching basic user information. Please try again later." });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while fetching basic user information: {ex.Message}" });
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
        return BadRequest(new { message = "Invalid request parameters." });
      }

      try
      {
        var result = await _mtnMomoService.CreateInvoiceAsync(model);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while creating the invoice: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while creating the invoice: {ex.Message}" });
    
    }
  }

    [HttpGet("get-invoice-status/{externalId}")]
    public async Task<IActionResult> GetInvoiceStatus(string externalId)
    {
      if (string.IsNullOrEmpty(externalId))
      {
        return BadRequest(new { message = "Invalid request parameters." });
      }

      _logger.LogInformation($"Fetching status for invoice with ExternalId: {externalId}");

      try
      {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.ExternalId == externalId);
        if (invoice == null)
        {
          _logger.LogWarning($"Invoice with ExternalId: {externalId} not found.");
          return NotFound(new { message = "Invoice not found." });
        }

        string statusMessage = string.Empty;

        if (invoice.Status == "Paid")
        {
          statusMessage = "Paid";
        }
        else
        {
          statusMessage = "Pending";
        }

        _logger.LogInformation($"Invoice status for ExternalId: {externalId} is {statusMessage}");
        return Ok(new { status = statusMessage });
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while getting the invoice status: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while getting the invoice status: {ex.Message}" });
      }
    }

    // delete the invoice
    [HttpPost("cancel-invoice/{referenceId}")]
    public async Task<IActionResult> CancelInvoice([FromBody] GetInvoiceStatusModel model, [FromHeader(Name = "Authorization")] string authorization, [FromHeader(Name = "X-Target-Environment")] string targetEnvironment, [FromHeader(Name = "X-Reference-Id")] string referenceId, [FromHeader(Name = "X-Callback-Url")] string callbackUrl)
    {
      if (!ModelState.IsValid || string.IsNullOrEmpty(model.ExternalId) || string.IsNullOrEmpty(authorization) || string.IsNullOrEmpty(targetEnvironment) || string.IsNullOrEmpty(referenceId))
      {
        return BadRequest(new { message = "Invalid request parameters." });
      }

      try
      {
        string requestUrl = $"https://sandbox.momodeveloper.mtn.com/collection/v2_0/invoice/{referenceId}";

        var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);

        request.Headers.Add("Authorization", authorization);
        request.Headers.Add("X-Target-Environment", targetEnvironment);
        request.Headers.Add("X-Reference-Id", referenceId);
        request.Headers.Add("X-Callback-Url", callbackUrl);

        HttpResponseMessage response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
          await _mtnMomoService.CancelInvoiceAsync(model.ExternalId);
          return Ok("Invoice canceled successfully.");
        }
        else
        {
          string errorMessage = $"Failed to cancel invoice. Status code: {response.StatusCode}";
          _logger.LogError(errorMessage);
          return StatusCode((int)response.StatusCode, new { message = errorMessage });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while canceling the invoice: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while canceling the invoice: {ex.Message}" });
      }
    }



  }
}
