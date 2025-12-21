using System;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class TechnicianForm : Form
    {
        private Panel pnlContent;
        private Button btnAll, btnMain;

        public TechnicianForm()
        {
            this.Text = "Hệ thống ViệtSport - Kỹ thuật viên";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            LoadView(new UC_Tech_AllFields()); // Mặc định vào "Tất cả sân"
        }

        private void InitializeComponent()
        {
            // 1. Sidebar (Menu trái màu xám)
            Panel pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Color.FromArgb(220, 220, 220) };

            // Header Menu
            Panel pnlLogo = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(200, 200, 200) };
            Label lblMenu = new Label { Text = "Menu", Font = new Font("Segoe UI", 20, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            pnlLogo.Controls.Add(lblMenu);

            // Buttons
            btnAll = CreateMenuBtn("Tất cả sân");
            btnAll.Click += (s, e) => { SetActive(btnAll); LoadView(new UC_Tech_AllFields()); };

            btnMain = CreateMenuBtn("Cần bảo trì");
            btnMain.Click += (s, e) => { SetActive(btnMain); LoadView(new UC_Tech_Maintenance()); };

            Button btnLogout = CreateMenuBtn("Đăng xuất");
            btnLogout.Click += (s, e) => {
                SessionData.Logout();
                Application.Restart();
                Environment.Exit(0);
            };

            pnlSidebar.Controls.AddRange(new Control[] { pnlLogo, btnAll, btnMain, btnLogout });
            // Sắp xếp lại thứ tự control vì AddRange add ngược
            pnlSidebar.Controls.SetChildIndex(pnlLogo, 3);
            pnlSidebar.Controls.SetChildIndex(btnAll, 2);
            pnlSidebar.Controls.SetChildIndex(btnMain, 1);
            pnlSidebar.Controls.SetChildIndex(btnLogout, 0);


            // 2. Header Top (Màu xanh/xám đậm)
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.DimGray }; // Màu tối như hình
            Label lblTitle = new Label { Text = "HỆ THỐNG VIỆTSPORT", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };

            // Avatar tròn (Giả lập)
            Panel pnlUser = new Panel { Size = new Size(40, 40), Location = new Point(this.Width - 320, 10), BackColor = Color.Gray }; // Trừ đi sidebar width
            pnlUser.Paint += (s, e) => e.Graphics.FillEllipse(Brushes.DarkGray, 0, 0, 40, 40);
            Label lblUser = new Label { Text = SessionData.CurrentUserFullName, ForeColor = Color.White, Location = new Point(this.Width - 270, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, pnlUser, lblUser });

            // 3. Content
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
        }

        private Button CreateMenuBtn(string text)
        {
            Button btn = new Button { Dock = DockStyle.Top, Height = 60, Text = text, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(220, 220, 220), Font = new Font("Segoe UI", 11) };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Gray;
            return btn;
        }

        private void SetActive(Button btn)
        {
            btnAll.BackColor = Color.FromArgb(220, 220, 220);
            btnMain.BackColor = Color.FromArgb(220, 220, 220);
            btn.BackColor = Color.LightGray; // Highlight
        }

        private void LoadView(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }
    }
}