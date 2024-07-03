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
        /// ȸ������ Insert
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Join([FromBody] RequestJoin req)
        {
            if (req == null)
            {
                return BadRequest("����ִ� �׸��� �����մϴ�.");
            }

            if (!DateTime.TryParseExact(req.Birth, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime birthDT))
            {
                return BadRequest("��������� yyyyMMdd �������� �Է����ּ���.");
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
                        return BadRequest($"������� ���� : {duplicateField}");
                    }
                    _logger.LogError(sqlEx, "ȸ������ �� ���� �߻�");
                    return StatusCode(500, "���� ������ �߻��߽��ϴ�.");
                }
                _logger.LogError(duEx, "ȸ������ �� ���� �߻�");
                return StatusCode(500, "���� ������ �߻��߽��ϴ�.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ȸ������ �� ���� �߻�");
                return StatusCode(500, "���� ������ �߻��߽��ϴ�.");
            }

            return Ok();
        }

        /// <summary>
        /// ��й�ȣ ��ȣȭ
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
        /// ȸ�����Խ� �ߺ� ������ ����
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private string GetDuplicateField(string errorMessage)
        {
            if (errorMessage.Contains("for key 'member.Id'"))
            {
                return "���̵�";
            }
            if (errorMessage.Contains("for key 'member.Email'"))
            {
                return "�̸���";
            }
            if (errorMessage.Contains("for key 'member.Phone'"))
            {
                return "����ó";
            }
            return "�� �� ���� �ʵ�";
        }
    }
}
