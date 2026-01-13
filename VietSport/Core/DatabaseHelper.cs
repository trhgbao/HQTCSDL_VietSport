using System;
using System.Data; // <-- QUAN TRỌNG: Phải có dòng này mới dùng được CommandType
using System.Data.SqlClient;

namespace VietSportSystem
{
    public static class DatabaseHelper
    {
        // CẤU HÌNH KẾT NỐI
        public static string ServerName = @"DESKTOP-KLE6SON"; // Tên server của bạn
        public static string DbName = "VietSportDB";

        public static string ConnectionString => $"Data Source={ServerName};Initial Catalog={DbName};Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        // =============================================================
        // CÁC HÀM DEMO TRANSACTION CŨ (GIỮ LẠI NẾU BẠN ĐANG DÙNG)
        // =============================================================

        public static string DatSan_Transaction_T1(string maKH, string maSan, DateTime batDau, DateTime ketThuc, bool dungBanFix)
        {
            string tenProcedure = dungBanFix ? "sp_DatSan_Demo_Fix" : "sp_DatSan_Demo_Loi";
            string maPhieu = "P" + DateTime.Now.Ticks.ToString().Substring(12);

            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(tenProcedure, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaKhachHang", maKH);
                        cmd.Parameters.AddWithValue("@MaSan", maSan);
                        cmd.Parameters.AddWithValue("@GioBatDau", batDau);
                        cmd.Parameters.AddWithValue("@GioKetThuc", ketThuc);
                        cmd.Parameters.AddWithValue("@MaPhieuDat", maPhieu);
                        cmd.CommandTimeout = 60;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader["ThongBao"].ToString();
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
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

        public static string CapNhatSan_Transaction_T2(string maSan, string trangThaiMoi)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = "UPDATE SanTheThao SET TinhTrang = @TinhTrang WHERE MaSan = @MaSan";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TinhTrang", trangThaiMoi);
                        cmd.Parameters.AddWithValue("@MaSan", maSan);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0) return "Cập nhật trạng thái thành công!";
                        return "Không tìm thấy sân để cập nhật.";
                    }
                }
                catch (Exception ex)
                {
                    return "Lỗi cập nhật: " + ex.Message;
                }
            }
        }

        // =============================================================
        // CÁC HÀM GỌI PROCEDURE MỚI TRONG Procedure.sql
        // =============================================================

        /// <summary>
        /// Gọi sp_DatSan_KiemTraGioiHan: kiểm tra giới hạn & trùng giờ, sau đó tự INSERT PhieuDatSan.
        /// Giới hạn sân được tự động lấy từ bảng THAMSO (key MAX_BOOK).
        /// Trả về thông báo lỗi (nếu có), null/empty nếu thành công.
        /// </summary>
        public static string? DatSan_KiemTraGioiHan(string maKhachHang, string maSan,
                                                   DateTime gioBatDau, DateTime gioKetThuc,
                                                   string kenhDat)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_DatSan_KiemTraGioiHan", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaKhachHang", maKhachHang);
                        cmd.Parameters.AddWithValue("@MaSan", maSan);
                        cmd.Parameters.AddWithValue("@GioBatDau", gioBatDau);
                        cmd.Parameters.AddWithValue("@GioKetThuc", gioKetThuc);
                        cmd.Parameters.AddWithValue("@KenhDat", kenhDat);
                        // Không cần truyền @GioiHanSan nữa, SP sẽ tự lấy từ THAMSO

                        cmd.ExecuteNonQuery();
                    }
                    return null; // Thành công
                }
                catch (SqlException ex)
                {
                    // Các RAISERROR trong SP sẽ nhảy vào đây
                    return ex.Message;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        /// <summary>
        /// Gọi sp_DatSan_GayXungDot: bản demo dùng mức cô lập READ COMMITTED để dễ xảy ra xung đột.
        /// SP trả về message qua SELECT KetQua; hàm này trả lại string message (null nếu không có).
        /// </summary>
        public static string? DatSan_GayXungDot(string maKhachHang, string maSan,
                                               DateTime gioBatDau, DateTime gioKetThuc)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_DatSan_GayXungDot", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaKhachHang", maKhachHang);
                        cmd.Parameters.AddWithValue("@MaSan", maSan);
                        cmd.Parameters.AddWithValue("@GioBatDau", gioBatDau);
                        cmd.Parameters.AddWithValue("@GioKetThuc", gioKetThuc);

                        object? result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
                catch (SqlException ex)
                {
                    return ex.Message;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        /// <summary>
        /// Gọi sp_ThueDungCu để trừ kho dịch vụ an toàn (có khóa UPDLOCK).
        /// Trả về thông báo lỗi nếu có, null nếu thành công.
        /// </summary>
        public static string? ThueDungCu(string maDichVu, int soLuongThue)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_ThueDungCu", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaDichVu", maDichVu);
                        cmd.Parameters.AddWithValue("@SoLuongThue", soLuongThue);
                        cmd.ExecuteNonQuery();
                    }
                    return null;
                }
                catch (SqlException ex)
                {
                    return ex.Message;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        /// <summary>
        /// Gọi sp_NhapKho để cộng thêm tồn kho dịch vụ.
        /// </summary>
        public static string? NhapKho(string maDichVu, int soLuongNhap)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_NhapKho", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaDichVu", maDichVu);
                        cmd.Parameters.AddWithValue("@SoLuongNhap", soLuongNhap);
                        cmd.ExecuteNonQuery();
                    }
                    return null;
                }
                catch (SqlException ex)
                {
                    return ex.Message;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        /// <summary>
        /// Gọi sp_TimKiemSanTrong, trả về DataTable danh sách sân trống.
        /// </summary>
        public static DataTable TimKiemSanTrong(DateTime gioBatDau, DateTime gioKetThuc, string loaiSan)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_TimKiemSanTrong", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GioBatDau", gioBatDau);
                    cmd.Parameters.AddWithValue("@GioKetThuc", gioKetThuc);
                    cmd.Parameters.AddWithValue("@LoaiSan", loaiSan);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
    }
}