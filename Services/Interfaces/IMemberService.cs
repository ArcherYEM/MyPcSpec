using MyPCSpec.Models.DAO;

namespace MyPCSpec.Services.Interfaces
{
    public interface IMemberService
    {
        Task<List<Member>> GetAll();
        Task InsertAsync(Member member);
    }
}
