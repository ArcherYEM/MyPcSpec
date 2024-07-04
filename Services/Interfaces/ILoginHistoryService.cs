using MyPCSpec.Models.DAO;

namespace MyPCSpec.Services.Interfaces
{
    public interface ILoginHistoryService
    {
        Task InsertAsync(LoginHistory loginHistory);
    }
}
