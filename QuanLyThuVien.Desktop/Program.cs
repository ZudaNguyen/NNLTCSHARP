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
            // Phải gọi trước khi tạo bất kỳ control nào
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ShowFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ShowFatal(e.ExceptionObject as Exception);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Forms.LoginForm());
        }

        private static void ShowFatal(Exception? ex)
        {
            MessageBox.Show(
                "Đã xảy ra lỗi không mong muốn. Thao tác vừa rồi có thể chưa được ghi nhận.\n\nChi tiết: " + (ex?.Message ?? "không rõ"),
                "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
