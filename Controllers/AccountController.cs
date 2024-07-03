using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPCSpec.Models;
using MyPCSpec.Models.DAO;
using MyPCSpec.Models.DTO;
using MyPCSpec.Services;
using MyPCSpec.Services.Interfaces;
using MySqlConnector;
using System.Security.Cryptography;
using System.Text;

namespace MyPCSpec.Controllers
{
    public class AccountController : Controller
	{
        private readonly ILogger<AccountController> _logger;
        private readonly IMemberService _memberService;
        private readonly MpsContext _mpsContext;

        public AccountController(
            ILogger<AccountController> logger,
            IMemberService memberService,
            MpsContext mpsContext
        )
        {
            _logger = logger;
            _memberService = memberService;
            _mpsContext = mpsContext;
        }

        public IActionResult Login()
		{
			return View();
		}

        public IActionResult Join()
        {
            return View();
        }

        /// <summary>
        /// 회원가입 Insert
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Join([FromBody] RequestJoin req)
        {
            if (req == null)
            {
                return BadRequest("비어있는 항목이 존재합니다.");
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
            try
            {
                await _memberService.InsertAsync(member);
                await _mpsContext.SaveChangesAsync();
            }
            catch (DbUpdateException duEx)
            {
                if (duEx.InnerException is MySqlException sqlEx)
                {
                    if (sqlEx.Message.Contains("Duplicate entry"))
                    {
                        string duplicateField = GetDuplicateField(sqlEx.Message);
                        return BadRequest($"사용중인 정보 : {duplicateField}");
                    }
                    _logger.LogError(sqlEx, "회원가입 중 오류 발생");
                    return StatusCode(500, "서버 오류가 발생했습니다.");
                }
                _logger.LogError(duEx, "회원가입 중 오류 발생");
                return StatusCode(500, "서버 오류가 발생했습니다.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원가입 중 오류 발생");
                return StatusCode(500, "서버 오류가 발생했습니다.");
            }

            return Ok();
        }

        /// <summary>
        /// 비밀번호 암호화
        /// </summary>
        /// <param name="pw"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 회원가입시 중복 데이터 추출
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private string GetDuplicateField(string errorMessage)
        {
            if (errorMessage.Contains("for key 'member.Id'"))
            {
                return "아이디";
            }
            if (errorMessage.Contains("for key 'member.Email'"))
            {
                return "이메일";
            }
            if (errorMessage.Contains("for key 'member.Phone'"))
            {
                return "연락처";
            }
            return "알 수 없는 필드";
        }
    }
}
