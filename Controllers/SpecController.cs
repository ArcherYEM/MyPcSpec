using Microsoft.AspNetCore.Mvc;
using System.Management;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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

        /// <summary>
        /// �׷���ī�� ������ȸ
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
            try
            {
                List<string> monitors = GetMonitors();

                return Ok(monitors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "����� �˻� �� ���� �߻�: " + ex.Message);
            }
        }

        private List<string> GetMonitors()
        {
            List<string> monitors = new List<string>();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE 'DISPLAY%'");
                foreach (ManagementObject monitor in searcher.Get())
                {
                    string name = monitor["Name"]?.ToString();
                    string deviceId = monitor["DeviceID"]?.ToString();

                    string screenWidth = "Unknown";
                    string screenHeight = "Unknown";

                    ManagementObjectSearcher configSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration");
                    foreach (ManagementObject config in configSearcher.Get())
                    {
                        if (config["DeviceName"] != null && config["DeviceName"].ToString().Contains(deviceId))
                        {
                            screenWidth = config["PelsWidth"]?.ToString();
                            screenHeight = config["PelsHeight"]?.ToString();
                        }
                    }

                    monitors.Add($"Monitor: {name}, Resolution: {screenWidth}x{screenHeight}");
                }
            }
            catch (Exception ex)
            {
                monitors.Add("����� ������ �������� �� ������ �߻��߽��ϴ�: " + ex.Message);
            }

            return monitors;
        }
    }
}
