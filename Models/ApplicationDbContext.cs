using Microsoft.EntityFrameworkCore;
using MyPCSpec.Models.DAO;

namespace MyPCSpec.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet 정의
        public DbSet<Member> Member { get; set; }
    }
}
