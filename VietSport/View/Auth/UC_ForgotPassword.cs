using System;
using System.Drawing;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_ForgotPassword : UserControl
    {
        private MainForm _mainForm;

        // Các control
        private TextBox txtInput;
        private Button btnCheck;
        private Panel pnlReset; // Khu vực đổi pass (mặc định ẩn)
        private TextBox txtNewPass, txtConfirmPass;
        private Button btnSave;
        private Button btnBack;

        // Biến lưu Tên đăng nhập tìm được
        private string foundUsername = "";

        public UC_ForgotPassword(MainForm main)
        {
            _mainForm = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;

            // 1. Container chính
            Panel pnlBox = new Panel();
            pnlBox.Size = new Size(500, 450);
            pnlBox.BackColor = Color.WhiteSmoke;

            // --- SỬA LỖI LỆCH ---
            // Không dùng AnchorTop nữa, mà dùng sự kiện Resize để luôn căn giữa
            pnlBox.Location = new Point((this.Width - 500) / 2, 80);
            this.Resize += (s, e) => {
                pnlBox.Left = (this.Width - pnlBox.Width) / 2; // Luôn căn giữa theo chiều ngang
            };
            // --------------------

            // Tiêu đề
            Label lblTitle = new Label
            {
                Text = "KHÔI PHỤC MẬT KHẨU",
                Font = UIHelper.HeaderFont,
                AutoSize = true,
                Location = new Point(130, 30),
                ForeColor = UIHelper.PrimaryColor
            };

            // --- PHẦN 1: XÁC MINH ---
            Label lblMsg = new Label { Text = "Vui lòng nhập Số điện thoại hoặc Email đã đăng ký:", Location = new Point(50, 80), AutoSize = true };

            txtInput = new TextBox { Location = new Point(50, 110), Width = 400 };
            UIHelper.StyleTextBox(txtInput);
            txtInput.PlaceholderText = "Ví dụ: 0909123456";

            btnCheck = new Button { Text = "XÁC MINH", Location = new Point(50, 160), Size = new Size(150, 40) };
            UIHelper.StyleButton(btnCheck, true);
            btnCheck.Click += BtnCheck_Click;

            // Nút quay lại
            btnBack = new Button { Text = "Quay lại", Location = new Point(220, 160), Size = new Size(100, 40) };
            UIHelper.StyleButton(btnBack, false);
            btnBack.Click += (s, e) => _mainForm.LoadView(new UC_Login(_mainForm));

            // --- PHẦN 2: ĐỔI MẬT KHẨU (Mặc định ẨN) ---
            pnlReset = new Panel { Location = new Point(0, 220), Size = new Size(500, 200), Visible = false };

            Label lblNew = new Label { Text = "Mật khẩu mới:", Location = new Point(50, 0), AutoSize = true };
            txtNewPass = new TextBox { Location = new Point(50, 25), Width = 400, UseSystemPasswordChar = true };
            UIHelper.StyleTextBox(txtNewPass);

            Label lblConfirm = new Label { Text = "Nhập lại mật khẩu:", Location = new Point(50, 70), AutoSize = true };
            txtConfirmPass = new TextBox { Location = new Point(50, 95), Width = 400, UseSystemPasswordChar = true };
            UIHelper.StyleTextBox(txtConfirmPass);

            btnSave = new Button { Text = "LƯU MẬT KHẨU", Location = new Point(50, 140), Size = new Size(400, 45) };
            UIHelper.StyleButton(btnSave, true);
            btnSave.BackColor = Color.SeaGreen;
            btnSave.Click += BtnSave_Click;

            pnlReset.Controls.AddRange(new Control[] { lblNew, txtNewPass, lblConfirm, txtConfirmPass, btnSave });

            // Add vào box
            pnlBox.Controls.AddRange(new Control[] { lblTitle, lblMsg, txtInput, btnCheck, btnBack, pnlReset });
            this.Controls.Add(pnlBox);
        }

        // --- LOGIC 1: KIỂM TRA TỒN TẠI ---
        private void BtnCheck_Click(object sender, EventArgs e)
        {
            string input = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) { MessageBox.Show("Vui lòng nhập thông tin!"); return; }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    // Tìm xem SĐT/Email này thuộc về User nào
                    string query = @"
                        SELECT tk.TenDangNhap 
                        FROM KhachHang kh
                        JOIN TaiKhoan tk ON kh.MaKhachHang = tk.MaKhachHang
                        WHERE kh.SoDienThoai = @Input OR kh.Email = @Input";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Input", input);

                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        foundUsername = result.ToString();
                        MessageBox.Show("Xác minh thành công!\nVui lòng nhập mật khẩu mới.", "Thông báo");

                        // Khóa phần trên, mở phần dưới
                        txtInput.Enabled = false;
                        btnCheck.Enabled = false;
                        pnlReset.Visible = true; // Hiện form đổi pass
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy tài khoản nào với thông tin này!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối: " + ex.Message);
                }
            }
        }

        // --- LOGIC 2: LƯU MẬT KHẨU MỚI ---
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (txtNewPass.Text.Length < 6) { MessageBox.Show("Mật khẩu phải từ 6 ký tự!"); return; }
            if (txtNewPass.Text != txtConfirmPass.Text) { MessageBox.Show("Mật khẩu xác nhận không khớp!"); return; }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    // Cập nhật mật khẩu cho Username đã tìm thấy ở bước 1
                    string query = "UPDATE TaiKhoan SET MatKhau = @Pass WHERE TenDangNhap = @User";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Pass", txtNewPass.Text); // Nhớ mã hóa MD5 nếu cần
                    cmd.Parameters.AddWithValue("@User", foundUsername);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Đổi mật khẩu thành công! Hãy đăng nhập lại.", "Hoàn tất");
                        _mainForm.LoadView(new UC_Login(_mainForm)); // Quay về màn hình Login
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi cập nhật: " + ex.Message);
                }
            }
        }
    }
}