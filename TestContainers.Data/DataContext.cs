using Microsoft.EntityFrameworkCore;

namespace TestContainers.Data
{
    public class DataContext : DbContext
    {
        public virtual DbSet<Message> Messages { get; set; }

    }
}
