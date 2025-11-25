using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using HwBindLib;

namespace HwBindingProtectedProgram
{
    // Класс с "мусорным" именем (Требование 11)
    class C_Zyx99
    {
        // Метод дешифровки А
        public static uint M_Dec_A(byte[] d)
        {
            if (d == null || d.Length < 4) return 0;
            byte[] c = new byte[4];
            for (int k = 0; k < 4; k++) c[k] = (byte)(d[k] ^ 0xAA ^ (k + 5));
            return BitConverter.ToUInt32(c, 0);
        }

        // Метод дешифровки Б
        public static uint M_Dec_B(byte[] d)
        {
            if (d == null || d.Length < 4) return 0;
            byte[] c = new byte[4];
            for (int k = 0; k < 4; k++) c[k] = (byte)(d[k] ^ 0x55 ^ (k * 3));
            return BitConverter.ToUInt32(c, 0);
        }
    }

    class Program
    {
        // Требование 11: Переменные с мусорными именами
        static void Main(string[] args)
        {
            // --- 1. Отвлекающие действия (Требование 6, 10) ---
            try
            {
                RegistryKey rk = Registry.CurrentUser.CreateSubKey("Software\\SystemDrives");
                rk.SetValue("LastScan", DateTime.Now.ToString()); // Пишем мусор в реестр
            }
            catch { }

            // --- 2. Сборка путей (Требование 4) ---
            string v_path1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                        Encoding.UTF8.GetString(new byte[] { 92, 77, 105, 99, 114, 111, 115, 111, 102, 116, 92, 87, 105, 110, 100, 111, 119, 115, 92, 115, 121, 115, 95, 99, 111, 110, 102, 105, 103, 46, 100, 97, 116 });

            string v_path2 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                        Encoding.UTF8.GetString(new byte[] { 92, 84, 101, 109, 112, 92, 100, 114, 105, 118, 101, 114, 95, 99, 97, 99, 104, 101, 46, 98, 105, 110 });

            // --- 3. Чтение и дешифровка (Требование 1, 2) ---
            uint v_key1 = 0;
            uint v_key2 = 0;

            if (File.Exists(v_path1)) v_key1 = C_Zyx99.M_Dec_A(File.ReadAllBytes(v_path1));
            if (File.Exists(v_path2)) v_key2 = C_Zyx99.M_Dec_B(File.ReadAllBytes(v_path2));

            // Считаем, что ключ верен, если совпадает хотя бы один из файлов (или оба)
            // Но в целях защиты лучше требовать совпадение. Допустим, мы берем Key1 как основной.
            uint v_target = (v_key1 != 0) ? v_key1 : v_key2;

            // Требование 3: Не анализировать сразу после чтения
            Thread.Sleep(100);

            // --- 4. Сканирование текущих дисков ---
            // Программа должна работать, если ПОДКЛЮЧЕН диск с нужным SN
            var v_drives = DriveInfo.GetDrives();

            // Используем неявный флаг (Требование 7, 8: Не хранить результат в bool переменной)
            int v_state = 0xBAD;

            foreach (var d in v_drives)
            {
                if (d.IsReady)
                {
                    uint v_curr = HardwareBinding.M_GetVolSN(d.Name);

                    // Эмуляция проверки без явного IF (Требование 6: не использовать функцию проверки)
                    // XOR разница. Если 0, значит равны.
                    long diff = v_curr ^ v_target;

                    if (diff == 0)
                    {
                        v_state = 0x900D; // GOOD
                        break;
                    }
                }
            }

            // Требование 5: Не выполнять действия сразу после проверки
            // Выполняем "отвлекающие вычисления"
            double x = Math.Sqrt(12345);

            // Требование 11: GOTO
            if (v_state == 0x900D) goto _L_Valid;
            else goto _L_Invalid;

            _L_Invalid:
            // Отвлекающий вывод
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Driver initialization failed (Code 0x80041).");
            Console.WriteLine("Insert the key medium and restart.");
            Console.ReadKey();
            return;

        _L_Valid:
            M_Payload();
        }

        static void M_Payload()
        {
            Console.Clear();
            Console.WriteLine("=== ACCESS GRANTED ===");
            Console.WriteLine("Welcome to the secure program.");
            Console.WriteLine("Hardware binding verified.");
            // Полезная нагрузка программы (из ЛР1)
            Console.WriteLine("Press Enter to execute logic...");
            Console.ReadLine();
        }
    }
}