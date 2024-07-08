using Microsoft.AspNetCore.Mvc;
using MyPCSpec.Helpers;
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

        /// <summary>
        /// CPU 정보조회
        /// </summary>
        /// <returns></returns>
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
                return StatusCode(500, "CPU 검색중 에러발생 : " + ex.Message);
            }
        }

        /// <summary>
        /// 그래픽카드 정보조회
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetGraphicsCard()
        {
            try
            {
                List<string> GraphicsCardNames = new List<string>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    GraphicsCardNames.Add(obj["Name"].ToString());
                }

                return Ok(GraphicsCardNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "그래픽 카드 검색중 에러발생 : " + ex.Message);
            }
        }

        /// <summary>
        /// 모니터 정보조회
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetMonitor()
        {
            try
            {
                var monitorNames = DisplayInfoHelper.GetMonitorNames();
                var monitorResolutions = DisplayInfoHelper.GetMonitorResolutions();

                List<string> monitors = new List<string>();
                int count = Math.Max(monitorNames.Count, monitorResolutions.Count);

                for (int i = 0; i < count; i++)
                {
                    string name = i < monitorNames.Count ? monitorNames[i] : "Unknown";
                    string resolution = i < monitorResolutions.Count ? monitorResolutions[i] : "Unknown";
                    monitors.Add($"{name} ({resolution})");
                }

                return Ok(monitors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "모니터 검색 중 에러 발생: " + ex.Message);
            }
        }
    }
}
