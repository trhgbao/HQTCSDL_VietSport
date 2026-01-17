using System;
using System.Data; // <-- QUAN TRỌNG: Phải có dòng này mới dùng được CommandType
using System.Data.SqlClient;

namespace VietSportSystem
{
    public static class DatabaseHelper
    {
        // CẤU HÌNH KẾT NỐI
        public static string ServerName = @"TOXICTILLTHEEND\MSSQLSERVER01"; 
        // public static string ServerName = @".\MSSQLSERVER01";
        // public static string ServerName = @"localhost\MSSQL_NEW"; 

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
        // =============================================================
        // CÁC HÀM GỌI PROCEDURE NEW (SCENARIOS 9, 10, 15)
        // =============================================================

        // --- SCENARIO 9: Update Price & Payment ---
        public static string Sp_CapNhatChinhSachGia(string hangTV, decimal mucGiamMoi)
        {
            return ExecuteScalarSP("sp_CapNhatChinhSachGia", new SqlParameter("@HangTV", hangTV),
                                                             new SqlParameter("@MucGiamMoi", mucGiamMoi));
        }

        public static string Sp_ThanhToanDonHang(string maKH, string maPhieu, bool isFix)
        {
            string spName = isFix ? "sp_ThanhToanDonHang_DaFix" : "sp_ThanhToanDonHang_CoLoi";
            return ExecuteScalarSP(spName, new SqlParameter("@MaKH", maKH),
                                           new SqlParameter("@MaPhieuDat", maPhieu));
        }

        // --- SCENARIO 10: Maintenance & View ---
        public static string Sp_CapNhatBaoTriSan(string maSan)
        {
            return ExecuteScalarSP("sp_CapNhatBaoTriSan", new SqlParameter("@MaSan", maSan));
        }

        public static string Sp_XemThongTinSan(string maCoSo, bool isFix)
        {
            string spName = isFix ? "sp_XemThongTinSan_DaFix" : "sp_XemThongTinSan_CoLoi";
            return ExecuteScalarSP(spName, new SqlParameter("@MaCoSo", maCoSo));
        }

        // --- SCENARIO 15: Change Time & Booking ---
        public static string Sp_DoiGioSan(string maPhieuCu, string maSanMoi, DateTime batDauMoi, DateTime ketThucMoi)
        {
            return ExecuteScalarSP("sp_DoiGioSan", new SqlParameter("@MaPhieuDat_Cu", maPhieuCu),
                                                   new SqlParameter("@MaSan_Moi", maSanMoi),
                                                   new SqlParameter("@GioBatDau_Moi", batDauMoi),
                                                   new SqlParameter("@GioKetThuc_Moi", ketThucMoi));
        }

        public static string Sp_TimVaDatSanTrong(string maSan, DateTime batDau, DateTime ketThuc, string maKH, bool isFix)
        {
            string spName = isFix ? "sp_TimVaDatSanTrong_DaFix" : "sp_TimVaDatSanTrong_CoLoi";
            return ExecuteScalarSP(spName, new SqlParameter("@MaSan", maSan),
                                           new SqlParameter("@GioBatDau", batDau),
                                           new SqlParameter("@GioKetThuc", ketThuc),
                                           new SqlParameter("@MaKhachHang", maKH));
        }


        // Helper private để gọi SP trả về Message qua Output Parameter @KetQua
        private static string ExecuteScalarSP(string spName, params SqlParameter[] parameters)
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
                        
                        // Output param
                        SqlParameter outParam = new SqlParameter("@KetQua", SqlDbType.NVarChar, 500);
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();

                        return outParam.Value != DBNull.Value ? outParam.Value.ToString() : "Success";
                    }
                }
                catch (SqlException ex) { return "SQL Error: " + ex.Message; }
                catch (Exception ex) { return "Error: " + ex.Message; }
            }
        }

        // =============================================================
        // STUB FUNCTIONS FOR OTHER SCENARIOS (NOT RELATED TO 9/10)
        // =============================================================

        public static string NhapKho_GayXungDot(string maDV, int soLuong)
        {
            // Stub function - implement if needed
            return "Nhập kho thành công (stub)";
        }

        public static string ThueDungCu_GayXungDot(string maDV, int soLuong)
        {
            // Stub function - implement if needed
            return "Thuê dụng cụ thành công (stub)";
        }
    }
}