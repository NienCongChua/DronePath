﻿using System.Runtime.InteropServices;

namespace DroneNien
{
    internal class UnrealAutomation
    {
        // Tìm cửa sổ theo tên
        [DllImport("User32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Đưa cửa sổ lên foreground
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // Gửi phím tắt
        [DllImport("User32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        // Mã phím
        private const byte VK_MENU = 0x12; // Alt
        private const byte VK_P = 0x50;   // P
        private const uint KEYEVENTF_KEYUP = 0x0002; // Thả phím
        //nien
        public static void TriggerPlayShortcut()
        {
            // Tên cửa sổ Unreal Editor (cần chính xác)
            string windowName = "CityParkEnvironment - Unreal Editor";

            // Lặp kiểm tra cửa sổ xuất hiện
            IntPtr hWnd = IntPtr.Zero;
            int retries = 60; // Tối đa 60 giây
            while (hWnd == IntPtr.Zero && retries > 0)
            {
                hWnd = FindWindow(null, windowName);
                Thread.Sleep(1000); // Chờ 1 giây trước mỗi lần kiểm tra
                retries--;
            }

            if (hWnd == IntPtr.Zero)
            {
                Console.WriteLine("Unreal Editor window not found.");
                return;
            }

            // Đưa cửa sổ Unreal Editor lên foreground
            SetForegroundWindow(hWnd);

            // Gửi phím tắt Alt + P
            keybd_event(VK_MENU, 0, 0, UIntPtr.Zero); // Nhấn Alt
            keybd_event(VK_P, 0, 0, UIntPtr.Zero);    // Nhấn P
            Thread.Sleep(100);                        // Giữ phím trong 100ms
            keybd_event(VK_P, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Thả P
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Thả Alt

            Console.WriteLine("Triggered Play using Alt + P.");
        }
    }
}
