using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MyPCSpec.Helpers
{
    public class DisplayInfoHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags]
        public enum DisplayDeviceStateFlags
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VGACompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmNup;
            public int dmDisplayFrequency;
        }

        public static List<string> GetMonitorNames()
        {
            List<string> monitorNames = new List<string>();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorID");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string name = GetStringFromUInt16Array((ushort[])queryObj["UserFriendlyName"]);
                    monitorNames.Add(name);
                }
            }
            catch (Exception ex)
            {
                monitorNames.Add("모니터 이름을 가져오는 중 오류가 발생했습니다: " + ex.Message);
            }

            return monitorNames;
        }

        public static List<string> GetMonitorResolutions()
        {
            List<string> monitorResolutions = new List<string>();
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            int devNum = 0;

            while (EnumDisplayDevices(null, (uint)devNum, ref d, 0))
            {
                if ((d.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) != 0)
                {
                    DEVMODE dm = new DEVMODE();
                    if (EnumDisplaySettings(d.DeviceName, -1, ref dm))
                    {
                        monitorResolutions.Add($"{dm.dmPelsWidth}x{dm.dmPelsHeight}");
                    }
                    else
                    {
                        monitorResolutions.Add("Unknown");
                    }
                }
                devNum++;
            }

            return monitorResolutions;
        }

        private static string GetStringFromUInt16Array(ushort[] values)
        {
            List<char> chars = new List<char>();
            foreach (ushort value in values)
            {
                if (value == 0) break; // Null terminator
                chars.Add((char)value);
            }
            return new string(chars.ToArray());
        }
    }
}
