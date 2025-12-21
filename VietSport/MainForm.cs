using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class MainForm : Form
    {
        private Panel pnlHeader;
        private Panel pnlFooter;
        private Panel pnlContent;

        // Container chứa menu bên phải (FlowLayout)
        private FlowLayoutPanel pnlNavContainer;

        // Các control điều hướng
        private Button btnLogin;
        private Button btnRegister;
        private Button btnBooking;
        private Button btnHome;

        // User Avatar
        private Panel pnlUserBadge;
        private PictureBox picAvatar;
        private Label lblUserName;
        private ContextMenuStrip menuUser;

        public MainForm()
        {
            this.Text = "Hệ thống ViệtSport - Đặt sân & Quản lý";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            InitializeComponent();

            // Mặc định load trang tìm kiếm
            LoadView(new UC_FastSearch(this));

            // Cập nhật trạng thái hiển thị nút
            UpdateHeaderState();
        }

        private void InitializeComponent()
        {
            // --- 1. HEADER ---
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.WhiteSmoke };

            // Logo bên trái
            Label lblLogo = new Label
            {
                Text = "VIETSPORT SYSTEM",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = UIHelper.PrimaryColor,
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Container chứa menu (Quan trọng: Dock sang phải để luôn dính lề phải)
            pnlNavContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft, // Xếp từ phải sang trái
                Padding = new Padding(10, 15, 10, 0), // Căn lề trên xuống một chút cho đẹp
                WrapContents = false
            };

            // --- 2. CÁC NÚT CHỨC NĂNG ---
            // Tạo nút (Không cần tính tọa độ nữa)
            btnLogin = CreateNavButton("Đăng nhập", true); // True là nút màu xanh
            btnLogin.Click += (s, e) => LoadView(new UC_Login(this));

            btnRegister = CreateNavButton("Đăng ký", false);
            btnRegister.Click += (s, e) => LoadView(new UC_Register(this));

            btnBooking = CreateNavButton("Đặt sân", false);
            btnBooking.Click += (s, e) => LoadView(new UC_FastSearch(this));

            btnHome = CreateNavButton("Trang chủ", false);
            btnHome.Click += (s, e) => LoadView(new UC_HomeBanner(this));

            // --- 3. PHẦN USER AVATAR (Mặc định ẩn) ---
            pnlUserBadge = new Panel
            {
                Size = new Size(180, 45),
                Cursor = Cursors.Hand,
                Visible = false // Mặc định ẩn
            };

            picAvatar = new PictureBox { Size = new Size(40, 40), Location = new Point(0, 2), BackColor = Color.LightGray };
            picAvatar.Paint += PicAvatar_Paint; // Vẽ tròn

            lblUserName = new Label
            {
                Text = "Khách hàng",
                Location = new Point(45, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UIHelper.SecondaryColor
            };

            // Menu sổ xuống
            menuUser = new ContextMenuStrip();
            menuUser.Items.Add("Thông tin tài khoản");
            menuUser.Items.Add("Lịch sử đặt sân", null, (s, e) => {
                if (SessionData.IsLoggedIn())
                    LoadView(new UC_BookingHistory(this));
                else
                    MessageBox.Show("Vui lòng đăng nhập!");
            });
            menuUser.Items.Add(new ToolStripSeparator());
            menuUser.Items.Add("Đăng xuất", null, (s, e) => {
                SessionData.Logout();
                UpdateHeaderState();
                LoadView(new UC_FastSearch(this));
                MessageBox.Show("Đã đăng xuất thành công!");
            });

            // Gán sự kiện click cho Avatar
            pnlUserBadge.Click += (s, e) => menuUser.Show(pnlUserBadge, 0, pnlUserBadge.Height);
            lblUserName.Click += (s, e) => menuUser.Show(pnlUserBadge, 0, pnlUserBadge.Height);
            picAvatar.Click += (s, e) => menuUser.Show(pnlUserBadge, 0, pnlUserBadge.Height);

            pnlUserBadge.Controls.Add(picAvatar);
            pnlUserBadge.Controls.Add(lblUserName);


            // --- 4. ADD VÀO CONTAINER (Thứ tự thêm vào sẽ hiển thị từ Phải -> Trái) ---
            // Ta muốn: [Trang chủ] [Đặt sân] [Đăng ký] [Đăng nhập/Avatar] | (Cạnh phải màn hình)

            // Vì FlowDirection = RightToLeft, cái nào Add trước sẽ nằm ngoài cùng bên phải
            pnlNavContainer.Controls.Add(pnlUserBadge); // Vị trí 1 (Nếu hiện)
            pnlNavContainer.Controls.Add(btnLogin);     // Vị trí 1 (Nếu UserBadge ẩn)
            pnlNavContainer.Controls.Add(btnRegister);  // Vị trí 2
            pnlNavContainer.Controls.Add(btnBooking);   // Vị trí 3
            pnlNavContainer.Controls.Add(btnHome);      // Vị trí 4

            pnlHeader.Controls.Add(pnlNavContainer);
            pnlHeader.Controls.Add(lblLogo);

            // --- 5. FOOTER & CONTENT ---
            pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = UIHelper.SecondaryColor };
            Label lblFooter = new Label { Text = "© 2025 VIETSPORT SYSTEM", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White };
            pnlFooter.Controls.Add(lblFooter);

            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
        }

        // Helper tạo nút nhanh
        private Button CreateNavButton(string text, bool isPrimary)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(110, 40);
            btn.Margin = new Padding(5, 0, 5, 0); // Khoảng cách giữa các nút

            // Style
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            if (isPrimary)
            {
                btn.BackColor = UIHelper.PrimaryColor;
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.BackColor = Color.WhiteSmoke;
                btn.ForeColor = Color.DimGray;
                btn.FlatAppearance.BorderColor = Color.Silver;
                btn.FlatAppearance.BorderSize = 1;
            }

            return btn;
        }

        public void LoadView(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }

        // Logic ẩn hiện nút
        public void UpdateHeaderState()
        {
            if (SessionData.IsLoggedIn())
            {
                // Đã đăng nhập: Ẩn nút Login/Reg, Hiện Avatar
                btnLogin.Visible = false;
                btnRegister.Visible = false;
                pnlUserBadge.Visible = true;

                lblUserName.Text = SessionData.CurrentUserFullName;
                // Nếu tên quá dài thì cắt bớt
                if (lblUserName.Text.Length > 15) lblUserName.Text = lblUserName.Text.Substring(0, 12) + "...";
            }
            else
            {
                // Chưa đăng nhập: Hiện nút Login/Reg, Ẩn Avatar
                btnLogin.Visible = true;
                btnRegister.Visible = true;
                pnlUserBadge.Visible = false;
            }
        }

        private void PicAvatar_Paint(object sender, PaintEventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, pic.Width, pic.Height);
            pic.Region = new Region(path);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(UIHelper.PrimaryColor, 2))
            {
                e.Graphics.DrawEllipse(pen, 1, 1, pic.Width - 2, pic.Height - 2);
            }
        }
    }
}