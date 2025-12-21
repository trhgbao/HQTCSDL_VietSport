using System;
using System.Drawing;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace VietSportSystem
{
    public class UC_Register : UserControl
    {
        private MainForm _mainForm;
        private ErrorProvider errorProvider;

        // Các controls nhập liệu
        private TextBox txtHoTen, txtSDT, txtCMND, txtEmail, txtDiaChi, txtMaNV, txtMatKhau, txtMatKhau2;
        private DateTimePicker dtpNgaySinh;
        private ComboBox cboGioiTinh;

        public UC_Register(MainForm main)
        {
            _mainForm = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.WhiteSmoke; // Nền xám tổng thể
            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // 1. Container chính (Khung xám đậm hơn ở giữa)
            Panel pnlContainer = new Panel
            {
                Size = new Size(700, 600),
                BackColor = Color.FromArgb(220, 220, 220), // Màu xám như hình
                Location = new Point((Screen.PrimaryScreen.Bounds.Width - 700) / 2, 20),
                Anchor = AnchorStyles.None
            };
            // Căn giữa panel
            this.Resize += (s, e) => {
                pnlContainer.Left = (this.Width - pnlContainer.Width) / 2;
                pnlContainer.Top = (this.Height - pnlContainer.Height) / 2;
            };

            // 2. Tiêu đề
            Label lblTitle = new Label
            {
                Text = "ĐĂNG KÍ THÀNH VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(220, 20)
            };
            pnlContainer.Controls.Add(lblTitle);

            // 3. Các dòng nhập liệu (Dùng Helper cho nhanh)
            int startY = 70;
            int gap = 50; // Khoảng cách giữa các dòng

            // Dòng 1: Họ và tên
            txtHoTen = CreateTextBox(pnlContainer, "Họ và tên", 20, startY, 660);

            // Dòng 2: Ngày sinh - SĐT - Giới tính (3 cột)
            // Ngày sinh
            dtpNgaySinh = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(20, startY + gap), Width = 200, Font = UIHelper.MainFont, Height = 35 };
            pnlContainer.Controls.Add(dtpNgaySinh);
            CreatePlaceholderLabel(pnlContainer, "Ngày sinh", 20, startY + gap - 20); // Label giả

            // SĐT
            txtSDT = CreateTextBox(pnlContainer, "Số điện thoại", 240, startY + gap, 200);

            // Giới tính
            cboGioiTinh = new ComboBox { Location = new Point(460, startY + gap), Width = 220, Font = UIHelper.MainFont, DropDownStyle = ComboBoxStyle.DropDownList, Height = 35 };
            cboGioiTinh.Items.AddRange(new string[] { "Nam", "Nữ", "Khác" });
            cboGioiTinh.SelectedIndex = 0;
            pnlContainer.Controls.Add(cboGioiTinh);
            CreatePlaceholderLabel(pnlContainer, "Giới tính", 460, startY + gap - 20);

            // Dòng 3: CMND
            txtCMND = CreateTextBox(pnlContainer, "Số CMND/CCCD", 20, startY + gap * 2, 660);

            // Dòng 4: Email
            txtEmail = CreateTextBox(pnlContainer, "Email", 20, startY + gap * 3, 660);

            // Dòng 5: Địa chỉ
            txtDiaChi = CreateTextBox(pnlContainer, "Địa chỉ", 20, startY + gap * 4, 660);

            // Dòng 6: Mã nhân viên (Optional)
            txtMaNV = CreateTextBox(pnlContainer, "Mã nhân viên (nếu là nhân viên, lấy mã từ quản lý)", 20, startY + gap * 5, 660);

            // Dòng 7: Mật khẩu
            txtMatKhau = CreateTextBox(pnlContainer, "Mật khẩu", 20, startY + gap * 6, 660, true);

            // Dòng 8: Nhập lại mật khẩu
            txtMatKhau2 = CreateTextBox(pnlContainer, "Nhập lại mật khẩu", 20, startY + gap * 7, 660, true);

            // Nút Đăng ký
            Button btnReg = new Button { Text = "ĐĂNG KÍ", Location = new Point(250, startY + gap * 9), Size = new Size(200, 50) };
            UIHelper.StyleButton(btnReg, true);
            btnReg.BackColor = Color.Gray; // Theo hình mẫu màu xám đậm
            btnReg.Click += BtnReg_Click;
            pnlContainer.Controls.Add(btnReg);

            // Nút quay lại nhỏ xíu
            Button btnBack = new Button { Text = "Thoát", Location = new Point(20, 20), Size = new Size(60, 30) };
            btnBack.Click += (s, e) => _mainForm.LoadView(new UC_FastSearch(_mainForm));
            pnlContainer.Controls.Add(btnBack);

            this.Controls.Add(pnlContainer);
        }

        // --- LOGIC XỬ LÝ ĐĂNG KÝ ---
        private void BtnReg_Click(object sender, EventArgs e)
        {
            // 1. Validate Form (Kiểm tra dữ liệu nhập cơ bản)
            if (!ValidateForm()) return;

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction(); // Bắt đầu giao dịch

                try
                {
                    // Lấy mã nhân viên (nếu có)
                    string maNV = txtMaNV.Text.Trim();

                    // --- TRƯỜNG HỢP 1: ĐĂNG KÝ CHO NHÂN VIÊN (QUẢN LÝ, LỄ TÂN...) ---
                    if (!string.IsNullOrEmpty(maNV))
                    {
                        // A. Kiểm tra xem Mã NV và SĐT có khớp trong hồ sơ nhân sự không?
                        string sqlCheckNV = "SELECT COUNT(*) FROM NhanVien WHERE MaNhanVien = @Ma AND SoDienThoai = @SDT";
                        SqlCommand cmdCheck = new SqlCommand(sqlCheckNV, conn, trans);
                        cmdCheck.Parameters.AddWithValue("@Ma", maNV);
                        cmdCheck.Parameters.AddWithValue("@SDT", txtSDT.Text);

                        int count = (int)cmdCheck.ExecuteScalar();
                        if (count == 0)
                        {
                            MessageBox.Show("Mã nhân viên không tồn tại hoặc Số điện thoại không khớp hồ sơ!", "Lỗi xác thực", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            trans.Rollback();
                            return;
                        }

                        // B. Kiểm tra xem nhân viên này đã có tài khoản chưa
                        string sqlCheckAcc = "SELECT COUNT(*) FROM TaiKhoan WHERE MaNhanVien = @Ma";
                        SqlCommand cmdAcc = new SqlCommand(sqlCheckAcc, conn, trans);
                        cmdAcc.Parameters.AddWithValue("@Ma", maNV);

                        if ((int)cmdAcc.ExecuteScalar() > 0)
                        {
                            MessageBox.Show("Nhân viên này đã có tài khoản rồi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            trans.Rollback();
                            return;
                        }

                        // C. Tạo tài khoản cho Nhân viên (Lấy SĐT làm User)
                        string sqlInsertTK = "INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaNhanVien) VALUES (@User, @Pass, @MaNV)";
                        SqlCommand cmdTK = new SqlCommand(sqlInsertTK, conn, trans);
                        cmdTK.Parameters.AddWithValue("@User", txtSDT.Text);
                        cmdTK.Parameters.AddWithValue("@Pass", txtMatKhau.Text);
                        cmdTK.Parameters.AddWithValue("@MaNV", maNV);
                        cmdTK.ExecuteNonQuery();

                        // Set thông tin để tự động đăng nhập sau này
                        SessionData.CurrentUserID = maNV;
                    }
                    // --- TRƯỜNG HỢP 2: ĐĂNG KÝ KHÁCH HÀNG (MẶC ĐỊNH) ---
                    else
                    {
                        // A. Check trùng lặp (SĐT, CMND, Email)
                        // Lưu ý: Phải truyền 'trans' vào để check trong cùng giao dịch
                        if (CheckExistTransaction(conn, trans, "KhachHang", "SoDienThoai", txtSDT.Text))
                        {
                            MessageBox.Show("Số điện thoại này đã được đăng ký!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            trans.Rollback(); return;
                        }
                        if (CheckExistTransaction(conn, trans, "KhachHang", "CMND", txtCMND.Text))
                        {
                            MessageBox.Show("Số CMND/CCCD này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            trans.Rollback(); return;
                        }
                        if (!string.IsNullOrEmpty(txtEmail.Text) && CheckExistTransaction(conn, trans, "KhachHang", "Email", txtEmail.Text))
                        {
                            MessageBox.Show("Email này đã được sử dụng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            trans.Rollback(); return;
                        }

                        // B. Insert KhachHang
                        string newMaKH = "KH" + DateTime.Now.ToString("ddHHmmss"); // Tạo mã tự động
                        string sqlKH = @"
                    INSERT INTO KhachHang 
                    (MaKhachHang, HoTen, NgaySinh, SoDienThoai, Email, CMND, GioiTinh, DiaChi, LaHSSV, LaNguoiCaoTuoi)
                    VALUES 
                    (@Ma, @Ten, @NgaySinh, @SDT, @Email, @CMND, @GioiTinh, @DiaChi, 0, 0)";

                        SqlCommand cmdKH = new SqlCommand(sqlKH, conn, trans);
                        cmdKH.Parameters.AddWithValue("@Ma", newMaKH);
                        cmdKH.Parameters.AddWithValue("@Ten", txtHoTen.Text);
                        cmdKH.Parameters.AddWithValue("@NgaySinh", dtpNgaySinh.Value);
                        cmdKH.Parameters.AddWithValue("@SDT", txtSDT.Text);
                        cmdKH.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);
                        cmdKH.Parameters.AddWithValue("@CMND", txtCMND.Text);
                        cmdKH.Parameters.AddWithValue("@GioiTinh", cboGioiTinh.SelectedItem.ToString());
                        cmdKH.Parameters.AddWithValue("@DiaChi", txtDiaChi.Text);
                        cmdKH.ExecuteNonQuery();

                        // C. Insert TaiKhoan (Lấy SĐT làm User)
                        string sqlTK = "INSERT INTO TaiKhoan (TenDangNhap, MatKhau, MaKhachHang) VALUES (@User, @Pass, @MaKH)";
                        SqlCommand cmdTK = new SqlCommand(sqlTK, conn, trans);
                        cmdTK.Parameters.AddWithValue("@User", txtSDT.Text);
                        cmdTK.Parameters.AddWithValue("@Pass", txtMatKhau.Text);
                        cmdTK.Parameters.AddWithValue("@MaKH", newMaKH);
                        cmdTK.ExecuteNonQuery();

                        // Set thông tin để tự động đăng nhập
                        SessionData.CurrentUserID = newMaKH;
                    }

                    // --- HOÀN TẤT ---
                    trans.Commit();
                    MessageBox.Show("Đăng ký thành công!\nTên đăng nhập là số điện thoại của bạn.", "Chúc mừng");

                    // Tự động Login
                    SessionData.CurrentUsername = txtSDT.Text;
                    SessionData.CurrentUserFullName = txtHoTen.Text;
                    // Role sẽ được cập nhật lại khi vào DB check, tạm thời gán hiển thị
                    _mainForm.UpdateHeaderState();

                    // Logic chuyển trang: Nếu là NV thì về Login để đăng nhập lại cho chắc (để phân quyền), Khách thì vào luôn
                    if (!string.IsNullOrEmpty(maNV))
                        _mainForm.LoadView(new UC_Login(_mainForm));
                    else
                        _mainForm.LoadView(new UC_FastSearch(_mainForm));
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi hệ thống: " + ex.Message);
                }
            }
        }

        private bool CheckExistTransaction(SqlConnection conn, SqlTransaction trans, string table, string column, string value)
        {
            string sql = $"SELECT COUNT(*) FROM {table} WHERE {column} = @Val";
            SqlCommand cmd = new SqlCommand(sql, conn, trans); // Quan trọng: Phải gắn 'trans' vào
            cmd.Parameters.AddWithValue("@Val", value);
            return (int)cmd.ExecuteScalar() > 0;
        }


        private bool CheckExist(SqlConnection conn, string table, string column, string value)
        {
            string sql = $"SELECT COUNT(*) FROM {table} WHERE {column} = @Val";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Val", value);
            return (int)cmd.ExecuteScalar() > 0;
        }

        private bool ValidateForm()
        {
            errorProvider.Clear();
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtHoTen.Text)) SetError(txtHoTen, "Vui lòng nhập họ tên", ref isValid);
            if (string.IsNullOrWhiteSpace(txtSDT.Text) || txtSDT.Text.Length < 9) SetError(txtSDT, "SĐT không hợp lệ", ref isValid);
            if (string.IsNullOrWhiteSpace(txtCMND.Text)) SetError(txtCMND, "Vui lòng nhập CMND/CCCD", ref isValid);
            if (string.IsNullOrWhiteSpace(txtMatKhau.Text) || txtMatKhau.Text.Length < 6) SetError(txtMatKhau, "Mật khẩu quá ngắn", ref isValid);
            if (txtMatKhau.Text != txtMatKhau2.Text) SetError(txtMatKhau2, "Mật khẩu không khớp", ref isValid);

            return isValid;
        }

        private void SetError(Control c, string msg, ref bool flag)
        {
            errorProvider.SetError(c, msg);
            flag = false;
        }

        // Helper tạo Textbox style giống hình
        private TextBox CreateTextBox(Panel parent, string placeholder, int x, int y, int width, bool isPass = false)
        {
            TextBox txt = new TextBox { Location = new Point(x, y), Width = width, Font = UIHelper.MainFont };
            txt.PlaceholderText = placeholder; // Hiện chữ mờ
            if (isPass) txt.UseSystemPasswordChar = true;
            parent.Controls.Add(txt);
            return txt;
        }

        private void CreatePlaceholderLabel(Panel parent, string text, int x, int y)
        {
            Label lbl = new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8) };
            parent.Controls.Add(lbl);
        }
    }
}