using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Application.Commands;
using Enterprise.Services.Banking.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.Banking.Api.Controllers;

/// <summary>
/// Accounts API controller
/// Handles HTTP requests for account operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Creates a new account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateAccountResult), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            var command = new CreateAccountCommand(
                CustomerId.From(request.CustomerId),
                AccountNumber.From(request.AccountNumber),
                request.AccountName,
                request.AccountType,
                Money.Of(request.InitialBalance, request.Currency),
                request.MinimumBalance.HasValue ? Money.Of(request.MinimumBalance.Value, request.Currency) : null,
                request.DailyTransactionLimit.HasValue ? Money.Of(request.DailyTransactionLimit.Value, request.Currency) : null);

            var result = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetAccountById),
                new { id = result.AccountId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets account by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountDetailsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var query = new GetAccountByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets account by account number
    /// </summary>
    [HttpGet("by-number/{accountNumber}")]
    [ProducesResponseType(typeof(AccountDetailsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAccountByNumber(string accountNumber)
    {
        var query = new GetAccountByNumberQuery(AccountNumber.From(accountNumber));
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets accounts by customer ID
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(PaginatedResponse<CustomerAccountSummaryDto>), 200)]
    public async Task<IActionResult> GetCustomerAccounts(
        Guid customerId,
        [FromQuery] bool includeClosed = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetCustomerAccountsQuery(
            CustomerId.From(customerId),
            includeClosed)
        {
            Pagination = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            }
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Searches accounts
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedResponse<AccountSummaryDto>), 200)]
    public async Task<IActionResult> SearchAccounts(
        [FromQuery] string? searchTerm,
        [FromQuery] string? accountType,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        // Parse enums safely
        AccountType? accountTypeEnum = null;
        if (!string.IsNullOrEmpty(accountType) && Enum.TryParse<AccountType>(accountType, true, out var type))
        {
            accountTypeEnum = type;
        }

        var query = new SearchAccountsQuery(searchTerm, accountType, status)
        {
            Pagination = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            }
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Deposits money to account
    /// </summary>
    [HttpPost("{id}/deposit")]
    [ProducesResponseType(typeof(DepositResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DepositMoney(Guid id, [FromBody] MoneyTransactionRequest request)
    {
        try
        {
            var command = new DepositMoneyCommand(
                id,
                Money.Of(request.Amount, request.Currency),
                request.Description);

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Withdraws money from account
    /// </summary>
    [HttpPost("{id}/withdraw")]
    [ProducesResponseType(typeof(WithdrawResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> WithdrawMoney(Guid id, [FromBody] MoneyTransactionRequest request)
    {
        try
        {
            var command = new WithdrawMoneyCommand(
                id,
                Money.Of(request.Amount, request.Currency),
                request.Description);

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Transfers money between accounts
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransferResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> TransferMoney([FromBody] TransferMoneyRequest request)
    {
        try
        {
            var command = new TransferMoneyCommand(
                request.FromAccountId,
                request.ToAccountId,
                Money.Of(request.Amount, request.Currency),
                request.Description);

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates account name
    /// </summary>
    [HttpPut("{id}/name")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAccountName(Guid id, [FromBody] UpdateAccountNameRequest request)
    {
        try
        {
            var command = new UpdateAccountNameCommand(id, request.NewName);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Closes account
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CloseAccount(Guid id)
    {
        try
        {
            var command = new CloseAccountCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets account balance
    /// </summary>
    [HttpGet("{id}/balance")]
    [ProducesResponseType(typeof(AccountBalanceDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAccountBalance(Guid id)
    {
        var query = new GetAccountBalanceQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets account transaction history
    /// </summary>
    [HttpGet("{id}/transactions")]
    [ProducesResponseType(typeof(PaginatedResponse<TransactionDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAccountTransactions(
        Guid id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? transactionType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25)
    {
        var query = new GetAccountTransactionHistoryQuery(id, fromDate, toDate, transactionType)
        {
            Pagination = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            }
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

/// <summary>
/// Request DTOs
/// </summary>
public record CreateAccountRequest(
    Guid CustomerId,
    string AccountNumber,
    string AccountName,
    AccountType AccountType,
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
