using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_Rec_CheckIn : UserControl
    {
        private TextBox txtSearch;
        private FlowLayoutPanel pnlList;

        public UC_Rec_CheckIn()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(40, 40, 40);

            // --- 1. SEARCH BAR ---
            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };
            txtSearch = new TextBox { Location = new Point(100, 25), Width = 300, Font = new Font("Segoe UI", 12) };

            Button btnSearch = new Button { Text = "Tìm kiếm", Location = new Point(410, 24), Size = new Size(100, 30) };
            btnSearch.Click += (s, e) => LoadBookings(txtSearch.Text);

            pnlSearch.Controls.Add(new Label { Text = "Tên KH:", Location = new Point(20, 28) });
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnSearch);

            // --- 2. CÁC NÚT DEMO (Quan trọng cho Scenario 11) ---
            Button btnDemoCheckIn = new Button
            {
                Text = "⚡ Check-in DEMO_RACE",
                Location = new Point(530, 24),
                Size = new Size(180, 30),
                BackColor = Color.LightBlue,
                Cursor = Cursors.Hand
            };
            btnDemoCheckIn.Click += (s, e) => RunRecCheckIn();

            Button btnResetDemo = new Button
            {
                Text = "🔄 Reset Data Demo",
                Location = new Point(720, 24),
                Size = new Size(150, 30),
                BackColor = Color.LightGray,
                Cursor = Cursors.Hand
            };
            btnResetDemo.Click += (s, e) => ResetDemoData();

            pnlSearch.Controls.Add(btnDemoCheckIn);
            pnlSearch.Controls.Add(btnResetDemo);

            // --- 3. DANH SÁCH ---
            pnlList = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(50) };

            this.Controls.Add(pnlList);
            this.Controls.Add(pnlSearch);

            LoadBookings("");
        }

        // --- HÀM 1: CHẠY DEMO CHECK-IN (GỌI SP CÓ LOCK) ---
        private void RunRecCheckIn()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_Demo_Rec_CheckIn", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaPhieu", "DEMO_RACE");

                    // Lệnh này sẽ bị TREO (Blocked) nếu System đang chạy Tự động hủy
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("LỄ TÂN: Check-in thành công!");
                    LoadBookings(""); // Refresh lại list
                }
            }
            catch (SqlException ex)
            {
                // Bắt lỗi 50005 (Do SP ném ra khi phát hiện phiếu đã bị hủy)
                MessageBox.Show("LỄ TÂN GẶP LỖI (Kết quả mong đợi):\n" + ex.Message, "Xung đột dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadBookings("");
            }
        }

        // --- HÀM 2: LOAD DANH SÁCH (CHỈ HIỆN PHIẾU CHƯA HỦY) ---
        private void LoadBookings(string keyword)
        {
            pnlList.Controls.Clear();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Lọc p.DaHuy = 0 để ẩn các phiếu đã hủy
                string sql = @"
                    SELECT p.MaPhieuDat, kh.HoTen, s.LoaiSan, s.MaSan, p.GioBatDau, p.GioKetThuc, p.TrangThaiThanhToan
                    FROM PhieuDatSan p
                    JOIN KhachHang kh ON p.MaKhachHang = kh.MaKhachHang
                    JOIN SanTheThao s ON p.MaSan = s.MaSan
                    WHERE (kh.HoTen LIKE @Key OR p.MaPhieuDat LIKE @Key)
                    AND p.DaHuy = 0
                    AND (@Key != '' OR CONVERT(date, p.GioBatDau) = CONVERT(date, GETDATE()))";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Key", "%" + keyword + "%");
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pnlList.Controls.Add(CreateBookingCard(
                                reader["MaPhieuDat"].ToString(),
                                reader["HoTen"].ToString(),
                                reader["MaSan"].ToString() + " - " + reader["LoaiSan"].ToString(),
                                DateTime.Parse(reader["GioBatDau"].ToString()),
                                DateTime.Parse(reader["GioKetThuc"].ToString()),
                                reader["TrangThaiThanhToan"].ToString()
                            ));
                        }
                    }
                }
            }
        }

        // --- HÀM 3: TẠO GIAO DIỆN THẺ ---
        private Panel CreateBookingCard(string maPhieu, string tenKH, string san, DateTime start, DateTime end, string status)
        {
            Panel pnl = new Panel { Size = new Size(800, 150), BackColor = Color.LightGray, Margin = new Padding(0, 0, 0, 20) };

            // Cột Trái
            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 400, BackColor = Color.Silver };
            Label lblSan = new Label { Text = san, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            Label lblKH = new Label { Text = tenKH, Font = new Font("Segoe UI", 12), Location = new Point(20, 70), AutoSize = true, BackColor = Color.White, Padding = new Padding(5) };

            // Nút Hủy (Thường)
            Button btnCancel = new Button { Text = "Hủy Đặt Sân", Location = new Point(200, 70), Size = new Size(120, 35) };
            btnCancel.Click += (s, e) => UpdateStatus(maPhieu, "Đã hủy");

            pnlLeft.Controls.AddRange(new Control[] { lblSan, lblKH, btnCancel });

            // Cột Phải
            Label lblTime = new Label { Text = $"{start:HH:mm} - {end:HH:mm}\n{start:dd/MM/yyyy}", Location = new Point(420, 20), Font = new Font("Segoe UI", 12) };

            // Nút Check-in (Thường)
            Button btnConfirm = new Button { Text = "Xác Nhận (Check-in)", Location = new Point(420, 80), Size = new Size(150, 40), BackColor = Color.DarkGray, ForeColor = Color.White };
            btnConfirm.Click += (s, e) => UpdateStatus(maPhieu, "Check-in");

            pnl.Controls.AddRange(new Control[] { pnlLeft, lblTime, btnConfirm });
            return pnl;
        }

        // --- HÀM 4: RESET DATA (ĐỂ TEST LẠI) ---
        private void ResetDemoData()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    // Tạo lại phiếu DEMO_RACE ở trạng thái chưa thanh toán, chưa hủy
                    string check = "SELECT COUNT(*) FROM PhieuDatSan WHERE MaPhieuDat = 'DEMO_RACE'";
                    SqlCommand cmdCheck = new SqlCommand(check, conn);

                    string sqlExec = "";
                    if ((int)cmdCheck.ExecuteScalar() == 0)
                        sqlExec = @"INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat, DaHuy)
                                    VALUES ('DEMO_RACE', 'KH_TEST', 'SAN01', GETDATE(), DATEADD(HOUR, 2, GETDATE()), N'Chưa thanh toán', 'Online', 0)";
                    else
                        sqlExec = "UPDATE PhieuDatSan SET DaHuy = 0, TrangThaiThanhToan = N'Chưa thanh toán' WHERE MaPhieuDat = 'DEMO_RACE'";

                    new SqlCommand(sqlExec, conn).ExecuteNonQuery();
                }
                MessageBox.Show("Đã Reset phiếu DEMO_RACE. Sẵn sàng test!");
                LoadBookings("");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi reset: " + ex.Message); }
        }

        // --- HÀM 5: XỬ LÝ CHECK-IN / HỦY (LOGIC CHUẨN) ---
        private void UpdateStatus(string maPhieu, string action)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "";

                // Phân biệt Hủy và Check-in để tránh lỗi Constraint
                if (action == "Đã hủy")
                {
                    // Nếu Hủy -> Update cột DaHuy = 1
                    sql = "UPDATE PhieuDatSan SET DaHuy = 1 WHERE MaPhieuDat = @Ma";
                }
                else if (action == "Check-in")
                {
                    // Nếu Check-in -> Update TrangThai = Đã thanh toán
                    sql = "UPDATE PhieuDatSan SET TrangThaiThanhToan = N'Đã thanh toán' WHERE MaPhieuDat = @Ma";
                }

                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Ma", maPhieu);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Thao tác thành công!");
                    LoadBookings(txtSearch.Text); // Reload để ẩn phiếu
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
    }
}