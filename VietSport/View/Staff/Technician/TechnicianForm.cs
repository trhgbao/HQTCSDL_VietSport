using System;
using System.Drawing;
using System.Windows.Forms;
using VietSportSystem.View.Staff; // Namespace nếu bạn đã chia thư mục

namespace VietSportSystem
{
    public class TechnicianForm : Form
    {
        // --- KHAI BÁO BIẾN TOÀN CỤC ---
        private Panel pnlContent;
        private Button btnAll, btnMain, btnLeave;
        private Label lblTitle;
        // ------------------------------

        public TechnicianForm()
        {
            this.Text = "Hệ thống ViệtSport - Kỹ thuật viên";
            this.Size = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent(); // Chạy hàm tạo giao diện

            // Lúc này pnlContent và lblTitle đã được khởi tạo (không bị null)
            if (lblTitle != null) lblTitle.Text = "TẤT CẢ SÂN BÃI";
            LoadView(new UC_Tech_AllFields());
        }

        private void InitializeComponent()
        {
            // 1. Sidebar
            Panel pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Color.FromArgb(220, 220, 220) };

            Panel pnlLogo = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(200, 200, 200) };
            Label lblMenu = new Label { Text = "Menu", Font = new Font("Segoe UI", 20, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            pnlLogo.Controls.Add(lblMenu);

            btnAll = CreateMenuBtn("Tất cả sân");
            btnAll.Click += (s, e) => {
                SetActive(btnAll);
                if (lblTitle != null) lblTitle.Text = "TẤT CẢ SÂN BÃI";
                LoadView(new UC_Tech_AllFields());
            };

            btnMain = CreateMenuBtn("Cần bảo trì");
            btnMain.Click += (s, e) => {
                SetActive(btnMain);
                if (lblTitle != null) lblTitle.Text = "BẢO TRÌ SÂN";
                LoadView(new UC_Tech_Maintenance());
            };

            btnLeave = CreateMenuBtn("Xin nghỉ phép");
            btnLeave.Click += (s, e) => {
                SetActive(btnLeave);
                if (lblTitle != null) lblTitle.Text = "XIN NGHỈ PHÉP";
                LoadView(new UC_LeaveRequest());
            };

            Button btnLogout = CreateMenuBtn("Đăng xuất");
            btnLogout.Click += (s, e) => {
                SessionData.Logout();
                Application.Restart();
                Environment.Exit(0);
            };

            pnlSidebar.Controls.AddRange(new Control[] { pnlLogo, btnAll, btnMain, btnLeave, btnLogout });

            pnlSidebar.Controls.SetChildIndex(pnlLogo, 4);
            pnlSidebar.Controls.SetChildIndex(btnAll, 3);
            pnlSidebar.Controls.SetChildIndex(btnMain, 2);
            pnlSidebar.Controls.SetChildIndex(btnLeave, 1);
            pnlSidebar.Controls.SetChildIndex(btnLogout, 0);

            // 2. Header Top
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.DimGray };

            // --- QUAN TRỌNG: Không được có chữ 'Label' ở đầu dòng này ---
            lblTitle = new Label { Text = "HỆ THỐNG VIỆTSPORT", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            // -----------------------------------------------------------

            Panel pnlUser = new Panel { Size = new Size(40, 40), Location = new Point(this.Width - 320, 10), BackColor = Color.Gray };
            pnlUser.Paint += (s, e) => e.Graphics.FillEllipse(Brushes.DarkGray, 0, 0, 40, 40);

            Label lblUser = new Label { Text = SessionData.CurrentUserFullName, ForeColor = Color.White, Location = new Point(this.Width - 270, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, pnlUser, lblUser });

            // 3. Content
            // --- QUAN TRỌNG: Không được có chữ 'Panel' ở đầu dòng này ---
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            // -----------------------------------------------------------

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
        }

        private Button CreateMenuBtn(string text)
        {
            Button btn = new Button { Dock = DockStyle.Top, Height = 60, Text = text, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(220, 220, 220), Font = new Font("Segoe UI", 11) };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Gray;
            btn.MouseEnter += (s, e) => { if (btn.BackColor != Color.LightGray) btn.BackColor = Color.FromArgb(240, 240, 240); };
            btn.MouseLeave += (s, e) => { if (btn.BackColor != Color.LightGray) btn.BackColor = Color.FromArgb(220, 220, 220); };
            return btn;
        }

        private void SetActive(Button btn)
        {
            btnAll.BackColor = Color.FromArgb(220, 220, 220);
            btnMain.BackColor = Color.FromArgb(220, 220, 220);
            btnLeave.BackColor = Color.FromArgb(220, 220, 220);
            btn.BackColor = Color.LightGray;
        }

        private void LoadView(UserControl uc)
        {
            // Nếu pnlContent bị null (do lỗi Shadowing), dòng này sẽ gây Crash
            if (pnlContent != null)
            {
                pnlContent.Controls.Clear();
                uc.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(uc);
            }
            else
            {
                MessageBox.Show("Lỗi: pnlContent chưa được khởi tạo! (Vui lòng kiểm tra code InitializeComponent)");
            }
        }
    }
}