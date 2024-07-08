using Microsoft.AspNetCore.Mvc;
using System.Management;

namespace MyPCSpec.Controllers
{
    public class SpecController : Controller
	{
		private readonly ILogger<SpecController> _logger;

		public SpecController(ILogger<SpecController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public IActionResult GetCPU()
		{
			try
			{
				List<string> CpuName = new List<string>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    CpuName.Add(obj["Name"].ToString());
                }

                return Ok(CpuName);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "CPU �˻��� �����߻� : " + ex.Message);
			}
		}
	}
}
