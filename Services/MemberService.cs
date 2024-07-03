using MyPCSpec.Services.Interfaces;
using MyPCSpec.Models.DAO;
using Microsoft.EntityFrameworkCore;
using MyPCSpec.Models;

namespace MyPCSpec.Services
{
    public class MemberService : IMemberService
    {
        private readonly MpsContext _mpsContext;

        public MemberService(MpsContext mpsContext)
        {
            _mpsContext = mpsContext;
        }

        public async Task<List<Member>> GetAll()
        {
            var datas = await _mpsContext.Member.ToListAsync();

            return datas;
        }

        public async Task InsertAsync(Member member)
        {
            await _mpsContext.Member.AddAsync(member);
        }
    }
}
