using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyPCSpec.Models;
using MyPCSpec.Models.DAO;
using MyPCSpec.Models.DTO;
using MyPCSpec.Models.VM;
using MyPCSpec.Services;
using MyPCSpec.Services.Interfaces;
using MySqlConnector;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MyPCSpec.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IMemberService _memberService;
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly MpsContext _mpsContext;

        public AccountController(
            ILogger<AccountController> logger,
            IMemberService memberService,
            ILoginHistoryService loginHistoryService,
            MpsContext mpsContext
        )
        {
            _logger = logger;
            _memberService = memberService;
            _loginHistoryService = loginHistoryService;
            _mpsContext = mpsContext;
        }

        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// �α��� �õ�
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] Member req)
        {
            using var transaction = await _mpsContext.Database.BeginTransactionAsync();
            try
            {
                var member = await _memberService.Get(req.Id);
                if (member == null)
                {
                    var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                    if (!loginHistoryBool)
                    {
                        _logger.LogError("�α��� ������ ���� ����");
                        return StatusCode(500, "�α��ο� �����Ͽ����ϴ�.");
                    }

                    _logger.LogError("�������� �ʴ� ȸ��");
                    return StatusCode(500, "�������� �ʴ� ȸ���Դϴ�.");
                }
                else
                {
                    if (member.Pw == SecurityPw(req.Pw))
                    {
                        if (member.UseYn == 'Y' && member.DelYn == 'N')
                        {
                            var loginHistoryBool = await InsertLoginHistory(req.Id, 'N');
                            if (!loginHistoryBool)
                            {
                                _logger.LogError("�α��� ������ ���� ����");
                                return StatusCode(500, "�α��ο� �����Ͽ����ϴ�.");
                            }

                            member.LastLoginAt = DateTime.Now;

                            _memberService.Update(member);
                            int updateInt = await _mpsContext.SaveChangesAsync();

                            if (updateInt == 1)
                            {
                                // update Ŀ��
                                await transaction.CommitAsync();

                                var vm = new MemberViewModel
                                {
                                    Id = member.Id,
                                    Name = member.Name,
                                    Birth = member.Birth,
                                    Email = member.Email,
                                    Phone = member.Phone,
                                    Level = member.Level,
                                    LastLoginAt = member.LastLoginAt
                                };

                                return RedirectToAction("Index", "Home", vm);
                            }
                            else
                            {
                                // update �ѹ�
                                await transaction.RollbackAsync();

                                _logger.LogError("�α��� �������� ���� ����");
                                return StatusCode(500, "�α��ο� �����Ͽ����ϴ�.");
                            }
                        }
                        else if (member.UseYn == 'N' && member.DelYn == 'N')
                        {
                            var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                            if (!loginHistoryBool)
                            {
                                _logger.LogError("�α��� ������ ���� ����");
                                return StatusCode(500, "�α��ο� �����Ͽ����ϴ�.");
                            }

                            _logger.LogError("�޸�ȸ�� �α��� �õ�");
                            return StatusCode(500, "�޸�ȸ���Դϴ�.");
                        }
                        else if (member.UseYn == 'N' && member.DelYn == 'Y')
                        {
                            var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                            if (!loginHistoryBool)
                            {
                                _logger.LogError("�α��� ������ ���� ����");
                                return StatusCode(500, "�α��ο� �����Ͽ����ϴ�.");
                            }

                            _logger.LogError("Ż���� ����� �α��� �õ�");
                            return StatusCode(500, "Ż���� ����� �Դϴ�.");
                        }
                        else
                        {
                            var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                            if (!loginHistoryBool)
                            {
                                _logger.LogError("�α��� ������ ���� ����");
                                return StatusCode(500, "�α��ο� �����Ͽ����ϴ�.");
                            }

                            _logger.LogError("���� ���� �߻�");
                            return StatusCode(500, "���� ���� �߻�. �����ڿ��� �������ּ���.");
                        }
                    }
                    else
                    {
                        var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                        if (!loginHistoryBool)
                        {
                            _logger.LogError("�α��� ������ ���� ����");
                            return StatusCode(500, "�α��ο� �����Ͽ����ϴ�.");
                        }

                        _logger.LogError("��й�ȣ ����");
                        return StatusCode(500, "��й�ȣ�� Ȯ�����ּ���.");
                    }
                }
            }
            catch (Exception ex)
            {
                var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                if (!loginHistoryBool)
                {
                    _logger.LogError("�α��� ������ ���� ����");
                    return StatusCode(500, "�α��ο� �����Ͽ����ϴ�.");
                }

                _logger.LogError(ex, "���� ���� �߻�");
                return StatusCode(500, "���� ���� �߻�. �����ڿ��� �������ּ���.");
            }
        }

        private async Task<bool> InsertLoginHistory(string id, char failYn)
        {
            var loginData = new LoginHistory
            {
                Id = id,
                FailYn = failYn,
                Ip = GetLocalIPv4(),
                CreatedAt = DateTime.Now
            };

            await _loginHistoryService.InsertAsync(loginData);
            await _mpsContext.SaveChangesAsync();

            if (loginData.Ip.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// �α��� ȯ�� IPv4 ����
        /// </summary>
        /// <returns></returns>
        public string GetLocalIPv4()
        {
            string localIPv4 = string.Empty;

            // ���� ��ǻ���� ��� ��Ʈ��ũ �������̽� ��������
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // ��Ʈ��ũ �������̽��� Ȱ��ȭ�Ǿ� �ְ� IPv4 �ּҸ� ������ �ִ��� Ȯ��
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    // IPv4 �ּ� ��������
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIPv4 = ip.Address.ToString();
                            break;
                        }
                    }

                    // IPv4 �ּҸ� ã������ ���� ����
                    if (!string.IsNullOrEmpty(localIPv4))
                    {
                        break;
                    }
                }
            }

            return localIPv4;
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
        public async Task<IActionResult> Join([FromBody] RequestMemberDTO req)
        {
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
                Level = req.Level,
                UseYn = 'Y',
                DelYn = 'N',
                CreatedAt = DateTime.Now,
                LastLoginAt = null
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

        [HttpPost]
        public async Task<IActionResult> DuplicateCheck([FromBody] Dictionary<string, string> req)
        {
            string column = req["field"];
            string data = req["value"].Trim();

            if (!column.IsNullOrEmpty() && !data.IsNullOrEmpty())
            {
                bool exists = await _memberService.Find(column, data);
                if (!exists)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(data + " (��)�� �̹� �����մϴ�.");
                }
            }
            else
            {
                return BadRequest("���� ����ֽ��ϴ�.");
            }
        }
    }
}