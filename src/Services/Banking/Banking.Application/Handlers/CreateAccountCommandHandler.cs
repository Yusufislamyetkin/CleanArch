using Enterprise.Services.Banking.Application.Commands;
using Enterprise.Services.Banking.Domain.Services;
using Enterprise.Services.Banking.Domain.Model;
using Enterprise.Services.Banking.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Application.Handlers;

/// <summary>
/// Command handler for creating accounts
/// Orchestrates the account creation process
/// </summary>
public class CreateAccountCommandHandler : MediatR.IRequestHandler<CreateAccountCommand, CreateAccountResult>
{
    private readonly IAccountDomainService _accountDomainService;
    private readonly IAccountNumberGeneratorDomainService _accountNumberGenerator;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(
        IAccountDomainService accountDomainService,
        IAccountNumberGeneratorDomainService accountNumberGenerator,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountDomainService = accountDomainService ?? throw new ArgumentNullException(nameof(accountDomainService));
        _accountNumberGenerator = accountNumberGenerator ?? throw new ArgumentNullException(nameof(accountNumberGenerator));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<CreateAccountResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Validate account creation requirements
        await _accountDomainService.ValidateAccountCreationAsync(
            request.CustomerId,
            request.AccountType,
            request.InitialBalance,
            cancellationToken);

        // Generate unique account number
        var accountNumber = await _accountNumberGenerator.GenerateUniqueAccountNumberAsync(
            request.AccountType,
            cancellationToken);

        // Create account (domain logic)
        var account = Account.Create(
            accountNumber,
            request.CustomerId,
            request.AccountName,
            Enum.Parse<AccountType>(request.AccountType),
            request.InitialBalance,
            request.MinimumBalance,
            request.DailyTransactionLimit);

        // Save to repository
        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return result
        return new CreateAccountResult(
            account.Id,
            account.AccountNumber,
            account.CreatedAt);
    }
}

/// <summary>
/// Command handler for depositing money
/// </summary>
public class DepositMoneyCommandHandler : MediatR.IRequestHandler<DepositMoneyCommand, DepositResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositMoneyCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<DepositResult> Handle(DepositMoneyCommand request, CancellationToken cancellationToken)
    {
        // Get account
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new KeyNotFoundException($"Account {request.AccountId} not found");

        // Perform deposit (domain logic)
        account.Deposit(request.Amount, request.Description);

        // Save changes
        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the transaction that was just created
        var transaction = account.Transactions.Last();

        return new DepositResult(
            account.Id,
            transaction.Id,
            request.Amount,
            account.Balance);
    }
}

/// <summary>
/// Command handler for withdrawing money
/// </summary>
public class WithdrawMoneyCommandHandler : MediatR.IRequestHandler<WithdrawMoneyCommand, MediatR.Unit>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WithdrawMoneyCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<MediatR.Unit> Handle(WithdrawMoneyCommand request, CancellationToken cancellationToken)
    {
        // Get account
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new KeyNotFoundException($"Account {request.AccountId} not found");

        // Perform withdrawal (domain logic)
        account.Withdraw(request.Amount, request.Description);

        // Save changes
        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}

/// <summary>
/// Command handler for transferring money
/// </summary>
public class TransferMoneyCommandHandler : MediatR.IRequestHandler<TransferMoneyCommand, TransferResult>
{
    private readonly IAccountDomainService _accountDomainService;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferMoneyCommandHandler(
        IAccountDomainService accountDomainService,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountDomainService = accountDomainService ?? throw new ArgumentNullException(nameof(accountDomainService));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<TransferResult> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
    {
        // Get both accounts
        var fromAccount = await _accountRepository.GetByIdAsync(request.FromAccountId, cancellationToken);
        if (fromAccount == null)
            throw new KeyNotFoundException($"From account {request.FromAccountId} not found");

        var toAccount = await _accountRepository.GetByIdAsync(request.ToAccountId, cancellationToken);
        if (toAccount == null)
            throw new KeyNotFoundException($"To account {request.ToAccountId} not found");

        // Perform transfer using domain service
        await _accountDomainService.TransferBetweenAccountsAsync(
            fromAccount,
            toAccount,
            request.Amount,
            request.Description,
            cancellationToken);

        // Save changes
        await _accountRepository.UpdateAsync(fromAccount, cancellationToken);
        await _accountRepository.UpdateAsync(toAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransferResult(
            Guid.NewGuid(), // Transfer ID
            fromAccount.Id,
            toAccount.Id,
            request.Amount,
            fromAccount.Balance,
            toAccount.Balance);
    }
}

/// <summary>
/// Command handler for updating account name
/// </summary>
public class UpdateAccountNameCommandHandler : MediatR.IRequestHandler<UpdateAccountNameCommand, MediatR.Unit>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountNameCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<MediatR.Unit> Handle(UpdateAccountNameCommand request, CancellationToken cancellationToken)
    {
        // Get account
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new KeyNotFoundException($"Account {request.AccountId} not found");

        // Update name (domain logic)
        account.UpdateName(request.NewName);

        // Save changes
        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}

/// <summary>
/// Command handler for closing account
/// </summary>
public class CloseAccountCommandHandler : MediatR.IRequestHandler<CloseAccountCommand, MediatR.Unit>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseAccountCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<MediatR.Unit> Handle(CloseAccountCommand request, CancellationToken cancellationToken)
    {
        // Get account
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new KeyNotFoundException($"Account {request.AccountId} not found");

        // Close account (domain logic)
        account.Close();

        // Save changes
        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}
