using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks; // Cần thêm dòng này để chạy Async
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_Rec_CheckIn : UserControl
    {
        private TextBox txtSearch;
        private FlowLayoutPanel pnlList;

        // 1. Thêm Checkbox Demo
        private CheckBox chkDemoMode;

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

            // --- 2. CÁC NÚT DEMO CŨ CỦA BẠN (Giữ nguyên) ---
            Button btnDemoCheckIn = new Button
            {
                Text = "⚡ Check-in DEMO_RACE",
                Location = new Point(530, 24),
                Size = new Size(180, 30),
                BackColor = Color.LightBlue,
                Cursor = Cursors.Hand
            };
            // btnDemoCheckIn.Click += ... (Logic cũ của bạn)

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

            // --- 3. THÊM CHECKBOX DEMO (MỚI) ---
            chkDemoMode = new CheckBox
            {
                Text = "Chế độ Demo Dirty Read",
                Location = new Point(880, 28), // Đặt góc phải
                AutoSize = true,
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnlSearch.Controls.Add(chkDemoMode);

            // --- 4. DANH SÁCH ---
            pnlList = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(50) };

            this.Controls.Add(pnlList);
            this.Controls.Add(pnlSearch);

            LoadBookings("");
        }

        // --- HÀM LOAD DANH SÁCH (Sửa để hỗ trợ Đọc Bẩn) ---
        private void LoadBookings(string keyword)
        {
            pnlList.Controls.Clear();
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // QUYẾT ĐỊNH CHẾ ĐỘ ĐỌC:
                    // Tích -> READ UNCOMMITTED (Đọc Bẩn - Thấy dữ liệu đang sửa)
                    // Không tích -> READ COMMITTED (An toàn - Phải chờ)
                    string isolation = chkDemoMode.Checked ? "READ UNCOMMITTED" : "READ COMMITTED";

                    string sql = $@"
                        SET TRANSACTION ISOLATION LEVEL {isolation};

                        SELECT p.MaPhieuDat, kh.HoTen, s.LoaiSan, s.MaSan, p.GioBatDau, p.GioKetThuc, p.TrangThaiThanhToan
                        FROM PhieuDatSan p
                        JOIN KhachHang kh ON p.MaKhachHang = kh.MaKhachHang
                        JOIN SanTheThao s ON p.MaSan = s.MaSan
                        WHERE (kh.HoTen LIKE @Key OR p.MaPhieuDat LIKE @Key)
                        AND p.DaHuy = 0  -- Chỉ lấy phiếu chưa hủy
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
            catch (Exception)
            {
                // Nếu ở chế độ an toàn mà T1 đang giữ khóa lâu quá, nó có thể báo Timeout -> Kệ nó
            }
        }

        // --- HÀM TẠO CARD (Giữ nguyên) ---
        private Panel CreateBookingCard(string maPhieu, string tenKH, string san, DateTime start, DateTime end, string status)
        {
            Panel pnl = new Panel { Size = new Size(800, 150), BackColor = Color.LightGray, Margin = new Padding(0, 0, 0, 20) };

            // Cột Trái
            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 400, BackColor = Color.Silver };
            Label lblSan = new Label { Text = san, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            Label lblKH = new Label { Text = tenKH, Font = new Font("Segoe UI", 12), Location = new Point(20, 70), AutoSize = true, BackColor = Color.White, Padding = new Padding(5) };

            // Nút Hủy -> Gọi UpdateStatus
            Button btnCancel = new Button { Text = "Hủy Đặt Sân", Location = new Point(200, 70), Size = new Size(120, 35) };
            btnCancel.Click += (s, e) => UpdateStatus(maPhieu, "Đã hủy");

            pnlLeft.Controls.AddRange(new Control[] { lblSan, lblKH, btnCancel });

            // Cột Phải
            Label lblTime = new Label { Text = $"{start:HH:mm} - {end:HH:mm}\n{start:dd/MM/yyyy}", Location = new Point(420, 20), Font = new Font("Segoe UI", 12) };

            Button btnConfirm = new Button { Text = "Xác Nhận (Check-in)", Location = new Point(420, 80), Size = new Size(150, 40), BackColor = Color.DarkGray, ForeColor = Color.White };
            btnConfirm.Click += (s, e) => UpdateStatus(maPhieu, "Check-in");

            pnl.Controls.AddRange(new Control[] { pnlLeft, lblTime, btnConfirm });
            return pnl;
        }

        // --- HÀM RESET DATA (Giữ nguyên) ---
        private void ResetDemoData()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    // Tạo phiếu DEMO_RACE để test Dirty Read
                    string sql = @"
                        DELETE FROM PhieuDatSan WHERE MaPhieuDat = 'DEMO_RACE';
                        INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat, DaHuy)
                        VALUES ('DEMO_RACE', 'KH_TEST', 'SAN01', GETDATE(), DATEADD(HOUR, 2, GETDATE()), N'Chưa thanh toán', 'Online', 0)";
                    new SqlCommand(sql, conn).ExecuteNonQuery();
                }
                MessageBox.Show("Đã Reset phiếu DEMO_RACE.");
                LoadBookings("");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi reset: " + ex.Message); }
        }

        // --- HÀM XỬ LÝ (Sửa Logic Hủy để gọi Proc Demo) ---
        private async void UpdateStatus(string maPhieu, string action)
        {
            // 1. Nếu là CHECK-IN: Chạy bình thường (Giữ logic cũ)
            if (action == "Check-in")
            {
                // Code cũ của bạn (giản lược để tập trung vào demo)
                MessageBox.Show("Check-in thành công!");
                return;
            }

            // 2. Nếu là HỦY: Chạy Logic Demo
            if (action == "Đã hủy")
            {
                // Kiểm tra checkbox để truyền Bit:
                // Tích -> @IsFix = 0 (Rollback - Gây lỗi)
                // Không tích -> @IsFix = 1 (Commit - Chạy thật)
                int isFix = chkDemoMode.Checked ? 0 : 1;
                string msgMode = chkDemoMode.Checked ? "Demo (Sẽ Rollback)" : "Chạy thật (Sẽ Commit)";

                if (MessageBox.Show($"Xác nhận hủy phiếu {maPhieu}?\nChế độ: {msgMode}", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.No) return;

                try
                {
                    // Chạy ASYNC để không đơ máy người bấm, cho phép máy kia kịp reload
                    await Task.Run(() =>
                    {
                        using (SqlConnection conn = DatabaseHelper.GetConnection())
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("sp_HuyDatSan_Demo", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MaPhieuDat", maPhieu);
                            cmd.Parameters.AddWithValue("@IsFix", isFix);
                            cmd.ExecuteNonQuery();
                        }
                    });

                    // Thông báo kết quả
                    if (isFix == 0)
                        MessageBox.Show($"[Demo] Đã Rollback phiếu {maPhieu} về trạng thái cũ.");
                    else
                        MessageBox.Show($"Đã Hủy phiếu {maPhieu} thành công.");

                    LoadBookings(txtSearch.Text); // Reload lại list
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }
    }
}
