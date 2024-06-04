using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using BankSystem.Services;
using BankSystem.Models;

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

    [HttpPost("create-payments")]
    public async Task<IActionResult> CreatePayments([FromBody] CreatePayment model)
    {
      if (model == null || string.IsNullOrEmpty(model.ExternalTransactionId) || model.Money == null ||
          string.IsNullOrEmpty(model.Money.Amount) || string.IsNullOrEmpty(model.Money.Currency) ||
          string.IsNullOrEmpty(model.CustomerReference) || string.IsNullOrEmpty(model.ServiceProviderUserName))
      {
        return BadRequest("Invalid request parameters.");
      }

      try
      {
        var result = await _mtnMomoService.CreatePaymentsAsync(model);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred while creating the payments: {ex.Message}");
      }
    }
  }
}
