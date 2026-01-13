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
        private CheckBox chkDemoFix;
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

            chkDemoFix = new CheckBox
            {
                Text = "Demo: Bật chế độ Fix Lỗi (Serializable)",
                Location = new Point(20, 170),
                AutoSize = true,
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            Button btnConfirm = new Button { Text = "XÁC NHẬN ĐẶT", Location = new Point(20, 215), Size = new Size(270, 50) };
            UIHelper.StyleButton(btnConfirm, true);
            btnConfirm.Click += BtnConfirm_Click;

            pnlRight.Controls.AddRange(new Control[] { lblPayTitle, lblTotalPrice, lblVoucher, txtVoucher, btnApply, chkDemoFix, btnConfirm });

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

            // Lấy MaSan (xử lý logic cắt chuỗi như cũ của bạn)
            // Lưu ý: Đảm bảo MaSan đúng định dạng trong DB (ví dụ 'S005')
            string maSanThuc = _sanInfo.TenSan.Split('-')[0].Trim();
            string maKH = SessionData.CurrentUserID;

            // Kiểm tra xem người dùng có tick vào chế độ Fix lỗi không
            bool dungCheDoFix = chkDemoFix.Checked;

            // Thông báo bắt đầu Demo
            MessageBox.Show("Hệ thống đang xử lý đặt sân...\n(Sẽ giả lập độ trễ 10s để bạn kịp thao tác máy khác)", "Thông báo Demo");

            // GỌI HÀM TRANSACTION TỪ DATABASE HELPER
            // Hàm này sẽ gọi xuống Stored Procedure có WAITFOR DELAY
            string ketQua = DatabaseHelper.DatSan_Transaction_T1(maKH, maSanThuc, dtpStart.Value, dtpEnd.Value, dungCheDoFix);

            // Kiểm tra kết quả trả về từ SQL
            if (ketQua.Contains("thành công") || ketQua.Contains("Success"))
            {
                MessageBox.Show(ketQua, "Thành công");

                // Chỉ khi thành công mới chuyển sang trang thanh toán
                // Tạo mã phiếu tạm (hoặc lấy từ DB nếu procedure trả về, ở đây mình giả lập lại để hiện QR)
                string maPhieuDisplay = "PD" + DateTime.Now.ToString("ddHHmmss");
                _mainForm.LoadView(new UC_Payment(_mainForm, maPhieuDisplay, currentTotal));
            }
            else
            {
                // Trường hợp lỗi (Sân trùng, hoặc lỗi Deadlock/Update conflict)
                MessageBox.Show(ketQua, "Thất bại - Xung đột xảy ra", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}