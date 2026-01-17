using System;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class CashierForm : Form
    {
        private Panel pnlContent;

        public CashierForm()
        {
            this.Text = "Hệ thống ViệtSport - Thu Ngân";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            LoadView(new UC_Cashier_Payment()); // Vào thẳng trang thanh toán
        }

        private void InitializeComponent()
        {
            // Header
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke };

            // 1. Logo (Bên trái)
            Label lblLogo = new Label { Text = "HỆ THỐNG VIỆTSPORT", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };

            // 2. Tên User (Cách phải 500)
            Label lblUser = new Label
            {
                Text = "Thu ngân: " + SessionData.CurrentUserFullName,
                AutoSize = true,
                Location = new Point(this.Width - 500, 20), // Dời sang trái nhiều hơn
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // 3. Nút Xin nghỉ phép (Cách phải 260)
            Button btnLeave = new Button { Text = "Xin nghỉ phép", Location = new Point(this.Width - 260, 15), Size = new Size(120, 35) };
            UIHelper.StyleButton(btnLeave, false); // Style màu xám
            btnLeave.Click += (s, e) => LoadView(new UC_LeaveRequest());

            // 4. Nút Đăng xuất (Cách phải 130 - Ngoài cùng)
            Button btnLogout = new Button { Text = "Đăng xuất", Location = new Point(this.Width - 130, 15), Size = new Size(100, 35) };
            UIHelper.StyleButton(btnLogout, false);
            btnLogout.Click += (s, e) => {
                SessionData.Logout();
                Application.Restart();
                Environment.Exit(0);
            };

            pnlHeader.Controls.AddRange(new Control[] { lblLogo, lblUser, btnLeave, btnLogout });

            // Content
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 40) }; // Nền tối

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
        }

        private void LoadView(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }
    }
}