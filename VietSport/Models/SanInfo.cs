using System;

namespace VietSportSystem
{
    // Class đại diện cho thông tin một sân
    public class SanInfo
    {
        public string TenSan { get; set; }      // Tên sân
        public string LoaiSan { get; set; }     // Loại sân
        public string KhungGio { get; set; }    // Khung giờ
        public decimal GiaTien { get; set; }    // Giá tiền
        public string TrangThai { get; set; }   // Trạng thái (Còn trống/Đã đặt)
    }
}