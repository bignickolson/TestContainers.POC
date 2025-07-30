using Microsoft.EntityFrameworkCore;

namespace TestContainers.Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        
        public virtual DbSet<Message> Messages { get; set; }

    }
}
