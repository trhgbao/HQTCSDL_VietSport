namespace VietSportSystem
{
    public static class SessionData
    {
        public static string CurrentUsername { get; set; } = null; // Tên đăng nhập
        public static string CurrentUserFullName { get; set; } = null; // Họ tên hiển thị
        public static string CurrentUserID { get; set; } = null; // Mã khách hàng hoặc Mã nhân viên
        public static string UserRole { get; set; } = null; // Vai trò (KhachHang, Admin...)

        // Hàm kiểm tra đã đăng nhập chưa
        public static bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(CurrentUsername);
        }

        // Hàm đăng xuất
        public static void Logout()
        {
            CurrentUsername = null;
            CurrentUserFullName = null;
            CurrentUserID = null;
            UserRole = null;
        }
    }
}