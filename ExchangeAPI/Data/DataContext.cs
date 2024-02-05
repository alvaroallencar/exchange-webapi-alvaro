using ExchangeAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeAPI.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Wallet> Wallets { get; set; }
}