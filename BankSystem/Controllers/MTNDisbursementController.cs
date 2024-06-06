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
      _mtnDisbursementService = mtnDisbursementService ?? throw new ArgumentNullException(nameof(mtnDisbursementService));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] Refund refund)
    {
      try
      {
        if (refund == null)
        {
          return BadRequest("Invalid refund object.");
        }

        var result = await _mtnDisbursementService.RefundAsync(refund);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing refund: {ex.Message}");
        return StatusCode(500, $"An error occurred while processing refund: {ex.Message}");
      }
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] Transfer transfer)
    {
      try
      {
        if (transfer == null)
        {
          return BadRequest("Invalid transfer object.");
        }

        var result = await _mtnDisbursementService.TransferAsync(transfer);
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogError($"An error occurred while processing transfer: {ex.Message}");
        return StatusCode(500, $"An error occurred while processing transfer: {ex.Message}");
      }
    }
  }
}
