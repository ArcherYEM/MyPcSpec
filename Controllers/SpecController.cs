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
        /// CPU ������ȸ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetCPU()
        {
            _logger.LogInformation("GetCPU endpoint called");

            try
            {
                List<string> CpuName = new List<string>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    CpuName.Add(obj["Name"].ToString());
                }

                _logger.LogInformation("CPU Names: {CpuName}", string.Join(", ", CpuName));
                return Ok(CpuName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CPU info");
                return StatusCode(500, "CPU �˻��� �����߻� : " + ex.Message);
            }
        }

        /// <summary>
        /// �׷���ī�� ������ȸ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetGraphicsCard()
        {
            _logger.LogInformation("GetGraphicsCard endpoint called");

            try
            {
                List<string> GraphicsCardNames = new List<string>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    GraphicsCardNames.Add(obj["Name"].ToString());
                }

                _logger.LogInformation("Graphics Card Names: {GraphicsCardNames}", string.Join(", ", GraphicsCardNames));
                return Ok(GraphicsCardNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching graphics card info");
                return StatusCode(500, "�׷��� ī�� �˻��� �����߻� : " + ex.Message);
            }
        }

        /// <summary>
        /// ����� ������ȸ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetMonitor()
        {
            _logger.LogInformation("GetMonitor endpoint called");

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

                _logger.LogInformation("Monitors: {Monitors}", string.Join(", ", monitors));
                return Ok(monitors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monitor info");
                return StatusCode(500, "����� �˻� �� ���� �߻�: " + ex.Message);
            }
        }
    }
}
