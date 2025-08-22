using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.Infrastructure.Data
{
    public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
    {
        public InventoryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

            // Use the Session Pooler connection string
            var connectionString = "Host=aws-1-ap-south-1.pooler.supabase.com; " +
                                   "Database=postgres; " +
                                   "Username=postgres.tksonejmooqlovsxvvda; " +
                                   "Password=SpinyardDatabase; " +
                                   "Port=5432; " + // <-- Note the port is 5432
                                   "SSL Mode=Require; " +
                                   "Trust Server Certificate=true";

            optionsBuilder.UseNpgsql(connectionString, o => o.CommandTimeout(90));

            return new InventoryDbContext(optionsBuilder.Options);
        }
    }
}