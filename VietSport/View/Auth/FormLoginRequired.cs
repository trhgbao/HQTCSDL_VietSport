using System;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public partial class FormLoginRequired : Form
    {
        // Biến để lưu lựa chọn của người dùng
        public string UserChoice { get; private set; } = "Cancel";

        public FormLoginRequired()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Yêu cầu xác thực";
            this.BackColor = Color.White;

            Label lblMsg = new Label
            {
                Text = "Vui lòng đăng nhập hoặc đăng ký thành viên\nđể sử dụng tính năng tìm kiếm và đặt sân!",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 100,
                Font = new Font("Segoe UI", 11)
            };

            Button btnLogin = new Button { Text = "ĐĂNG NHẬP", Location = new Point(40, 120), Size = new Size(140, 50) };
            UIHelper.StyleButton(btnLogin, true); // Màu xanh
            btnLogin.Click += (s, e) => { UserChoice = "Login"; this.Close(); };

            Button btnRegister = new Button { Text = "ĐĂNG KÝ", Location = new Point(200, 120), Size = new Size(140, 50) };
            UIHelper.StyleButton(btnRegister, false); // Màu xám
            btnRegister.Click += (s, e) => { UserChoice = "Register"; this.Close(); };

            this.Controls.AddRange(new Control[] { lblMsg, btnLogin, btnRegister });
        }
    }
}