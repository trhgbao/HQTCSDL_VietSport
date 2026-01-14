using System;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class ReceptionistForm : Form
    {
        private Panel pnlContent;

        public ReceptionistForm()
        {
            this.Text = "Hệ thống ViệtSport - Dành cho Lễ tân";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();
            // Mặc định vào trang Check-in
            LoadView(new UC_Rec_CheckIn());
        }

        private void InitializeComponent()
        {
            // HEADER
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke };
            Label lblLogo = new Label
            {
                Text = "HỆ THỐNG VIỆTSPORT",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // --- SỬA ĐOẠN HIỂN THỊ TÊN LỄ TÂN (Dùng FlowLayout để căn phải chuẩn) ---
            FlowLayoutPanel pnlUserRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft, // Xếp từ phải qua trái
                Padding = new Padding(0, 15, 20, 0) // Căn lề trên 15, lề phải 20
            };

            Label lblUser = new Label
            {
                // Lấy tên từ Session (Nếu null thì hiện "Lễ tân")
                Text = "Lễ tân: " + (SessionData.CurrentUserFullName ?? "Unknown"),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIHelper.PrimaryColor
            };

            // Nút Đăng xuất
            Button btnLogout = new Button { Text = "Đăng xuất", Size = new Size(100, 35) };
            UIHelper.StyleButton(btnLogout, false);
            btnLogout.Click += (s, e) => {
                SessionData.Logout();
                Application.Restart();
                Environment.Exit(0);
            };

            Button btnLeave = CreateBtn("Xin nghỉ phép", 4);

            // Thêm vào panel phải (Thứ tự: Đăng xuất trước -> Tên sau vì xếp phải qua trái)
            pnlUserRight.Controls.Add(btnLogout);
            pnlUserRight.Controls.Add(lblUser);
            btnLeave.Click += (s, e) => LoadView(new UC_LeaveRequest());
            pnlHeader.Controls.Add(btnLeave);
            // ------------------------------------------------------------------------

            // MENU BUTTONS (Giữa)
            Button btnCheckIn = CreateBtn("Check-in", 1);
            btnCheckIn.Click += (s, e) => LoadView(new UC_Rec_CheckIn());

            Button btnBooking = CreateBtn("Đặt sân trực tiếp", 2);
            btnBooking.Click += (s, e) => LoadView(new UC_Rec_DirectBooking());

            pnlHeader.Controls.AddRange(new Control[] { lblLogo, btnCheckIn, btnBooking, pnlUserRight });

            // CONTENT
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(50, 50, 50) };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
        }

        private Button CreateBtn(string text, int index)
        {
            Button btn = new Button { Text = text, Size = new Size(120, 40) };
            UIHelper.StyleButton(btn, false);
            btn.Location = new Point(500 + (130 * index), 10);
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