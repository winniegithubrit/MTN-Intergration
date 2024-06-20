using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankSystem.Services;
using Microsoft.Extensions.Logging;
using BankSystem.Models;

namespace BankSystem.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MTNDisbursementController : ControllerBase
  {
    private readonly MTNDisbursementService _mtnService;
    private readonly ILogger<MTNDisbursementController> _logger;

    public MTNDisbursementController(MTNDisbursementService mtnService, ILogger<MTNDisbursementController> logger)
    {
      _mtnService = mtnService ?? throw new ArgumentNullException(nameof(mtnService));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("token")]
    public async Task<IActionResult> CreateAccessToken()
    {
      try
      {
        var token = await _mtnService.GetAccessTokenAsync();
        return Ok(new { access_token = token });
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error creating access token: {ex.Message}");
        return StatusCode(500, new { error = ex.Message });
      }
    }
    // DEPOSIT FUNCTIONALITY
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] Deposit model)
    {
      if (model == null ||
          string.IsNullOrEmpty(model.Amount) ||
          string.IsNullOrEmpty(model.Currency) ||
          string.IsNullOrEmpty(model.ExternalId) ||
          model.Payee == null ||
          string.IsNullOrEmpty(model.Payee.PartyIdType) ||
          string.IsNullOrEmpty(model.Payee.PartyId))
      {
        return BadRequest(new { message = "Invalid request parameters." });
      }

      try
      {
        var result = await _mtnService.DepositAsync(model);
        return Ok(new { message = result });
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while creating the deposit: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while creating the deposit: {ex.Message}" });
      }
    }
    // BALANCE
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
      try
      {
        var balance = await _mtnService.GetAccountBalanceAsync();
        return Ok(balance);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while getting account balance: {ex.Message}" });
      }
    }
    // BASIC USER INFO
    [HttpGet("basicuserinfo/{accountHolderMSISDN}")]
    public async Task<IActionResult> GetBasicUserInfo(string accountHolderMSISDN)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(accountHolderMSISDN))
        {
          return BadRequest(new { message = "Account holder MSISDN parameter is required." });
        }

        var accessToken = await _mtnService.GetAccessTokenAsync();
        var basicUserInfo = await _mtnService.GetBasicUserInfoAsync(accessToken, "sandbox", accountHolderMSISDN);

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
    // DEPOSIT STATUS
    [HttpGet("deposit-status/{referenceId}")]
    public async Task<IActionResult> DepositStatusResponse(string referenceId)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(referenceId))
        {
          return BadRequest(new { message = "ReferenceId parameter is required." });
        }

        string accessToken = await _mtnService.GetAccessTokenAsync();
        string targetEnvironment = "sandbox";

        var depositResult = await _mtnService.GetDepositStatusAsync(accessToken, targetEnvironment, referenceId);

        if (depositResult != null)
        {
          return Ok(depositResult);
        }
        else
        {
          return NotFound(new { message = "deposit not found." });
        }
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = $"An error occurred while getting deposit status: {ex.Message}" });
      }
    }
    // REFUND FUNCTIONALITY
    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] RefundModel model)
    {
      if (model == null ||
          string.IsNullOrEmpty(model.Amount) ||
          string.IsNullOrEmpty(model.Currency) ||
          string.IsNullOrEmpty(model.ExternalId) ||
          string.IsNullOrEmpty(model.ReferenceIdToRefund))
      {
        return BadRequest(new { message = "Invalid request parameters." });
      }

      try
      {
        var result = await _mtnService.RefundAsync(model);
        return Ok(new { message = result });
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing the refund: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while processing the refund: {ex.Message}" });
      }
    }


  }
}
