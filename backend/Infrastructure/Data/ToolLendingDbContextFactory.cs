using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ToolLendingPlatform.Infrastructure.Data
{
    public class ToolLendingDbContextFactory : IDesignTimeDbContextFactory<ToolLendingDbContext>
    {
        public ToolLendingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ToolLendingDbContext>();
            optionsBuilder.UseSqlite("Data Source=tool_lending.db");

            return new ToolLendingDbContext(optionsBuilder.Options);
        }
    }
}
