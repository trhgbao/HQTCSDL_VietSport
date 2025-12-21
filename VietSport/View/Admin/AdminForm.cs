using System;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class AdminForm : Form
    {
        private Panel pnlContent;

        public AdminForm()
        {
            this.Text = "Hệ thống ViệtSport - Quản trị viên";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            LoadView(new UC_Admin_Employee()); // Mặc định vào Quản lý nhân viên
        }

        private void InitializeComponent()
        {
            // SIDEBAR
            Panel pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Color.LightGray };
            Label lblMenu = new Label { Text = "Menu", Font = new Font("Segoe UI", 20, FontStyle.Bold), Dock = DockStyle.Top, Height = 80, TextAlign = ContentAlignment.MiddleCenter };

            Button btnEmp = CreateMenuBtn("Quản lý nhân viên");
            btnEmp.Click += (s, e) => LoadView(new UC_Admin_Employee());

            Label lblParam = new Label { Text = "Tham số quy định", Font = new Font("Segoe UI", 12, FontStyle.Underline), Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(10, 0, 0, 0) };

            Button btnPrice = CreateMenuBtn("   > Bảng giá"); // Thụt đầu dòng giả lập sub-menu
            btnPrice.Click += (s, e) => LoadView(new UC_Admin_PriceList());

            Button btnLimit = CreateMenuBtn("   > Thời gian/Hạn mức");
            btnLimit.Click += (s, e) => LoadView(new UC_Admin_Settings());

            Button btnLogout = CreateMenuBtn("Đăng xuất");
            btnLogout.Click += (s, e) => {
                SessionData.Logout();
                Application.Restart();
                Environment.Exit(0);
            };

            pnlSidebar.Controls.AddRange(new Control[] { btnLimit, btnPrice, lblParam, btnEmp, lblMenu, btnLogout });

            // HEADER
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke };
            Label lblTitle = new Label { Text = "HỆ THỐNG VIỆTSPORT", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };

            // Avatar Admin
            Panel pnlAvatar = new Panel { Size = new Size(40, 40), BackColor = Color.Gray, Location = new Point(this.Width - 100, 10) };
            pnlAvatar.Paint += (s, e) => e.Graphics.FillEllipse(Brushes.Gray, 0, 0, 40, 40); // Vẽ tròn
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, pnlAvatar });

            // CONTENT
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
        }

        private Button CreateMenuBtn(string text)
        {
            Button btn = new Button { Dock = DockStyle.Top, Height = 50, Text = text, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0), BackColor = Color.LightGray };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadView(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }
    }
}