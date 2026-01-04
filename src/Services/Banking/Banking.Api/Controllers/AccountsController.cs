using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.Banking.Api.Controllers;

/// <summary>
/// Accounts API controller - STUB IMPLEMENTATION
/// For demonstration purposes only
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    /// <summary>
    /// Creates a new account - STUB
    /// </summary>
    [HttpPost]
    [ProducesResponseType(201)]
    public IActionResult CreateAccount([FromBody] CreateAccountRequest request)
    {
        var accountId = Guid.NewGuid();
        var result = new
        {
            AccountId = accountId,
            AccountNumber = "TEST12345678",
            CreatedAt = DateTime.UtcNow,
            Message = "Account created successfully (stub)"
        };

        return CreatedAtAction(nameof(GetAccountById), new { id = accountId }, result);
    }

    /// <summary>
    /// Gets account by ID - STUB
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDetailsDto), 200)]
    public IActionResult GetAccountById(Guid id)
    {
        var result = new AccountDetailsDto(
            Id: id,
            AccountNumber: new AccountNumber("TEST12345678"),
            CustomerId: new CustomerId(Guid.NewGuid()),
            AccountName: "Test Account",
            AccountType: "Savings",
            Status: "Active",
            CurrentBalance: new Money(1000, "TRY"),
            AvailableBalance: new Money(1000, "TRY"),
            MinimumBalance: new Money(100, "TRY"),
            DailyTransactionLimit: new Money(5000, "TRY"),
            TodayTransactionTotal: new Money(0, "TRY"),
            CreatedAt: DateTime.UtcNow,
            LastTransactionAt: null,
            TransactionCount: 0
        );

        return Ok(result);
    }

    /// <summary>
    /// Gets account by account number - STUB
    /// </summary>
    [HttpGet("by-number/{accountNumber}")]
    [ProducesResponseType(200)]
    public IActionResult GetAccountByNumber(string accountNumber)
    {
        return Ok(new { accountNumber, name = "Test Account", balance = 1000.00 });
    }

    /// <summary>
    /// Gets accounts by customer ID - STUB
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(200)]
    public IActionResult GetCustomerAccounts(Guid customerId)
    {
        return Ok(new[] { new { customerId, name = "Test Account", balance = 1000.00 } });
    }

    /// <summary>
    /// Searches accounts - STUB
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(200)]
    public IActionResult SearchAccounts([FromQuery] string? searchTerm)
    {
        return Ok(new[] { new { id = Guid.NewGuid(), name = "Test Account", balance = 1000.00 } });
    }

    /// <summary>
    /// Deposits money to account - STUB
    /// </summary>
    [HttpPost("{id}/deposit")]
    [ProducesResponseType(200)]
    public IActionResult DepositMoney(Guid id, [FromBody] object request)
    {
        return Ok(new { id, message = "Deposit successful (stub)", newBalance = 1100.00 });
    }

    /// <summary>
    /// Withdraws money from account - STUB
    /// </summary>
    [HttpPost("{id}/withdraw")]
    [ProducesResponseType(200)]
    public IActionResult WithdrawMoney(Guid id, [FromBody] object request)
    {
        return Ok(new { id, message = "Withdrawal successful (stub)", newBalance = 900.00 });
    }

    /// <summary>
    /// Transfers money between accounts - STUB
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(200)]
    public IActionResult TransferMoney([FromBody] object request)
    {
        return Ok(new { message = "Transfer successful (stub)" });
    }

    /// <summary>
    /// Updates account name - STUB
    /// </summary>
    [HttpPut("{id}/name")]
    [ProducesResponseType(204)]
    public IActionResult UpdateAccountName(Guid id, [FromBody] object request)
    {
        return NoContent();
    }

    /// <summary>
    /// Closes account - STUB
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    public IActionResult CloseAccount(Guid id)
    {
        return NoContent();
    }

    /// <summary>
    /// Gets account balance - STUB
    /// </summary>
    [HttpGet("{id}/balance")]
    [ProducesResponseType(200)]
    public IActionResult GetAccountBalance(Guid id)
    {
        return Ok(new { id, balance = 1000.00, availableBalance = 900.00 });
    }

    /// <summary>
    /// Gets account transaction history - STUB
    /// </summary>
    [HttpGet("{id}/transactions")]
    [ProducesResponseType(200)]
    public IActionResult GetAccountTransactions(Guid id)
    {
        return Ok(new[] {
            new { id = Guid.NewGuid(), type = "Deposit", amount = 100.00, date = DateTime.UtcNow }
        });
    }
}

/// <summary>
/// Request DTOs - STUB
/// </summary>
public record CreateAccountRequest(
    Guid CustomerId,
    string AccountName,
    string AccountType,
    decimal InitialBalance,
    string Currency = "TRY",
    decimal? MinimumBalance = null,
    decimal? DailyTransactionLimit = null);

public record MoneyTransactionRequest(
    decimal Amount,
    string Currency = "TRY",
    string Description = "");

public record TransferMoneyRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency = "TRY",
    string Description = "");

public record UpdateAccountNameRequest(
    string NewName);