namespace QL_KhachSan
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
            using var login = new QL_KhachSan.DangNhap.FormDangNhap();
            if (login.ShowDialog() != DialogResult.OK)
                return;

            using var main = new FormMain();
            Application.Run(main);
        }
    }
}
