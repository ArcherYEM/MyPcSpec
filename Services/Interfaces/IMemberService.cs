using MyPCSpec.Models.DAO;

namespace MyPCSpec.Services.Interfaces
{
    public interface IMemberService
    {
        Task<Member> Get(string id);
        Task InsertAsync(Member member);
        void Update(Member member);
        Task<bool> Find(string column, string data);
    }
}
