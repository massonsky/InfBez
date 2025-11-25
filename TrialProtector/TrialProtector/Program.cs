using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Win32; // Для работы с реестром (отвлекающий маневр)

namespace LabProtection
{
    // ==========================================
    // C_01: Hardware ID & Crypto (Требования 4, 12)
    // ==========================================
    public static class C_01
    {
        // Получение "слепка" железа
        public static int M_GetHw()
        {
            string s = Environment.MachineName + Environment.ProcessorCount + Environment.UserName;
            return s.GetHashCode();
        }

        // Алгоритм 1 (для файла А)
        public static byte[] M_Enc_A(byte[] d)
        {
            int k = M_GetHw();
            byte[] r = new byte[d.Length];
            for (int i = 0; i < d.Length; i++) r[i] = (byte)(d[i] ^ k ^ (i + 11)); // +11
            return r;
        }

        // Алгоритм 2 (для файла Б - Требование 12: разные алгоритмы)
        public static byte[] M_Enc_B(byte[] d)
        {
            int k = ~M_GetHw(); // Инверсия ключа
            byte[] r = new byte[d.Length];
            for (int i = 0; i < d.Length; i++) r[i] = (byte)(d[i] ^ k ^ (i * 2)); // *2
            return r;
        }

        public static string M_Str(byte[] b) => Encoding.UTF8.GetString(b);
    }

    // ==========================================
    // C_02: Path Manager (Требования 1, 3, 5, 6)
    // ==========================================
    public class C_02
    {
        public string M_P1() // Путь 1
        {
            // %APPDATA%\Microsoft\Windows\sys_log.dat
            string r = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Сборка пути из байтов (Требование 6)
            string s = C_01.M_Str(new byte[] { 92, 77, 105, 99, 114, 111, 115, 111, 102, 116, 92, 87, 105, 110, 100, 111, 119, 115 });
            string f = C_01.M_Str(new byte[] { 92, 115, 121, 115, 95, 108, 111, 103, 46, 100, 97, 116 });
            return r + s + f;
        }

        public string M_P2() // Путь 2 (в другом месте)
        {
            // %LOCALAPPDATA%\Temp\cache_v2.bin
            string r = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string s = C_01.M_Str(new byte[] { 92, 84, 101, 109, 112 });
            string f = C_01.M_Str(new byte[] { 92, 99, 97, 99, 104, 101, 95, 118, 50, 46, 98, 105, 110 });
            return r + s + f;
        }
    }

    // ==========================================
    // C_03: Storage & Decoys (Требования 2, 11, 13)
    // ==========================================
    public class C_03
    {
        // Отвлекающий маневр в реестре (Требование 11, 13)
        public void M_DecoyReg()
        {
            try
            {
                // Пишем в реестр "License: OK", но на самом деле это ложь
                RegistryKey k = Registry.CurrentUser.CreateSubKey("Software\\MyAppConfig");
                k.SetValue("LicenseStatus", "Valid");
                k.SetValue("MaxRuns", 9999); // Пусть хакер радуется, это ничего не меняет
                k.Close();
            }
            catch { }
        }

        public void M_Save(string p, int v, bool isTypeA)
        {
            // Требование 2: Дата не меняется
            DateTime d1 = new DateTime(2021, 5, 10);
            if (File.Exists(p)) d1 = File.GetLastWriteTime(p);

            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(v));

            // Шифруем разными алгоритмами (Требование 12)
            byte[] enc = isTypeA ? C_01.M_Enc_A(b.ToArray()) : C_01.M_Enc_B(b.ToArray());

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(p));
                File.WriteAllBytes(p, enc);
                // Восстановление времени
                File.SetLastWriteTime(p, d1);
                File.SetCreationTime(p, d1);
            }
            catch { }
        }

        public int M_Load(string p, bool isTypeA)
        {
            if (!File.Exists(p)) return 0;
            try
            {
                byte[] raw = File.ReadAllBytes(p);
                byte[] dec = isTypeA ? C_01.M_Enc_A(raw) : C_01.M_Enc_B(raw);
                return BitConverter.ToInt32(dec, 0);
            }
            catch { return 999; } // Если ошибка дешифровки (чужой ПК) - блокируем
        }
    }

    // ==========================================
    // C_04: VM (Требования 8, 10)
    // ==========================================
    public class C_04
    {
        // Проверка без использования стандартных функций и явных bool переменных
        public int M_Exec(int curr, int max)
        {
            // Возвращает не true/false, а код состояния
            // 0x1A = OK, 0xFF = FAIL

            byte[] code = { 0x01, (byte)curr, 0x02, (byte)max, 0x03 }; // LOAD, CMP, EXIT
            int ip = 0;
            int reg = 0;

            while (ip < code.Length)
            {
                switch (code[ip++])
                {
                    case 0x01: reg = code[ip++]; break; // Load current
                    case 0x02: // Compare
                        int lim = code[ip++];
                        if (reg < lim) return 0x1A; // Код успеха
                        else return 0xFF;           // Код провала
                }
            }
            return 0xFF;
        }
    }

    // ==========================================
    // MAIN (Требования 7, 9, 14)
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            // Непонятные имена (Требование 14)
            C_02 pm = new C_02();
            C_03 st = new C_03();
            C_04 vm = new C_04();

            // 1. Отвлекающий маневр (создаем ключи в реестре)
            st.M_DecoyReg();

            // 2. Чтение (из двух мест)
            string p1 = pm.M_P1();
            string p2 = pm.M_P2();

            int v1 = st.M_Load(p1, true);
            int v2 = st.M_Load(p2, false);

            int v_real = (v1 > v2) ? v1 : v2;

            // Требование 7: Не анализировать сразу. Делаем паузу или действие.
            Thread.Sleep(50);

            int v_next = v_real + 1;

            // 3. Запись (с маскировкой времени)
            st.M_Save(p1, v_next, true);
            st.M_Save(p2, v_next, false);

            // Требование 9: Отложенная реакция. Сначала сохранили, потом думаем.

            // 4. Проверка через VM (Требование 8)
            // Лимит запусков = 4
            int status = vm.M_Exec(v_real, 4);

            // Требование 10: Результат проверки в status (int), а не в явном bool
            // Требование 14: GOTO
            if (status == 0x1A) goto _L_OK;
            goto _L_FAIL;

        _L_FAIL:
            // Требование 11: Отвлекающие действия
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Memory write at 0x0045F1 failed.");
            Console.WriteLine("Trial period expired.");
            Console.ReadKey();
            return;

        _L_OK:
            M_Payload();
        }

        static void M_Payload()
        {
            Console.Clear();
            Console.WriteLine("=== PROGRAM ACTIVE ===");
            Console.WriteLine("Enter file path to touch:");
            string p = Console.ReadLine();
            if (File.Exists(p))
            {
                File.SetLastWriteTime(p, DateTime.Now);
                Console.WriteLine("Done.");
            }
            Console.ReadKey();
        }
    }
}