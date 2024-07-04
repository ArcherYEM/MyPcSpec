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
        /// 로그인 시도
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
                        _logger.LogError("로그인 데이터 저장 실패");
                        return StatusCode(500, "로그인에 실패하였습니다.");
                    }

                    _logger.LogError("존재하지 않는 회원");
                    return StatusCode(500, "존재하지 않는 회원입니다.");
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
                                _logger.LogError("로그인 데이터 저장 실패");
                                return StatusCode(500, "로그인에 실패하였습니다.");
                            }

                            member.LastLoginAt = DateTime.Now;

                            _memberService.Update(member);
                            int updateInt = await _mpsContext.SaveChangesAsync();

                            if (updateInt == 1)
                            {
                                // update 커밋
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
                                // update 롤백
                                await transaction.RollbackAsync();

                                _logger.LogError("로그인 일자정보 저장 실패");
                                return StatusCode(500, "로그인에 실패하였습니다.");
                            }
                        }
                        else if (member.UseYn == 'N' && member.DelYn == 'N')
                        {
                            var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                            if (!loginHistoryBool)
                            {
                                _logger.LogError("로그인 데이터 저장 실패");
                                return StatusCode(500, "로그인에 실패하였습니다.");
                            }

                            _logger.LogError("휴면회원 로그인 시도");
                            return StatusCode(500, "휴면회원입니다.");
                        }
                        else if (member.UseYn == 'N' && member.DelYn == 'Y')
                        {
                            var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                            if (!loginHistoryBool)
                            {
                                _logger.LogError("로그인 데이터 저장 실패");
                                return StatusCode(500, "로그인에 실패하였습니다.");
                            }

                            _logger.LogError("탈퇴한 사용자 로그인 시도");
                            return StatusCode(500, "탈퇴한 사용자 입니다.");
                        }
                        else
                        {
                            var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                            if (!loginHistoryBool)
                            {
                                _logger.LogError("로그인 데이터 저장 실패");
                                return StatusCode(500, "로그인에 실패하였습니다.");
                            }

                            _logger.LogError("서버 오류 발생");
                            return StatusCode(500, "서버 오류 발생. 관리자에게 문의해주세요.");
                        }
                    }
                    else
                    {
                        var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                        if (!loginHistoryBool)
                        {
                            _logger.LogError("로그인 데이터 저장 실패");
                            return StatusCode(500, "로그인에 실패하였습니다.");
                        }

                        _logger.LogError("비밀번호 오류");
                        return StatusCode(500, "비밀번호를 확인해주세요.");
                    }
                }
            }
            catch (Exception ex)
            {
                var loginHistoryBool = await InsertLoginHistory(req.Id, 'Y');
                if (!loginHistoryBool)
                {
                    _logger.LogError("로그인 데이터 저장 실패");
                    return StatusCode(500, "로그인에 실패하였습니다.");
                }

                _logger.LogError(ex, "서버 오류 발생");
                return StatusCode(500, "서버 오류 발생. 관리자에게 문의해주세요.");
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
        /// 로그인 환경 IPv4 수집
        /// </summary>
        /// <returns></returns>
        public string GetLocalIPv4()
        {
            string localIPv4 = string.Empty;

            // 현재 컴퓨터의 모든 네트워크 인터페이스 가져오기
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // 네트워크 인터페이스가 활성화되어 있고 IPv4 주소를 가지고 있는지 확인
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    // IPv4 주소 가져오기
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIPv4 = ip.Address.ToString();
                            break;
                        }
                    }

                    // IPv4 주소를 찾았으면 루프 종료
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
        /// 회원가입 Insert
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Join([FromBody] RequestMemberDTO req)
        {
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
                    return BadRequest(data + " (은)는 이미 존재합니다.");
                }
            }
            else
            {
                return BadRequest("값이 비어있습니다.");
            }
        }
    }
}