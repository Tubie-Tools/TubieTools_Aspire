using Microsoft.EntityFrameworkCore;
using TubieTools_Map.Data;
using TubieTools_Map.Data.Models;

namespace TubieTools_Map.Services;

public class AccountService
{
    private readonly MapAppDbContext _context;
    private readonly ILogger<AccountService> _logger;

    public AccountService(MapAppDbContext context, ILogger<AccountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Account?> RegisterAsync(string email, string fullName,
        string? organization, string? entraObjectId)
    {
        try
        {
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == email);

            if (existingAccount != null)
            {
                _logger.LogWarning("Account registration attempted for existing email: {Email}", email);
                return existingAccount;
            }

            var account = new Account
            {
                Email = email,
                FullName = fullName,
                Organization = organization,
                EntraObjectId = entraObjectId,
                Role = "User"
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Account registered: {Email}", email);
            return account;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering account");
            return null;
        }
    }

    public async Task<Account?> GetAccountByEmailAsync(string email)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<Account?> GetOrCreateAccountAsync(string email, string fullName, string? entraObjectId)
    {
        var account = await GetAccountByEmailAsync(email);
        if (account != null)
        {
            account.LastLoginDate = DateTime.UtcNow;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return account;
        }

        return await RegisterAsync(email, fullName, null, entraObjectId);
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        return await _context.Accounts
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> UpdateAccountRoleAsync(string email, string role)
    {
        try
        {
            var account = await GetAccountByEmailAsync(email);
            if (account == null)
                return false;

            account.Role = role;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account role");
            return false;
        }
    }
}