using System;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_Payment : UserControl
    {
        private MainForm _mainForm;
        public UC_Payment(MainForm main, string maPhieu, decimal amount)
        {
            _mainForm = main;
            InitializeComponent(maPhieu, amount);
        }

        private void InitializeComponent(string maPhieu, decimal amount)
        {
            this.BackColor = Color.White;

            Label lblTitle = new Label
            {
                Text = "CẢM ƠN BẠN ĐÃ ĐẶT SÂN!",
                Dock = DockStyle.Top,
                Font = UIHelper.HeaderFont,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 80,
                ForeColor = Color.Green
            };

            // Panel chứa QR và Thông tin
            Panel pnlBox = new Panel { Size = new Size(800, 400), BackColor = Color.WhiteSmoke };

            // --- CODE SỬA: CĂN GIỮA MÀN HÌNH ---
            pnlBox.Location = new Point((this.Width - 800) / 2, 100); // Hạ xuống Y=100 cho thoáng
            this.Resize += (s, e) =>
            {
                pnlBox.Left = (this.Width - pnlBox.Width) / 2;
                pnlBox.Top = (this.Height - pnlBox.Height) / 2; // Căn giữa cả chiều dọc luôn cho đẹp
            };
            // -----------------------------------

            // (Phần tạo PictureBox và Label bên dưới giữ nguyên code cũ...)
            PictureBox picQR = new PictureBox { Size = new Size(200, 200), Location = new Point(50, 50), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };

            // Load ảnh QR tĩnh
            string pathQR = Application.StartupPath + "\\Images\\qr_static.png";
            if (System.IO.File.Exists(pathQR)) picQR.Image = Image.FromFile(pathQR);

            Label lblQRNote = new Label { Text = "Quét mã để thanh toán", Location = new Point(80, 260), AutoSize = true };

            Label lblInfo = new Label
            {
                Text = $"MÃ ĐƠN HÀNG: {maPhieu}\n\n" +
                       $"Ngân hàng: MB Bank\n" +
                       $"Số tài khoản: 12345678\n" +
                       $"Chủ tài khoản: NGUYEN VAN A\n" +
                       $"Số tiền: {amount.ToString("N0")} VND\n\n" +
                       $"Nội dung CK: {maPhieu}",
                Location = new Point(300, 50),
                Font = new Font("Segoe UI", 12),
                AutoSize = true
            };

            Button btnDone = new Button { Text = "HOÀN TẤT & VỀ TRANG CHỦ", Location = new Point(300, 250), Size = new Size(250, 50) };
            UIHelper.StyleButton(btnDone, true);
            btnDone.Click += (s, e) => _mainForm.LoadView(new UC_HomeBanner(_mainForm));

            pnlBox.Controls.AddRange(new Control[] { picQR, lblQRNote, lblInfo, btnDone });

            this.Controls.Add(pnlBox);
            this.Controls.Add(lblTitle);
        }
    }
}