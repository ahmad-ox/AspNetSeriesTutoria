using Host.Models;
using Microsoft.EntityFrameworkCore;

namespace Host.ContextData;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Base64FileModel> Base64FileModels => Set<Base64FileModel>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Owner> Owners => Set<Owner>();
}
