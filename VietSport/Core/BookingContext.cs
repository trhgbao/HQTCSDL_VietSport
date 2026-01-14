using System;

namespace VietSportSystem
{
    // Class tĩnh dùng để truyền dữ liệu đặt phòng VIP sang màn hình Thanh toán
    public static class BookingContext
    {
        public static bool VipSelected { get; set; } = false;
        public static DateTime VipStart { get; set; }
        public static DateTime VipEnd { get; set; }
        public static bool VipUseFix { get; set; } = false;
        public static string VipMaCoSo { get; set; } = "";

        // Hàm reset dữ liệu sau khi thanh toán xong
        public static void ClearVip()
        {
            VipSelected = false;
            VipUseFix = false;
            VipMaCoSo = "";
            VipStart = DateTime.MinValue;
            VipEnd = DateTime.MinValue;
        }
    }
}