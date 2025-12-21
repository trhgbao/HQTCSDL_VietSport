using System;
using System.Drawing;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_BookingConfirm : UserControl
    {
        private MainForm _mainForm;
        private SanInfo _sanInfo;

        // Controls
        private DateTimePicker dtpStart, dtpEnd;
        private Label lblDuration, lblTotalPrice;
        private TextBox txtNote, txtVoucher;
        private decimal currentTotal;

        public UC_BookingConfirm(MainForm main, SanInfo san)
        {
            _mainForm = main;
            _sanInfo = san;
            InitializeComponent();
            CalculateTotal(); // Tính tiền lần đầu
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.WhiteSmoke;
            this.Dock = DockStyle.Fill;

            // 1. Container chính (Hộp trắng ở giữa)
            Panel pnlContainer = new Panel { Size = new Size(900, 500), BackColor = Color.White };

            // Logic tự động căn giữa màn hình
            pnlContainer.Location = new Point((this.Width - 900) / 2, 50);
            this.Resize += (s, e) => {
                pnlContainer.Left = (this.Width - pnlContainer.Width) / 2;
            };

            // 2. Header (Thanh xanh đậm tiêu đề)
            Label lblTitle = new Label
            {
                Text = "XÁC NHẬN ĐẶT SÂN",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = UIHelper.HeaderFont,
                BackColor = UIHelper.SecondaryColor,
                ForeColor = Color.White
            };

            // 3. Grid chia cột (Thêm Padding top = 30 để đẩy nội dung xuống -> KHẮC PHỤC LỖI LỆCH)
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                // Padding(Left, Top, Right, Bottom) -> Top=30 giúp chữ không bị dính lên trên
                Padding = new Padding(20, 30, 20, 20)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // Cột trái 60%
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Cột phải 40%

            // --- CỘT TRÁI (Thông tin & Giờ) ---
            Panel pnlLeft = new Panel { Dock = DockStyle.Fill };

            Label lblSan = new Label { Text = $"SÂN: {_sanInfo.TenSan}", Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, ForeColor = UIHelper.PrimaryColor };
            Label lblGia = new Label { Text = $"Đơn giá: {_sanInfo.GiaTien:N0} VNĐ/giờ", Location = new Point(0, 30), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Italic) };

            // GroupBox chọn giờ
            GroupBox grpTime = new GroupBox { Text = "Thời gian đặt", Location = new Point(0, 70), Size = new Size(480, 100), Font = new Font("Segoe UI", 10) };

            Label lblS = new Label { Text = "Bắt đầu:", Location = new Point(20, 35), AutoSize = true };
            dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Location = new Point(90, 32), Width = 160 };
            // Mặc định: Giờ hiện tại + 1 tiếng, phút chẵn
            dtpStart.Value = DateTime.Now.AddHours(1).Date.AddHours(DateTime.Now.Hour + 1);
            dtpStart.ValueChanged += Time_Changed;

            Label lblE = new Label { Text = "Kết thúc:", Location = new Point(20, 70), AutoSize = true };
            dtpEnd = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Location = new Point(90, 67), Width = 160 };
            dtpEnd.Value = dtpStart.Value.AddHours(1);
            dtpEnd.ValueChanged += Time_Changed;

            lblDuration = new Label { Text = "(1 giờ)", Location = new Point(270, 67), AutoSize = true, ForeColor = Color.Blue, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            grpTime.Controls.AddRange(new Control[] { lblS, dtpStart, lblE, dtpEnd, lblDuration });

            // Ghi chú
            Label lblNote = new Label { Text = "Ghi chú:", Location = new Point(0, 190), AutoSize = true };
            txtNote = new TextBox { Location = new Point(0, 215), Width = 480, Height = 80, Multiline = true, BorderStyle = BorderStyle.FixedSingle };

            pnlLeft.Controls.AddRange(new Control[] { lblSan, lblGia, grpTime, lblNote, txtNote });


            // --- CỘT PHẢI (Thanh toán) ---
            Panel pnlRight = new Panel { Dock = DockStyle.Fill };
            // Để cột phải background màu trắng luôn cho đồng bộ hoặc màu xám nhạt tùy ý

            Label lblPayTitle = new Label { Text = "THANH TOÁN", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 0), AutoSize = true };

            lblTotalPrice = new Label
            {
                Text = "0 VNĐ",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Red,
                Location = new Point(20, 40),
                AutoSize = true
            };

            Label lblVoucher = new Label { Text = "Mã giảm giá:", Location = new Point(20, 100), AutoSize = true };
            txtVoucher = new TextBox { Location = new Point(20, 125), Width = 180, Font = UIHelper.MainFont };
            Button btnApply = new Button { Text = "Áp dụng", Location = new Point(210, 124), Size = new Size(80, 29) };
            UIHelper.StyleButton(btnApply, false);
            btnApply.Click += (s, e) => MessageBox.Show("Mã giảm giá không tồn tại!");

            Button btnConfirm = new Button { Text = "XÁC NHẬN ĐẶT", Location = new Point(20, 215), Size = new Size(270, 50) };
            UIHelper.StyleButton(btnConfirm, true);
            btnConfirm.Click += BtnConfirm_Click;

            pnlRight.Controls.AddRange(new Control[] { lblPayTitle, lblTotalPrice, lblVoucher, txtVoucher, btnApply, btnConfirm });

            // Add cột vào grid
            grid.Controls.Add(pnlLeft, 0, 0);
            grid.Controls.Add(pnlRight, 1, 0);

            // Thứ tự Add quan trọng: Header trước, Grid sau
            pnlContainer.Controls.Add(lblTitle);
            pnlContainer.Controls.Add(grid);

            // Vì Grid Dock Fill nên phải Bring Header to Front để chắc chắn nó ở trên cùng
            lblTitle.BringToFront();

            this.Controls.Add(pnlContainer);
        }

        // Logic tính tiền (Giữ nguyên)
        private void Time_Changed(object sender, EventArgs e)
        {
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            if (dtpEnd.Value <= dtpStart.Value)
            {
                lblDuration.Text = "(Lỗi thời gian)";
                lblDuration.ForeColor = Color.Red;
                lblTotalPrice.Text = "---";
                currentTotal = 0;
                return;
            }

            TimeSpan span = dtpEnd.Value - dtpStart.Value;
            double hours = Math.Round(span.TotalHours, 1);

            if (hours < 0.5)
            {
                lblDuration.Text = "(Tối thiểu 30p)";
                lblDuration.ForeColor = Color.Red;
                currentTotal = 0;
            }
            else
            {
                lblDuration.Text = $"({hours} giờ)";
                lblDuration.ForeColor = Color.Blue;
                currentTotal = (decimal)hours * _sanInfo.GiaTien;
            }

            lblTotalPrice.Text = currentTotal.ToString("N0") + " VNĐ";
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (currentTotal <= 0) { MessageBox.Show("Vui lòng chọn thời gian hợp lệ!"); return; }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                try
                {
                    // Kiểm tra xem khung giờ này sân có bị trùng không (Logic quan trọng)
                    // ... (Đoạn này ta làm đơn giản trước, sau này thêm Check trùng sau)

                    string maPhieu = "PD" + DateTime.Now.ToString("ddHHmmss");
                    string maSanThuc = _sanInfo.TenSan.Split('-')[0].Trim();

                    string sql = @"INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
                                   VALUES (@Ma, @KH, @San, @Start, @End, N'Chưa thanh toán', 'Online')";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Ma", maPhieu);
                    cmd.Parameters.AddWithValue("@KH", SessionData.CurrentUserID);
                    cmd.Parameters.AddWithValue("@San", maSanThuc);
                    cmd.Parameters.AddWithValue("@Start", dtpStart.Value);
                    cmd.Parameters.AddWithValue("@End", dtpEnd.Value);

                    cmd.ExecuteNonQuery();

                    // Chuyển sang trang thanh toán QR
                    _mainForm.LoadView(new UC_Payment(_mainForm, maPhieu, currentTotal));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
    }
}