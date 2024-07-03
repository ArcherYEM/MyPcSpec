using Microsoft.AspNetCore.Mvc;
using MyPCSpec.Models.DAO;
using MyPCSpec.Models.DTO;
using System.Security.Cryptography;
using System.Text;

namespace MyPCSpec.Controllers
{
    public class AccountController : Controller
	{
		private readonly ILogger<AccountController> _logger;

		public AccountController(ILogger<AccountController> logger)
		{
			_logger = logger;
		}

		public IActionResult Login()
		{
			return View();
		}

        public IActionResult Join()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Join([FromBody] RequestJoin req)
        {
            if (req == null)
            {
                return StatusCode(500, "비어있는 항목이 존재합니다.");
            }

            if (!DateTime.TryParseExact(req.Birth, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime birthDT))
            {
                return BadRequest("생년월일은 yyyyMMdd 형식으로 입력해주세요.");
            }

            Member member = new Member()
            {
                Id = req.Id,
                Pw = SecurityPw(req.Pw),
                Name = req.Name,
                Birth = birthDT,
                Email = req.Email,
                Phone = req.Phone,
                UseYn = 'Y',
                DelYn = 'N',
                CreatedAt = DateTime.Now
            };

            // member 테이블에서 id, email, phone 컬럼 풀스캔 후 중복여부 검수

            // member 테이블 insertAsync

            return Ok();
        }

        private string SecurityPw(string pw)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(pw));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
