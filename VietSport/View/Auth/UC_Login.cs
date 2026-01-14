using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient; // Quan trọng: Để kết nối CSDL

namespace VietSportSystem
{
    public class UC_Login : UserControl
    {
        private MainForm _mainForm;

        // --- KHAI BÁO BIẾN Ở ĐÂY ĐỂ DÙNG ĐƯỢC TOÀN CLASS ---
        private TextBox txtUser;
        private TextBox txtPass;

        public UC_Login(MainForm main)
        {
            _mainForm = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;

            // Container box ở giữa
            Panel pnlLoginBox = new Panel();
            pnlLoginBox.Size = new Size(500, 350);
            pnlLoginBox.BackColor = Color.FromArgb(230, 230, 230);
            pnlLoginBox.Location = new Point((Screen.PrimaryScreen.Bounds.Width - 500) / 2, 100);
            pnlLoginBox.Anchor = AnchorStyles.None;

            Label lblTitle = new Label { Text = "Đăng nhập", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(190, 30) };

            // Khởi tạo TextBox User (Gán vào biến toàn cục)
            txtUser = new TextBox { Width = 400, Location = new Point(50, 80), Text = "" }; // Để trống cho user nhập
            txtUser.PlaceholderText = "Email/Số điện thoại"; // Chỉ chạy trên .NET Core/.NET 5+, nếu lỗi thì bỏ dòng này
            UIHelper.StyleTextBox(txtUser);

            // Khởi tạo TextBox Pass (Gán vào biến toàn cục)
            txtPass = new TextBox { Width = 400, Location = new Point(50, 130), Text = "", UseSystemPasswordChar = true };
            txtPass.PlaceholderText = "Mật khẩu";
            UIHelper.StyleTextBox(txtPass);

            Button btnLogin = new Button { Text = "ĐĂNG NHẬP", Location = new Point(150, 200) };
            UIHelper.StyleButton(btnLogin, true);

            // Gán sự kiện Click
            btnLogin.Click += BtnLogin_Click;

            Label lblForgot = new Label { Text = "Quên mật khẩu?", AutoSize = true, Location = new Point(350, 170), Cursor = Cursors.Hand };
            lblForgot.Click += (s, e) => _mainForm.LoadView(new UC_ForgotPassword(_mainForm));

            pnlLoginBox.Controls.AddRange(new Control[] { lblTitle, txtUser, txtPass, btnLogin, lblForgot });
            this.Controls.Add(pnlLoginBox);

            this.Resize += (s, e) => {
                pnlLoginBox.Left = (this.Width - pnlLoginBox.Width) / 2;
                pnlLoginBox.Top = (this.Height - pnlLoginBox.Height) / 2;
            };
        }

        // --- HÀM XỬ LÝ ĐĂNG NHẬP ---
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string input = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();

            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // CÂU TRUY VẤN NÂNG CẤP: LEFT JOIN cả 2 bảng Khách & Nhân viên
                    // Để tìm xem tài khoản này thuộc về ai
                    string query = @"
                SELECT 
                    tk.TenDangNhap, 
                    tk.MaKhachHang, 
                    tk.MaNhanVien,
                    kh.HoTen AS TenKhach,
                    nv.HoTen AS TenNhanVien,
                    nv.ChucVu,
                    kh.SoDienThoai AS SDT_Khach,
                    nv.SoDienThoai AS SDT_NhanVien
                FROM TaiKhoan tk
                LEFT JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
                LEFT JOIN NhanVien nv ON tk.MaNhanVien = nv.MaNhanVien
                WHERE 
                    (tk.TenDangNhap = @Input OR kh.SoDienThoai = @Input OR nv.SoDienThoai = @Input OR kh.Email = @Input) 
                    AND tk.MatKhau = @Pass";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Input", input);
                        cmd.Parameters.AddWithValue("@Pass", pass);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 1. Kiểm tra xem đây là NHÂN VIÊN hay KHÁCH HÀNG
                                if (reader["MaNhanVien"] != DBNull.Value)
                                {
                                    // --- ĐÂY LÀ NHÂN VIÊN ---
                                    string chucVu = reader["ChucVu"].ToString();
                                    string hoTen = reader["TenNhanVien"].ToString();
                                    string maNV = reader["MaNhanVien"].ToString();
                                    SessionData.CurrentUserFullName = hoTen;
                                    SessionData.CurrentUserID = maNV;
                                    SessionData.UserRole = chucVu;
                                    SessionData.CurrentUsername = reader["TenDangNhap"].ToString();

                                    if (chucVu == "Quản trị")
                                    {
                                        MessageBox.Show("Xin chào Quản trị viên: " + hoTen);
                                        AdminForm frmAdmin = new AdminForm(); // Sẽ tạo ở Bước 3
                                        _mainForm.Hide();
                                        frmAdmin.FormClosed += (s, args) => _mainForm.Close();
                                        frmAdmin.Show();
                                    }
                                    else if (chucVu == "Quản lý")
                                    {
                                        MessageBox.Show("Xin chào Quản lý: " + hoTen);
                                        ManagerForm frmManager = new ManagerForm();
                                        _mainForm.Hide();
                                        frmManager.FormClosed += (s, args) => _mainForm.Close();
                                        frmManager.Show();
                                    }
                                    else if (chucVu == "Lễ tân")
                                    {
                                        MessageBox.Show("Xin chào Lễ tân: " + hoTen);
                                        ReceptionistForm frmRec = new ReceptionistForm();
                                        _mainForm.Hide();
                                        frmRec.FormClosed += (s, args) => _mainForm.Close();
                                        frmRec.Show();
                                    }
                                    else if (chucVu == "Kỹ thuật")
                                    {
                                        MessageBox.Show("Xin chào Kỹ thuật: " + hoTen);
                                        TechnicianForm frmTech = new TechnicianForm();
                                        _mainForm.Hide();
                                        frmTech.FormClosed += (s, args) => _mainForm.Close();
                                        frmTech.Show();
                                    }
                                    else if (chucVu == "Thu ngân")
                                    {
                                        MessageBox.Show($"Xin chào Thu ngân: {hoTen}");
                                        CashierForm frmCashier = new CashierForm(); // Tạo ở Bước 2
                                        _mainForm.Hide();
                                        frmCashier.FormClosed += (s, args) => _mainForm.Close();
                                        frmCashier.Show();
                                    }
                                    else
                                    {
                                        // Các nhân viên khác (Lễ tân, Kỹ thuật...) 
                                        // Tạm thời báo thông báo vì ta chưa vẽ giao diện cho họ
                                        MessageBox.Show($"Xin chào {chucVu}: {hoTen}.\nHệ thống dành cho bộ phận của bạn đang bảo trì!", "Thông báo");
                                    }
                                }
                                else
                                {
                                    // --- ĐÂY LÀ KHÁCH HÀNG (Logic cũ) ---
                                    SessionData.CurrentUsername = reader["TenDangNhap"].ToString();
                                    SessionData.CurrentUserFullName = reader["TenKhach"].ToString();
                                    SessionData.CurrentUserID = reader["MaKhachHang"].ToString();
                                    SessionData.UserRole = "KhachHang";

                                    MessageBox.Show($"Đăng nhập thành công!\nXin chào: {SessionData.CurrentUserFullName}");

                                    _mainForm.UpdateHeaderState();
                                    _mainForm.LoadView(new UC_FastSearch(_mainForm));
                                }
                            }
                            else
                            {
                                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }
    }
}