using ExchangeAPI.Data;
using ExchangeAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExchangeAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController : ControllerBase
{
    private readonly DataContext _context;

    public WalletController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Wallet>>> GetAllWallets()
    {
        var wallets = await _context.Wallets.ToListAsync();

        return Ok(wallets);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Wallet>> GetWallet(int id)
    {
        var wallet = await _context.Wallets.FindAsync(id);

        return wallet == null ? NotFound() : Ok(wallet);
    }

    [HttpPost]
    public async Task<ActionResult<Wallet>> CreateWallet([FromBody] CreateWalletModel createWallet)
    {
        var wallet = new Wallet
        {
            Currency = createWallet.Currency
        };

        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, wallet);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> Deposit(int id, [FromBody] BankOperationModel bankOperationModel)
    {
        var wallet = await _context.Wallets.FindAsync(id);

        if (wallet == null)
        {
            return NotFound();
        }

        switch (bankOperationModel.OperationType)
        {
            case OperationType.Deposit:
                wallet.Balance += bankOperationModel.Amount;
                break;
            case OperationType.Withdraw:
                wallet.Balance -= bankOperationModel.Amount;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await _context.SaveChangesAsync();

        return Ok(wallet);
    }

    [HttpPost("exchange")]
    public async Task<ActionResult> ExchangeBalance([FromBody] ExchangeBalanceModel exchangeBalanceModel)
    {
        if (exchangeBalanceModel.FromWalletId == exchangeBalanceModel.ToWalletId)
        {
            return BadRequest("Wallets must be different");
        }

        var fromWallet = await _context.Wallets.FindAsync(exchangeBalanceModel.FromWalletId);
        var toWallet = await _context.Wallets.FindAsync(exchangeBalanceModel.ToWalletId);

        if (fromWallet == null || toWallet == null)
        {
            return NotFound();
        }

        if (fromWallet.Balance <= 0)
        {
            return UnprocessableEntity("Insufficient balance");
        }

        fromWallet.Balance -= exchangeBalanceModel.Amount;
        toWallet.Balance += exchangeBalanceModel.Amount * fromWallet.ExchangeRate;
        await _context.SaveChangesAsync();

        return Ok(new List<Wallet>() { fromWallet, toWallet });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteWallet(int id)
    {
        var wallet = await _context.Wallets.FindAsync(id);

        if (wallet == null)
        {
            return NotFound();
        }

        _context.Wallets.Remove(wallet);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateWalletModel
{
    public required Currency Currency { get; set; }
}

public class BankOperationModel
{
    public required OperationType OperationType { get; set; }
    public required decimal Amount { get; set; }
}

public class ExchangeBalanceModel
{
    public required int FromWalletId { get; set; }
    public required int ToWalletId { get; set; }
    public required decimal Amount { get; set; }
}

public enum OperationType
{
    Deposit,
    Withdraw
}