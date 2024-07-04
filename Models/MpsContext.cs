using Microsoft.EntityFrameworkCore;
using MyPCSpec.Models.DAO;

namespace MyPCSpec.Models
{
    public class MpsContext : DbContext
    {
        public MpsContext(DbContextOptions<MpsContext> options) : base(options) { }

        // DbSet 정의
        public DbSet<Member> Member { get; set; }
        public DbSet<LoginHistory> LoginHistory { get; set; }
    }
}
