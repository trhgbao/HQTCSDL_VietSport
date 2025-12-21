using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VietSportSystem
{
    public class UC_Cashier_Payment : UserControl
    {
        // Search
        private TextBox txtSearch;
        private DataGridView gridSearch; // Grid tạm để chọn phiếu cần thanh toán

        // Left Panel (Input)
        private Label lblKhach, lblSan;
        private TextBox txtTienKhach;
        private Button btnConfirm;

        // Right Panel (Bill)
        private Label lblBillMa, lblBillSan, lblBillTime, lblBillTotal, lblBillThua;
        private Button btnPrint;

        // Data Variables
        private string _maPhieu = "";
        private decimal _tongTien = 0;

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

            Label lblInputMoney = new Label { Text = "Số tiền khách đưa:", Location = new Point(30, 130), Font = new Font("Segoe UI", 10) };
            txtTienKhach = new TextBox { Location = new Point(30, 160), Width = 300, Font = new Font("Segoe UI", 14), Text = "0" };
            txtTienKhach.TextChanged += TxtTienKhach_TextChanged;
            txtTienKhach.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };

            btnConfirm = new Button { Text = "Xác Nhận Thanh Toán", Location = new Point(30, 220), Size = new Size(250, 50), BackColor = Color.LightGray, Enabled = false };
            btnConfirm.Click += BtnConfirm_Click;

            // --- SỬA VỊ TRÍ QR (Đẩy xuống dưới) ---
            Label lblQR = new Label { Text = "MÃ QR THANH TOÁN", Location = new Point(30, 300), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            PictureBox picQR = new PictureBox { Size = new Size(150, 150), Location = new Point(30, 330), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            // Load ảnh QR tĩnh nếu có
            string pathQR = Application.StartupPath + "\\Images\\qr_static.jpg";
            if (System.IO.File.Exists(pathQR)) picQR.Image = Image.FromFile(pathQR);
            // --------------------------------------

            pnlLeft.Controls.AddRange(new Control[] { lblKhach, lblSan, lblInputMoney, txtTienKhach, btnConfirm, picQR, lblQR });

            // --- CỘT PHẢI (HÓA ĐƠN) ---
            Panel pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke };
            Label lblBillTitle = new Label { Text = "THÔNG TIN ĐƠN HÀNG", Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Silver, Font = new Font("Segoe UI", 12, FontStyle.Bold) };

            lblBillMa = CreateLabel("Mã đặt sân: ...", 30, 80, false);
            lblBillSan = CreateLabel("Sân: ...", 30, 120, false);
            lblBillTime = CreateLabel("Thời gian: ...", 30, 160, false);

            lblBillTotal = CreateLabel("Tổng thành tiền: 0 VNĐ", 30, 240, true);
            lblBillTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblBillTotal.ForeColor = Color.Red;

            lblBillThua = CreateLabel("Tiền thừa: 0 VNĐ", 30, 280, true);
            lblBillThua.ForeColor = Color.Blue;

            btnPrint = new Button { Text = "In hóa đơn", Location = new Point(150, 400), Size = new Size(200, 50), Enabled = false, BackColor = UIHelper.PrimaryColor, ForeColor = Color.White };
            btnPrint.Click += BtnPrint_Click;

            pnlRight.Controls.AddRange(new Control[] { lblBillTitle, lblBillMa, lblBillSan, lblBillTime, lblBillTotal, lblBillThua, btnPrint });

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
                // Tìm phiếu đặt CHƯA có hóa đơn (hoặc chưa thanh toán xong)
                string sql = @"
                    SELECT p.MaPhieuDat, kh.HoTen, s.LoaiSan, s.MaSan, p.GioBatDau, p.GioKetThuc, g.DonGia
                    FROM PhieuDatSan p
                    JOIN KhachHang kh ON p.MaKhachHang = kh.MaKhachHang
                    JOIN SanTheThao s ON p.MaSan = s.MaSan
                    JOIN GiaThueSan g ON s.MaCoSo = g.MaCoSo AND s.LoaiSan = g.LoaiSan
                    WHERE (p.MaPhieuDat LIKE @Key OR kh.HoTen LIKE @Key)
                    AND p.MaPhieuDat NOT IN (SELECT MaPhieuDat FROM HoaDon)"; // Chỉ tìm cái chưa xuất hóa đơn

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Key", "%" + txtSearch.Text + "%");
                System.Data.DataTable dt = new System.Data.DataTable();
                da.Fill(dt);

                gridSearch.DataSource = dt;
                gridSearch.Visible = true;

                if (dt.Rows.Count == 0) MessageBox.Show("Không tìm thấy phiếu đặt nào chưa thanh toán!");
            }
        }

        // --- LOGIC 2: CHỌN PHIẾU ---
        private void GridSearch_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = gridSearch.Rows[e.RowIndex];
            _maPhieu = row.Cells["MaPhieuDat"].Value.ToString();
            string tenKH = row.Cells["HoTen"].Value.ToString();
            string tenSan = row.Cells["LoaiSan"].Value.ToString();

            DateTime start = DateTime.Parse(row.Cells["GioBatDau"].Value.ToString());
            DateTime end = DateTime.Parse(row.Cells["GioKetThuc"].Value.ToString());
            decimal donGia = Convert.ToDecimal(row.Cells["DonGia"].Value);

            // Tính tiền: (Giờ * Đơn giá)
            double hours = (end - start).TotalHours;
            _tongTien = (decimal)hours * donGia;

            // Hiển thị lên giao diện
            lblKhach.Text = "Tên khách hàng: " + tenKH;
            lblSan.Text = "Sân: " + tenSan;

            lblBillMa.Text = "Mã đặt sân: " + _maPhieu;
            lblBillSan.Text = "Sân: " + tenSan;
            lblBillTime.Text = $"Thời gian: {start:dd/MM} {start:HH:mm}-{end:HH:mm}";
            lblBillTotal.Text = "Tổng thành tiền: " + _tongTien.ToString("N0") + " VND";

            // Ẩn grid đi cho gọn
            gridSearch.Visible = false;
            txtTienKhach.Focus();
            btnConfirm.Enabled = true;
            btnConfirm.BackColor = Color.LightGreen;
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
                    lblBillThua.ForeColor = Color.Red; // Thiếu tiền
                    btnConfirm.Enabled = false;
                }
            }
        }

        // --- LOGIC 4: XÁC NHẬN & LƯU HÓA ĐƠN ---
        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            // --- THÊM CHECK LỖI ---
            if (string.IsNullOrEmpty(_maPhieu))
            {
                MessageBox.Show("Vui lòng chọn phiếu đặt sân cần thanh toán trước!", "Chưa chọn phiếu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // ---------------------

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                // ... (Giữ nguyên code xử lý SQL bên trong như cũ) ...
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // Copy lại đoạn Insert HoaDon và Update PhieuDatSan ở bài trước vào đây
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
                    MessageBox.Show("Thanh toán thành công!");
                    btnConfirm.Enabled = false;
                    btnPrint.Enabled = true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        // --- LOGIC 5: IN HÓA ĐƠN (Giả lập) ---
        private void BtnPrint_Click(object sender, EventArgs e)
        {
            string receipt =
                "====== VIETSPORT SYSTEM ======\n" +
                "====== HÓA ĐƠN THANH TOÁN ======\n\n" +
                $"Mã phiếu: {_maPhieu}\n" +
                $"Khách hàng: {lblKhach.Text.Replace("Tên khách hàng: ", "")}\n" +
                $"Thời gian: {DateTime.Now}\n" +
                "------------------------------\n" +
                $"TỔNG TIỀN: {_tongTien:N0} VND\n" +
                $"Tiền khách đưa: {decimal.Parse(txtTienKhach.Text):N0} VND\n" +
                $"Tiền thừa: {lblBillThua.Text.Replace("Tiền thừa: ", "")}\n" +
                "------------------------------\n" +
                "Cảm ơn quý khách và hẹn gặp lại!";

            MessageBox.Show(receipt, "Đang in hóa đơn...");

            // Reset form để làm phiếu mới
            _maPhieu = "";
            txtSearch.Text = "";
            txtTienKhach.Text = "0";
            lblBillTotal.Text = "0 VND";
            lblBillThua.Text = "0 VND";
            btnPrint.Enabled = false;
        }
    }
}