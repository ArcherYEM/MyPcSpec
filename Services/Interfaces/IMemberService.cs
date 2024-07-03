using MyPCSpec.Models.DAO;

namespace MyPCSpec.Services.Interfaces
{
    public interface IMemberService
    {
        Task InsertAsync(Member member);
        Task<bool> Find(string column, string data);
    }
}
