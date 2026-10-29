using QuanLyThuVien.Desktop.Forms;

namespace QuanLyThuVien.Desktop;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        // Phải chỉ rõ namespace: `Application` trần sẽ trỏ vào project QuanLyThuVien.Application.
        System.Windows.Forms.Application.Run(new LoginForm());
    }
}
