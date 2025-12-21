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
            Label lblLogo = new Label { Text = "HỆ THỐNG VIỆTSPORT", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };

            Label lblUser = new Label
            {
                Text = "Thu ngân: " + SessionData.CurrentUserFullName,
                AutoSize = true,
                Location = new Point(this.Width - 300, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            Button btnLogout = new Button { Text = "Đăng xuất", Location = new Point(this.Width - 120, 15) };
            btnLogout.Click += (s, e) => {
                SessionData.Logout();
                Application.Restart();
                Environment.Exit(0);
            };

            pnlHeader.Controls.AddRange(new Control[] { lblLogo, lblUser, btnLogout });

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