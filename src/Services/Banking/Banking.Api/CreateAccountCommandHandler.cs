namespace Enterprise.Services.Banking.Api;

/// <summary>
/// Command handler for creating accounts - STUB
/// </summary>
public class CreateAccountCommandHandler : MediatR.IRequestHandler<CreateAccountCommand, CreateAccountResult>
{
    public async Task<CreateAccountResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Stub implementation
        return new CreateAccountResult(
            Guid.NewGuid(),
            new AccountNumber("TEST12345678"),
            DateTime.UtcNow);
    }
}

/// <summary>
/// Command handler for depositing money - STUB
/// </summary>
public class DepositMoneyCommandHandler : MediatR.IRequestHandler<DepositMoneyCommand, DepositResult>
{
    public async Task<DepositResult> Handle(DepositMoneyCommand request, CancellationToken cancellationToken)
    {
        // Stub implementation
        return new DepositResult(
            request.AccountId,
            Guid.NewGuid(),
            request.Amount,
            new Money(1000, "TRY"));
    }
}

/// <summary>
/// Command handler for withdrawing money - STUB
/// </summary>
public class WithdrawMoneyCommandHandler : MediatR.IRequestHandler<WithdrawMoneyCommand, MediatR.Unit>
{
    public async Task<MediatR.Unit> Handle(WithdrawMoneyCommand request, CancellationToken cancellationToken)
    {
        // Stub implementation
        return MediatR.Unit.Value;
    }
}

/// <summary>
/// Command handler for transferring money - STUB
/// </summary>
public class TransferMoneyCommandHandler : MediatR.IRequestHandler<TransferMoneyCommand, TransferResult>
{
    public async Task<TransferResult> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
    {
        // Stub implementation
        return new TransferResult(
            Guid.NewGuid(),
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            new Money(900, "TRY"),
            new Money(1100, "TRY"));
    }
}

/// <summary>
/// Command handler for updating account name - STUB
/// </summary>
public class UpdateAccountNameCommandHandler : MediatR.IRequestHandler<UpdateAccountNameCommand, MediatR.Unit>
{
    public async Task<MediatR.Unit> Handle(UpdateAccountNameCommand request, CancellationToken cancellationToken)
    {
        // Stub implementation
        return MediatR.Unit.Value;
    }
}

/// <summary>
/// Command handler for closing account - STUB
/// </summary>
public class CloseAccountCommandHandler : MediatR.IRequestHandler<CloseAccountCommand, MediatR.Unit>
{
    public async Task<MediatR.Unit> Handle(CloseAccountCommand request, CancellationToken cancellationToken)
    {
        // Stub implementation
        return MediatR.Unit.Value;
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
