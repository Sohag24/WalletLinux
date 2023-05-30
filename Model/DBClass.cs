using Microsoft.EntityFrameworkCore;

namespace WalletApp.Model
{
    public class DBClass : DbContext
    {
        public DBClass(DbContextOptions<DBClass> options)
      : base(options)
        { }
        public DbSet<VaultInfo> VaultInfo { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<TransactionInfo> TransactionInfo { get; set; }

        public DbSet<AccountInfo> accountInfo { get; set; }

        public DbSet<Tokens> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Other configurations...
            modelBuilder.Entity<TransactionInfo>()
                .Property(x => x.Amount)
                .HasPrecision(18, 6); // Update the decimal precision here
            // Other configurations...
        }
    }

    
}
