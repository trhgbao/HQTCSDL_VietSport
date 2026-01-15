using System;
using System.Data;
using System.Data.SqlClient;

namespace VietSportSystem
{
    public static class DatabaseHelper
    {
        // =============================================================
        // 1. CẤU HÌNH KẾT NỐI (CHỌN 1 TRONG 2)
        // =============================================================

        // Option A: Cấu hình của Nam
        public static string ServerName = @"localhost\MSSQL_NEW";

        // Option B: Cấu hình của Trí (Bỏ comment dòng dưới nếu dùng máy Trí)
        // public static string ServerName = @".\MSSQLSERVER01"; 

        public static string DbName = "VietSportDB";

        public static string ConnectionString => $"Data Source={ServerName};Initial Catalog={DbName};Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        // =============================================================
        // 2. CÁC HÀM DEMO CŨ (SHARED)
        // =============================================================

        public static string DatSan_Transaction_T1(string maKH, string maSan, DateTime batDau, DateTime ketThuc, bool isFix)
        {
            // Logic chọn Procedure dựa trên checkbox (isFix)
            string tenProcedure = isFix ? "sp_DatSan_Demo_Fix" : "sp_DatSan_Demo_Loi";
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
                        return result > 0 ? "Cập nhật trạng thái thành công!" : "Không tìm thấy sân để cập nhật.";
                    }
                }
                catch (Exception ex) { return "Lỗi cập nhật: " + ex.Message; }
            }
        }

        // =============================================================
        // 3. CÁC HÀM CỦA NAM (INVENTORY & BOOKING LIMITS)
        // =============================================================

        public static string? DatSan_KiemTraGioiHan(string maKhachHang, string maSan, DateTime gioBatDau, DateTime gioKetThuc, string kenhDat)
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
                        cmd.ExecuteNonQuery();
                    }
                    return null; // Thành công
                }
                catch (Exception ex) { return ex.Message; }
            }
        }

        public static string? DatSan_GayXungDot(string maKhachHang, string maSan, DateTime gioBatDau, DateTime gioKetThuc)
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
                catch (Exception ex) { return ex.Message; }
            }
        }

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
                catch (Exception ex) { return ex.Message; }
            }
        }

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
                catch (Exception ex) { return ex.Message; }
            }
        }

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

        // =============================================================
        // 4. CÁC HÀM CỦA TRÍ (SCENARIOS 9, 10, 15)
        // =============================================================

        // --- SCENARIO 9: Update Price & Payment ---
        public static string Sp_CapNhatChinhSachGia(string hangTV, decimal mucGiamMoi)
        {
            return ExecuteSP_WithOutput("sp_CapNhatChinhSachGia",
                new SqlParameter("@HangTV", hangTV),
                new SqlParameter("@MucGiamMoi", mucGiamMoi));
        }

        public static string Sp_ThanhToanDonHang(string maKH, string maPhieu, bool isFix)
        {
            string spName = isFix ? "sp_ThanhToanDonHang_DaFix" : "sp_ThanhToanDonHang_CoLoi";
            return ExecuteSP_WithOutput(spName,
                new SqlParameter("@MaKH", maKH),
                new SqlParameter("@MaPhieuDat", maPhieu));
        }

        // =============================================================
        // 5. CÁC HÀM DEMO LOST UPDATE (TÌNH HUỐNG 6 & 13 - KHO DỊCH VỤ)
        // =============================================================

        /// <summary>
        /// Demo gây xung đột Lost Update (Tình huống 6): Không dùng UPDLOCK, đọc xong nhả khóa ngay.
        /// SP trả về SELECT KetQua. Trả về string message (null nếu không có).
        /// </summary>
        public static string? ThueDungCu_GayXungDot(string maDichVu, int soLuongThue)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_ThueDungCu_GayXungDot", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaDichVu", maDichVu);
                        cmd.Parameters.AddWithValue("@SoLuongThue", soLuongThue);
                        cmd.CommandTimeout = 30; // Tăng timeout để đợi WAITFOR DELAY

                        object? result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205) return "Deadlock: Giao dịch bị hủy do xung đột khóa (Lost Update).";
                    return ex.Message;
                }
                catch (Exception ex) { return ex.Message; }
            }
        }

        /// <summary>
        /// Demo gây xung đột Lost Update khi nhập kho (Tình huống 13).
        /// </summary>
        public static string? NhapKho_GayXungDot(string maDichVu, int soLuongNhap)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_NhapKho_GayXungDot", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaDichVu", maDichVu);
                        cmd.Parameters.AddWithValue("@SoLuongNhap", soLuongNhap);
                        cmd.CommandTimeout = 30;

                        object? result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205) return "Deadlock: Giao dịch bị hủy do xung đột khóa (Lost Update).";
                    return ex.Message;
                }
                catch (Exception ex) { return ex.Message; }
            }
        }

        // --- SCENARIO 10: Maintenance & View ---
        public static string Sp_CapNhatBaoTriSan(string maSan)
        {
            return ExecuteSP_WithOutput("sp_CapNhatBaoTriSan", new SqlParameter("@MaSan", maSan));
        }

        public static string Sp_XemThongTinSan(string maCoSo, bool isFix)
        {
            string spName = isFix ? "sp_XemThongTinSan_DaFix" : "sp_XemThongTinSan_CoLoi";
            return ExecuteSP_WithOutput(spName, new SqlParameter("@MaCoSo", maCoSo));
        }

        // --- SCENARIO 15: Change Time & Booking ---
        public static string Sp_DoiGioSan(string maPhieuCu, string maSanMoi, DateTime batDauMoi, DateTime ketThucMoi)
        {
            return ExecuteSP_WithOutput("sp_DoiGioSan",
                new SqlParameter("@MaPhieuDat_Cu", maPhieuCu),
                new SqlParameter("@MaSan_Moi", maSanMoi),
                new SqlParameter("@GioBatDau_Moi", batDauMoi),
                new SqlParameter("@GioKetThuc_Moi", ketThucMoi));
        }

        public static string Sp_TimVaDatSanTrong(string maSan, DateTime batDau, DateTime ketThuc, string maKH, bool isFix)
        {
            string spName = isFix ? "sp_TimVaDatSanTrong_DaFix" : "sp_TimVaDatSanTrong_CoLoi";
            return ExecuteSP_WithOutput(spName,
                new SqlParameter("@MaSan", maSan),
                new SqlParameter("@GioBatDau", batDau),
                new SqlParameter("@GioKetThuc", ketThuc),
                new SqlParameter("@MaKhachHang", maKH));
        }

        // --- Helper cho các Procedure có Output Parameter @KetQua ---
        private static string ExecuteSP_WithOutput(string spName, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(spName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters);

                        // Thêm tham số Output chuẩn để lấy thông báo từ SQL
                        SqlParameter outParam = new SqlParameter("@KetQua", SqlDbType.NVarChar, 500);
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();

                        return outParam.Value != DBNull.Value ? outParam.Value.ToString() : "Thao tác thành công (Không có thông báo trả về)";
                    }
                }
                catch (SqlException ex) { return "Lỗi SQL: " + ex.Message; }
                catch (Exception ex) { return "Lỗi hệ thống: " + ex.Message; }
            }
        }
    }
}