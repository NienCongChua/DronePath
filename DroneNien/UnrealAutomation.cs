using System.Runtime.InteropServices;

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

        public static void TriggerPlayShortcut()
        {
            // Tên cửa sổ Unreal Editor (cần chính xác)
            string windowName = "Blocks";

            // Tìm cửa sổ Unreal Editor
            IntPtr hWnd = FindWindow(null, windowName);
            if (hWnd == IntPtr.Zero)
            {
                Console.WriteLine("Unreal Editor window not found.");
                return;
            }

            // Đợi cửa sổ Unreal Editor sẵn sàng (độ trễ: 20 giây)
            Console.WriteLine("Waiting for Unreal Editor to be ready...");
            Thread.Sleep(30000); // Chờ 20 giây

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
