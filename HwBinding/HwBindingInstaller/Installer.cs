using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using HwBindLib;

namespace HwBindingInstaller
{
    class Installer
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== INSTALLER WIZARD ===");
            Console.WriteLine("Scanning drives...");

            var drives = DriveInfo.GetDrives();
            List<string> options = new List<string>();

            int i = 0;
            foreach (var d in drives)
            {
                if (d.IsReady)
                {
                    uint sn = HardwareBinding.M_GetVolSN(d.Name);
                    // Показываем пользователю диск и его серийник (в HEX)
                    Console.WriteLine($"{i}. {d.Name} [{d.DriveType}] - SN: {sn:X}");
                    options.Add(d.Name);
                    i++;
                }
            }

            Console.Write("\nSelect drive to bind license to (0-{0}): ", i - 1);
            int idx = int.Parse(Console.ReadLine());
            string selectedDrive = options[idx];
            uint targetSN = HardwareBinding.M_GetVolSN(selectedDrive);

            Console.WriteLine($"Binding to {selectedDrive} (SN: {targetSN:X})...");

            // --- ШАГ 1: Генерация путей (Требование 4: Зашифрованные пути) ---
            // Путь 1: %APPDATA%\Microsoft\Windows\sys_config.dat
            string p1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                        Encoding.UTF8.GetString(new byte[] { 92, 77, 105, 99, 114, 111, 115, 111, 102, 116, 92, 87, 105, 110, 100, 111, 119, 115, 92, 115, 121, 115, 95, 99, 111, 110, 102, 105, 103, 46, 100, 97, 116 });

            // Путь 2: %LOCALAPPDATA%\Temp\driver_cache.bin (Требование 1: Несколько файлов)
            string p2 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                        Encoding.UTF8.GetString(new byte[] { 92, 84, 101, 109, 112, 92, 100, 114, 105, 118, 101, 114, 95, 99, 97, 99, 104, 101, 46, 98, 105, 110 });

            // --- ШАГ 2: Шифрование (Требование 2, 9: Разные алгоритмы) ---

            // Алгоритм А
            byte[] bytesA = BitConverter.GetBytes(targetSN);
            for (int k = 0; k < bytesA.Length; k++) bytesA[k] = (byte)(bytesA[k] ^ 0xAA ^ (k + 5));

            // Алгоритм Б
            byte[] bytesB = BitConverter.GetBytes(targetSN);
            for (int k = 0; k < bytesB.Length; k++) bytesB[k] = (byte)(bytesB[k] ^ 0x55 ^ (k * 3));

            // --- ШАГ 3: Запись ---
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(p1));
                Directory.CreateDirectory(Path.GetDirectoryName(p2));

                File.WriteAllBytes(p1, bytesA);
                File.WriteAllBytes(p2, bytesB);

                Console.WriteLine("Installation & Binding Successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing files: " + ex.Message);
            }

            Console.ReadKey();
        }
    }
}