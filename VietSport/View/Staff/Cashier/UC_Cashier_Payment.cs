using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace VietSportSystem
{
    public class UC_Cashier_Payment : UserControl
    {
        // Search
        private TextBox txtSearch;
        private DataGridView gridSearch;

        // Left Panel (Input)
        private Label lblKhach, lblSan;
        private TextBox txtTienKhach;
        private Button btnConfirm;

        // Right Panel (Bill)
        private Label lblBillMa, lblBillSan, lblBillTime, lblBillTotal, lblBillThua;
        private Label lblBillGiaGoc, lblBillDiscount; // Mới: Hiển thị giảm giá
        private Button btnPrint;

        // Demo Controls
        private CheckBox chkDemoMode; // Demo Scenario 9
        private Label lblDemoStatus;

        // Data Variables
        private string _maPhieu = "";
        private decimal _tongTien = 0;
        private decimal _giaGoc = 0;
        private decimal _giamGia = 0; // % giảm giá
        private string _hangTV = "Standard";

        public UC_Cashier_Payment()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(40, 40, 40);

            // 1. THANH TÌM KIẾM
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.WhiteSmoke };
            txtSearch = new TextBox { Location = new Point(150, 25), Width = 350, Font = new Font("Segoe UI", 12) };
            txtSearch.PlaceholderText = "Nhập mã phiếu hoặc tên khách...";

            Button btnSearch = new Button { Text = "Tìm kiếm", Location = new Point(520, 24), Size = new Size(100, 32) };
            btnSearch.Click += (s, e) => SearchBooking();

            pnlTop.Controls.AddRange(new Control[] { new Label { Text = "Tìm kiếm:", Location = new Point(50, 28) }, txtSearch, btnSearch });

            // Grid kết quả tìm kiếm
            gridSearch = new DataGridView
            {
                Height = 150,
                Dock = DockStyle.Top,
                BackgroundColor = Color.White,
                Visible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            gridSearch.CellClick += GridSearch_CellClick;

            // 2. KHUNG CHÍNH
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            Panel pnlCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            // --- CỘT TRÁI (NHẬP LIỆU) ---
            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 500, BackColor = Color.FromArgb(230, 230, 230) };

            lblKhach = CreateLabel("Tên khách hàng: (Chưa chọn)", 30, 30, true);
            lblSan = CreateLabel("Sân: (Chưa chọn)", 30, 70, true);

            // Tăng Y lên để tách biệt
            Label lblInputMoney = new Label { Text = "Số tiền khách đưa:", Location = new Point(30, 140), Font = new Font("Segoe UI", 10) };

            txtTienKhach = new TextBox { Location = new Point(30, 170), Width = 300, Font = new Font("Segoe UI", 14), Text = "0" };
            txtTienKhach.TextChanged += TxtTienKhach_TextChanged;
            txtTienKhach.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };

            btnConfirm = new Button { Text = "Xác Nhận Thanh Toán", Location = new Point(30, 230), Size = new Size(250, 50), BackColor = Color.LightGray, Enabled = false };
            btnConfirm.Click += BtnConfirm_Click;

            // Checkbox Demo (Scenario 9)
            chkDemoMode = new CheckBox { Text = "Demo: Test Non-Repeatable Read (delay 10s)", Location = new Point(30, 300), AutoSize = true, ForeColor = Color.Blue };

            // QR Code (Đẩy xuống dưới cùng hoặc sang bên phải)
            Label lblQR = new Label { Text = "MÃ QR", Location = new Point(350, 280), Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            PictureBox picQR = new PictureBox { Size = new Size(120, 120), Location = new Point(350, 150), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom }; string pathQR = Application.StartupPath + "\\Images\\qr_static.jpg";
            if (System.IO.File.Exists(pathQR)) picQR.Image = Image.FromFile(pathQR);

            pnlLeft.Controls.AddRange(new Control[] { lblKhach, lblSan, lblInputMoney, txtTienKhach, btnConfirm, picQR, lblQR, chkDemoMode, lblDemoStatus });

            // --- CỘT PHẢI (HÓA ĐƠN) ---
            Panel pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke };
            Label lblBillTitle = new Label { Text = "THÔNG TIN ĐƠN HÀNG", Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Silver, Font = new Font("Segoe UI", 12, FontStyle.Bold) };

            lblBillMa = CreateLabel("Mã đặt sân: ...", 30, 80, false);
            lblBillSan = CreateLabel("Sân: ...", 30, 120, false);
            lblBillTime = CreateLabel("Thời gian: ...", 30, 160, false);

            // Labels giảm giá
            lblBillGiaGoc = CreateLabel("Giá gốc: 0 VNĐ", 30, 200, false);
            lblBillDiscount = CreateLabel("Giảm giá (hạng TV): 0%", 30, 230, false);
            lblBillDiscount.ForeColor = Color.Green;

            lblBillTotal = CreateLabel("Thành tiền sau giảm: 0 VNĐ", 30, 270, true);
            lblBillTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblBillTotal.ForeColor = Color.Red;

            lblBillThua = CreateLabel("Tiền thừa: 0 VNĐ", 30, 310, true);
            lblBillThua.ForeColor = Color.Blue;

            btnPrint = new Button { Text = "In hóa đơn", Location = new Point(150, 400), Size = new Size(200, 50), Enabled = false, BackColor = UIHelper.PrimaryColor, ForeColor = Color.White };
            btnPrint.Click += BtnPrint_Click;

            pnlRight.Controls.AddRange(new Control[] { lblBillTitle, lblBillMa, lblBillSan, lblBillTime, lblBillGiaGoc, lblBillDiscount, lblBillTotal, lblBillThua, btnPrint });

            pnlCard.Controls.Add(pnlRight);
            pnlCard.Controls.Add(pnlLeft);
            pnlMain.Controls.Add(pnlCard);

            this.Controls.Add(pnlMain);
            this.Controls.Add(gridSearch);
            this.Controls.Add(pnlTop);
        }

        private Label CreateLabel(string text, int x, int y, bool bold)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        // --- LOGIC 1: TÌM KIẾM ---
        private void SearchBooking()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // SỬA CÂU SQL: Bỏ JOIN GiaThueSan, lấy DonGia bằng sub-query hoặc để 0 tính sau
                string sql = @"
            SELECT DISTINCT p.MaPhieuDat, kh.HoTen, s.LoaiSan, s.MaSan, p.GioBatDau, p.GioKetThuc
            FROM PhieuDatSan p
            JOIN KhachHang kh ON p.MaKhachHang = kh.MaKhachHang
            JOIN SanTheThao s ON p.MaSan = s.MaSan
            WHERE (p.MaPhieuDat LIKE @Key OR kh.HoTen LIKE @Key)
            AND p.MaPhieuDat NOT IN (SELECT MaPhieuDat FROM HoaDon)
            AND p.TrangThaiThanhToan != N'Đã hủy' AND p.DaHuy = 0"; // Thêm điều kiện chưa hủy

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Key", "%" + txtSearch.Text + "%");
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Thêm cột DonGia giả định để tránh lỗi code cũ đọc cell
                if (!dt.Columns.Contains("DonGia"))
                {
                    dt.Columns.Add("DonGia", typeof(decimal));
                    foreach (DataRow r in dt.Rows) r["DonGia"] = 0; // Giá sẽ được tính lại chính xác khi Click
                }

                gridSearch.DataSource = dt;
                gridSearch.Visible = true;

                // Ẩn cột DonGia đi cho đỡ rối
                if (gridSearch.Columns["DonGia"] != null) gridSearch.Columns["DonGia"].Visible = false;

                if (dt.Rows.Count == 0) MessageBox.Show("Không tìm thấy phiếu đặt nào chưa thanh toán!");
            }
        }
        // --- LOGIC 2: CHỌN PHIẾU ---
        private void GridSearch_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                // 1. Lấy thông tin cơ bản từ dòng đã chọn
                var row = gridSearch.Rows[e.RowIndex];
                _maPhieu = row.Cells["MaPhieuDat"].Value?.ToString() ?? "";
                string tenKH = row.Cells["HoTen"].Value?.ToString() ?? "N/A";
                string tenLoaiSan = row.Cells["LoaiSan"].Value?.ToString() ?? "N/A";
                string maSan = row.Cells["MaSan"].Value?.ToString() ?? ""; // Lấy mã sân

                DateTime start = DateTime.Parse(row.Cells["GioBatDau"].Value.ToString());
                DateTime end = DateTime.Parse(row.Cells["GioKetThuc"].Value.ToString());

                // 2. Tính số giờ
                double hours = Math.Abs((end - start).TotalHours);
                if (hours < 0.5) hours = 1;

                // 3. Kết nối Database để lấy: ĐƠN GIÁ và GIẢM GIÁ
                decimal donGia = 0;

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // --- A. LẤY ĐƠN GIÁ (Dựa vào MaSan và Thứ trong tuần) ---
                    // Xác định khung giờ: Cuối tuần hay Ngày thường
                    string khungGio = (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
                                      ? "Cuối tuần" : "Ngày thường";

                    string sqlGia = @"
                SELECT TOP 1 g.DonGia 
                FROM GiaThueSan g
                JOIN SanTheThao s ON g.MaCoSo = s.MaCoSo AND g.LoaiSan = s.LoaiSan
                WHERE s.MaSan = @MaSan AND g.KhungGio = @Khung";

                    SqlCommand cmdGia = new SqlCommand(sqlGia, conn);
                    cmdGia.Parameters.AddWithValue("@MaSan", maSan);
                    cmdGia.Parameters.AddWithValue("@Khung", khungGio);

                    object resGia = cmdGia.ExecuteScalar();
                    if (resGia != null && resGia != DBNull.Value)
                    {
                        donGia = Convert.ToDecimal(resGia);
                    }
                    else
                    {
                        donGia = 100000; // Giá mặc định nếu chưa cấu hình
                    }

                    // --- B. LẤY HẠNG THÀNH VIÊN & GIẢM GIÁ ---
                    string sqlRank = @"
                SELECT kh.CapBacThanhVien, ISNULL(cs.GiamGia, 0) as GiamGia
                FROM PhieuDatSan p 
                JOIN KhachHang kh ON p.MaKhachHang = kh.MaKhachHang 
                LEFT JOIN ChinhSachGiamGia cs ON kh.CapBacThanhVien = cs.HangTV
                WHERE p.MaPhieuDat = @MaPhieu";

                    SqlCommand cmdRank = new SqlCommand(sqlRank, conn);
                    cmdRank.Parameters.AddWithValue("@MaPhieu", _maPhieu);

                    SqlDataReader reader = cmdRank.ExecuteReader();
                    if (reader.Read())
                    {
                        _hangTV = reader["CapBacThanhVien"].ToString();
                        _giamGia = Convert.ToDecimal(reader["GiamGia"]);
                    }
                    else
                    {
                        _hangTV = "Standard";
                        _giamGia = 0;
                    }
                }

                // 4. Tính toán tổng tiền
                _giaGoc = (decimal)hours * donGia;
                _tongTien = _giaGoc * (100 - _giamGia) / 100;

                // 5. Hiển thị lên giao diện
                lblKhach.Text = "Tên khách hàng: " + tenKH + $" ({_hangTV})";
                lblSan.Text = "Sân: " + tenLoaiSan; // Hiện loại sân hoặc mã sân tùy ý

                lblBillMa.Text = "Mã đặt sân: " + _maPhieu;
                lblBillSan.Text = "Sân: " + maSan;
                lblBillTime.Text = $"Thời gian: {start:dd/MM} {start:HH:mm}-{end:HH:mm} ({hours}h)";

                lblBillGiaGoc.Text = $"Giá gốc: {_giaGoc:N0} VND ({donGia:N0}/h)";
                lblBillDiscount.Text = $"Giảm giá ({_hangTV}): {_giamGia}%";
                lblBillTotal.Text = "Thành tiền: " + _tongTien.ToString("N0") + " VND";

                // UI Update
                gridSearch.Visible = false;
                txtTienKhach.Text = "0"; // Reset tiền khách đưa
                txtTienKhach.Focus();
                btnConfirm.Enabled = true;
                btnConfirm.BackColor = Color.LightGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi chọn phiếu: " + ex.Message);
            }
        }

        // --- LOGIC 3: TÍNH TIỀN THỪA ---
        private void TxtTienKhach_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtTienKhach.Text, out decimal tienKhach))
            {
                decimal tienThua = tienKhach - _tongTien;
                lblBillThua.Text = "Tiền thừa: " + tienThua.ToString("N0") + " VND";

                if (tienThua >= 0)
                {
                    lblBillThua.ForeColor = Color.Blue;
                    btnConfirm.Enabled = true;
                }
                else
                {
                    lblBillThua.ForeColor = Color.Red;
                    btnConfirm.Enabled = false;
                }
            }
        }

        // --- LOGIC 4: XÁC NHẬN THANH TOÁN ---
        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_maPhieu))
            {
                MessageBox.Show("Vui lòng chọn phiếu đặt sân trước!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- DEMO MODE: Bỏ qua check giá (Giả lập SP đã fix) ---
            if (chkDemoMode.Checked)
            {
                // Chạy thẳng vào thanh toán với giá cũ
                ProcessPayment();
                return;
            }

            // --- NORMAL MODE: Kiểm tra lại giá (Check Non-Repeatable Read) ---
            decimal currentDiscount = 0;
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sqlDiscount = "SELECT ISNULL(GiamGia, 0) FROM ChinhSachGiamGia WHERE HangTV = @Hang";
                using (SqlCommand cmd = new SqlCommand(sqlDiscount, conn))
                {
                    cmd.Parameters.AddWithValue("@Hang", _hangTV);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        currentDiscount = Convert.ToDecimal(result);
                }
            }

            // Nếu giá thay đổi (do Admin vừa sửa)
            if (currentDiscount != _giamGia)
            {
                _giamGia = currentDiscount;
                _tongTien = _giaGoc * (100 - _giamGia) / 100;

                lblBillDiscount.Text = $"Giảm giá ({_hangTV}): {_giamGia}%";
                lblBillDiscount.ForeColor = Color.Red;
                lblBillTotal.Text = "Thành tiền: " + _tongTien.ToString("N0") + " VND";

                MessageBox.Show($"⚠️ CẢNH BÁO: Chính sách giá vừa thay đổi!\n\nGiảm giá mới: {_giamGia}%\nThành tiền mới: {_tongTien:N0} VND\n\nVui lòng kiểm tra lại tiền khách và xác nhận lại.", "Giá đã thay đổi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ProcessPayment();
        }

        private void ProcessPayment()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string maHD = "HD" + DateTime.Now.ToString("ddHHmm");
                    string sqlHD = @"INSERT INTO HoaDon (MaHoaDon, MaPhieuDat, MaNhanVien, TongTien, GhiChu) 
                                     VALUES (@Ma, @Phieu, @NV, @Tien, N'Thanh toán tại quầy')";

                    SqlCommand cmd = new SqlCommand(sqlHD, conn, trans);
                    cmd.Parameters.AddWithValue("@Ma", maHD);
                    cmd.Parameters.AddWithValue("@Phieu", _maPhieu);
                    object maNV = SessionData.CurrentUserID ?? (object)DBNull.Value;
                    cmd.Parameters.AddWithValue("@NV", maNV);
                    cmd.Parameters.AddWithValue("@Tien", _tongTien);
                    cmd.ExecuteNonQuery();

                    SqlCommand cmdUpdate = new SqlCommand("UPDATE PhieuDatSan SET TrangThaiThanhToan = N'Đã thanh toán' WHERE MaPhieuDat = @Phieu", conn, trans);
                    cmdUpdate.Parameters.AddWithValue("@Phieu", _maPhieu);
                    cmdUpdate.ExecuteNonQuery();

                    trans.Commit();
                    MessageBox.Show($"Thanh toán thành công!\n\nGiảm giá áp dụng: {_giamGia}%\nTổng tiền: {_tongTien:N0} VND");

                    btnConfirm.Enabled = false;
                    btnPrint.Enabled = true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi thanh toán: " + ex.Message);
                }
            }
        }

        // --- LOGIC 5: IN HÓA ĐƠN ---
        private void BtnPrint_Click(object sender, EventArgs e)
        {
            string receipt =
                "====== VIETSPORT SYSTEM ======\n" +
                "====== HÓA ĐƠN THANH TOÁN ======\n\n" +
                $"Mã phiếu: {_maPhieu}\n" +
                $"Khách hàng: {lblKhach.Text.Replace("Tên khách hàng: ", "")}\n" +
                $"Thời gian: {DateTime.Now}\n" +
                "------------------------------\n" +
                $"Giá gốc: {_giaGoc:N0} VND\n" +
                $"Giảm giá: {_giamGia}%\n" +
                $"TỔNG TIỀN: {_tongTien:N0} VND\n" +
                $"Tiền khách đưa: {decimal.Parse(txtTienKhach.Text):N0} VND\n" +
                $"Tiền thừa: {lblBillThua.Text.Replace("Tiền thừa: ", "")}\n" +
                "------------------------------\n" +
                "Cảm ơn quý khách và hẹn gặp lại!";

            MessageBox.Show(receipt, "Đang in hóa đơn...");

            // Reset
            _maPhieu = "";
            txtSearch.Text = "";
            txtTienKhach.Text = "0";
            lblBillTotal.Text = "0 VND";
            lblBillThua.Text = "0 VND";
            btnPrint.Enabled = false;
        }
    }
}