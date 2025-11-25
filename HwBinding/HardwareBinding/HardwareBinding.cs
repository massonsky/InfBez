using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HwBindLib
{
    // Класс с нечитаемым именем для обфускации
    public static class HardwareBinding
    {
        // Импорт WinAPI функции GetVolumeInformation
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetVolumeInformation(
            string lpRootPathName,
            StringBuilder lpVolumeNameBuffer,
            int nVolumeNameSize,
            out uint lpVolumeSerialNumber,
            out uint lpMaximumComponentLength,
            out uint lpFileSystemFlags,
            StringBuilder lpFileSystemNameBuffer,
            int nFileSystemNameSize);

        // Метод получения серийного номера диска (например, "C:\")
        public static uint M_GetVolSN(string rootPath)
        {
            uint serialNum = 0;
            uint maxCompLen = 0;
            uint fsFlags = 0;
            StringBuilder volName = new StringBuilder(256);
            StringBuilder fsName = new StringBuilder(256);

            if (!rootPath.EndsWith("\\")) rootPath += "\\";

            bool success = GetVolumeInformation(
                rootPath, volName, 256, out serialNum,
                out maxCompLen, out fsFlags, fsName, 256);

            return success ? serialNum : 0;
        }
    }
}