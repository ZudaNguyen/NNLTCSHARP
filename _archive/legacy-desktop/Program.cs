using QuanLyThuVien.Desktop.Data;
namespace QuanLyThuVien.Desktop
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            string result = DatabaseConnection.TestConnection();

            MessageBox.Show(
                result,
                "Kiểm tra kết nối SQL Server",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            Application.Run(new Forms.LoginForm());
        }
    }
}