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

        public async Task InsertAsync(Member member)
        {
            await _mpsContext.Member.AddAsync(member);
        }

        public async Task<bool> Find(string column, string data)
        {
            bool exists = true;

            if (column == "Id")
            {
                exists = await _mpsContext.Member.AnyAsync(m => m.Id == data);
            }
            else if (column == "email")
            {
                exists = await _mpsContext.Member.AnyAsync(m => m.Email == data);
            }
            else if (column == "phone")
            {
                exists = await _mpsContext.Member.AnyAsync(m => m.Phone == data);
            }

            return exists;
        }
    }
}
