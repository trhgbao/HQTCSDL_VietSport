using System;
using System.Drawing;
using System.Windows.Forms;
using VietSportSystem.View.Staff; // Nếu có dùng các UC chung

namespace VietSportSystem
{
    public class ManagerForm : Form
    {
        private Panel pnlSidebar;
        private Panel pnlContent;
        private Label lblTitle;

        public ManagerForm()
        {
            this.Text = "Hệ thống Quản lý ViệtSport";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            InitializeComponent();

            // Mặc định vào Thống kê
            LoadView(new UC_Manager_Stats());
            if (lblTitle != null) lblTitle.Text = "THỐNG KÊ KINH DOANH";
        }

        private void InitializeComponent()
        {
            // 1. Sidebar (Menu trái)
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = Color.FromArgb(230, 230, 230) };

            // Header Menu
            Panel pnlLogo = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(200, 200, 200) };
            Label lblMenu = new Label { Text = "Menu", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            pnlLogo.Controls.Add(lblMenu);

            // Các nút Menu
            Button btnStats = CreateMenuBtn("Thống kê kinh doanh");
            btnStats.Click += (s, e) => { SetActive(btnStats); lblTitle.Text = "THỐNG KÊ KINH DOANH"; LoadView(new UC_Manager_Stats()); };

            Button btnEmp = CreateMenuBtn("Quản lý nhân viên");
            btnEmp.Click += (s, e) => { SetActive(btnEmp); lblTitle.Text = "QUẢN LÝ NHÂN VIÊN"; LoadView(new UC_Manager_Employee()); };

            Button btnShift = CreateMenuBtn("Quản lý ca trực");
            btnShift.Click += (s, e) => { SetActive(btnShift); lblTitle.Text = "QUẢN LÝ CA TRỰC"; LoadView(new UC_Manager_Shift()); };

            Button btnLeave = CreateMenuBtn("Duyệt đơn nghỉ phép");
            btnLeave.Click += (s, e) => { SetActive(btnLeave); lblTitle.Text = "DUYỆT ĐƠN NGHỈ PHÉP"; LoadView(new UC_Manager_Leave()); };

            // ✅ Nút mới: Quản lý kho
            Button btnInventory = CreateMenuBtn("Quản lý kho dịch vụ");
            btnInventory.Click += (s, e) => { SetActive(btnInventory); lblTitle.Text = "QUẢN LÝ KHO DỊCH VỤ"; LoadView(new UC_Manager_Inventory()); };

            // Các nút Demo
            Button btnLeaveRequest = CreateMenuBtn("Xin nghỉ phép (Cá nhân)");
            btnLeaveRequest.Click += (s, e) => { SetActive(btnLeaveRequest); lblTitle.Text = "ĐƠN XIN NGHỈ PHÉP CÁ NHÂN"; LoadView(new UC_LeaveRequest()); };

            Button btnStatus = CreateMenuBtn("Trạng thái sân (Demo 10)");
            btnStatus.Click += (s, e) => { SetActive(btnStatus); lblTitle.Text = "THEO DÕI TRẠNG THÁI SÂN"; LoadView(new UC_Manager_CourtStatus()); };

            Button btnLogout = CreateMenuBtn("Đăng xuất");
            btnLogout.Click += (s, e) => {
                SessionData.Logout();
                Application.Restart();
                Environment.Exit(0);
            };

            // Add nút vào Sidebar (Dùng FlowLayout cho gọn)
            FlowLayoutPanel flowMenu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            flowMenu.Controls.AddRange(new Control[] { btnEmp, btnShift, btnLeave, btnInventory, btnStats, btnLeaveRequest, btnStatus, btnLogout });

            pnlSidebar.Controls.Add(flowMenu);
            pnlSidebar.Controls.Add(pnlLogo);

            // 2. Content (Bên phải)
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            // Header Content
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke };
            lblTitle = new Label { Text = "THỐNG KÊ KINH DOANH", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(30, 15) };
            pnlHeader.Controls.Add(lblTitle);

            pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            pnlMain.Controls.Add(pnlContent);
            pnlMain.Controls.Add(pnlHeader);

            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlSidebar);
        }

        private Button CreateMenuBtn(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(260, 60); // Tăng size cho đẹp
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(230, 230, 230);
            btn.Font = new Font("Segoe UI", 11);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0);

            // Hover effect
            btn.MouseEnter += (s, e) => { if (btn.BackColor != Color.LightGray) btn.BackColor = Color.FromArgb(240, 240, 240); };
            btn.MouseLeave += (s, e) => { if (btn.BackColor != Color.LightGray) btn.BackColor = Color.FromArgb(230, 230, 230); };

            return btn;
        }

        private void SetActive(Button activeBtn)
        {
            // Reset màu tất cả các nút trong FlowLayout
            if (pnlSidebar.Controls[0] is FlowLayoutPanel flow)
            {
                foreach (Control c in flow.Controls)
                {
                    if (c is Button btn)
                    {
                        btn.BackColor = Color.FromArgb(230, 230, 230);
                        btn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
                    }
                }
            }

            // Set màu nút active
            activeBtn.BackColor = Color.LightGray;
            activeBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private void LoadView(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }
    }
}