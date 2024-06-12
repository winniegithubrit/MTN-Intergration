using System;
using System.Threading.Tasks;
using BankSystem.Models;
using BankSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BankSystem.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MTNDisbursementController : ControllerBase
  {
    private readonly MTNDisbursementService _mtnDisbursementService;
    private readonly ILogger<MTNDisbursementController> _logger;

    public MTNDisbursementController(MTNDisbursementService mtnDisbursementService, ILogger<MTNDisbursementController> logger)
    {
      _mtnDisbursementService = mtnDisbursementService;
      _logger = logger;
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] Deposit deposit)
    {
      try
      {
        if (deposit == null)
        {
          return BadRequest("Invalid deposit object.");
        }

        var result = await _mtnDisbursementService.DepositAsync(deposit);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing deposit: {ex.Message}");
        return StatusCode(500, $"An error occurred while processing deposit: {ex.Message}");
      }
    }
    // balance

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
      try
      {
        var balance = await _mtnDisbursementService.GetAccountBalanceAsync();
        return Ok(balance);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while getting account balance: {ex.Message}");
        return StatusCode(500, $"An error occurred while getting account balance: {ex.Message}");
      }
    }
    [HttpGet("user-info/{msisdn}")]
    public async Task<IActionResult> GetUserInfo(string msisdn)
    {
      try
      {
        var userInfo = await _mtnDisbursementService.GetBasicUserInfoAsync(msisdn);
        return Ok(userInfo);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while getting user info for MSISDN {msisdn}: {ex.Message}");
        return StatusCode(500, $"An error occurred while getting user info for MSISDN {msisdn}: {ex.Message}");
      }
    }
    [HttpGet("deposit/{referenceId}")]
    public async Task<IActionResult> GetDepositStatus(string referenceId)
    {
      try
      {
        if (string.IsNullOrEmpty(referenceId))
        {
          return BadRequest("Reference ID is required.");
        }

        var result = await _mtnDisbursementService.GetDepositStatusAsync(referenceId);

        if (result == null)
        {
          return NotFound("Deposit not found.");
        }

        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while getting deposit status: {ex.Message}");
        return StatusCode(500, $"An error occurred while getting deposit status: {ex.Message}");
      }
    }
    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] Refund model)
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
        var result = await _mtnDisbursementService.RefundAsync(model);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing the refund: {ex.Message}");
        return StatusCode(500, new { message = $"An error occurred while processing the refund: {ex.Message}" });
      }
    }
  }
}
