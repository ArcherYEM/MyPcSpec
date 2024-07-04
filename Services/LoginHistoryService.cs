using MyPCSpec.Services.Interfaces;
using MyPCSpec.Models.DAO;
using Microsoft.EntityFrameworkCore;
using MyPCSpec.Models;

namespace MyPCSpec.Services
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly MpsContext _mpsContext;

        public LoginHistoryService(MpsContext mpsContext)
        {
            _mpsContext = mpsContext;
        }

        public async Task InsertAsync(LoginHistory loginHistory)
        {
            await _mpsContext.LoginHistory.AddAsync(loginHistory);
        }
    }
}