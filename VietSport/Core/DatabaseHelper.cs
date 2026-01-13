using System;
using System.Data; // <-- QUAN TRỌNG: Phải có dòng này mới dùng được CommandType
using System.Data.SqlClient;

public static class DatabaseHelper
{
    // CẤU HÌNH KẾT NỐI
    public static string ServerName = @"localhost\MSSQL_NEW"; // Tên server của bạn
    public static string DbName = "VietSportDB";

    public static string ConnectionString => $"Data Source={ServerName};Initial Catalog={DbName};Integrated Security=True";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }

    // =============================================================
    // PHẦN XỬ LÝ DEMO XUNG ĐỘT (THÊM MỚI VÀO ĐÂY)
    // =============================================================

    /// <summary>
    /// Hàm dùng cho TRANSACTION 1 (Người đặt sân)
    /// </summary>
    /// <param name="maKH">Mã khách hàng đặt</param>
    /// <param name="maSan">Mã sân muốn đặt</param>
    /// <param name="batDau">Giờ bắt đầu</param>
    /// <param name="ketThuc">Giờ kết thúc</param>
    /// <param name="dungBanFix">False = Chạy bản Lỗi, True = Chạy bản đã Fix</param>
    public static string DatSan_Transaction_T1(string maKH, string maSan, DateTime batDau, DateTime ketThuc, bool dungBanFix)
    {
        // 1. Chọn tên SP dựa vào việc bạn muốn test Lỗi hay test Fix
        // Lưu ý: Tên SP phải trùng với tên bạn đã tạo trong SQL Server ở bước trước
        string tenProcedure = dungBanFix ? "sp_DatSan_Demo_Fix" : "sp_DatSan_Demo_Loi";
        
        // Tạo mã phiếu tự động (ví dụ: P + số giây hiện tại)
        string maPhieu = "P" + DateTime.Now.Ticks.ToString().Substring(12);

        using (SqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(tenProcedure, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // 2. Truyền tham số vào SP
                cmd.Parameters.AddWithValue("@MaKhachHang", maKH);
                cmd.Parameters.AddWithValue("@MaSan", maSan);
                cmd.Parameters.AddWithValue("@GioBatDau", batDau);
                cmd.Parameters.AddWithValue("@GioKetThuc", ketThuc);
                cmd.Parameters.AddWithValue("@MaPhieuDat", maPhieu);

                // 3. QUAN TRỌNG: Tăng thời gian chờ (Timeout)
                // Vì trong SQL mình có lệnh WAITFOR DELAY 10s, mặc định C# chờ 30s là ok,
                // nhưng set lên 60s cho chắc chắn không bị ngắt giữa chừng.
                cmd.CommandTimeout = 60; 

                // 4. Thực thi và đọc kết quả trả về từ SQL
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Lấy cột 'ThongBao' từ câu SELECT trong SP
                        return reader["ThongBao"].ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                // Mã lỗi 1205 là Deadlock (nếu xảy ra)
                if (ex.Number == 1205) 
                    return "Xung đột Deadlock! Hệ thống đã tự động hủy giao dịch này.";
                
                return "Lỗi SQL: " + ex.Message;
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống: " + ex.Message;
            }
        }
        return "Không có phản hồi từ Server";
    }

    /// <summary>
    /// Hàm dùng cho TRANSACTION 2 (Người quản lý set Bảo trì hoặc Người đặt thứ 2)
    /// </summary>
    public static string CapNhatSan_Transaction_T2(string maSan, string trangThaiMoi)
    {
        // Giả sử bạn dùng câu lệnh SQL trực tiếp hoặc gọi SP cập nhật
        using (SqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                
                // Ví dụ cập nhật trực tiếp cho nhanh (hoặc gọi SP sp_CapNhatTrangThaiSan)
                string query = "UPDATE SanTheThao SET TinhTrang = @TinhTrang WHERE MaSan = @MaSan";
                
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TinhTrang", trangThaiMoi); // Ví dụ: 'Bảo trì'
                cmd.Parameters.AddWithValue("@MaSan", maSan);

                int result = cmd.ExecuteNonQuery();
                
                if (result > 0) return "Cập nhật trạng thái thành công!";
                return "Không tìm thấy sân để cập nhật.";
            }
            catch (Exception ex)
            {
                return "Lỗi cập nhật: " + ex.Message;
            }
        }
    }
}