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

  }
}
