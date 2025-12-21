using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_Rec_DirectBooking : UserControl
    {
        // Inputs
        private TextBox txtHoTen, txtSDT, txtCMND, txtDichVu;
        private DateTimePicker dtpNgay, dtpStart, dtpEnd;
        private ComboBox cboLoaiSan;
        private FlowLayoutPanel pnlSanTrong; // Danh sách sân trống bên dưới

        private string _selectedMaKH = ""; // Nếu chọn từ tìm kiếm

        public UC_Rec_DirectBooking()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(40, 40, 40); // Nền tối

            // 1. FORM NHẬP LIỆU (Panel Xám ở giữa)
            Panel pnlForm = new Panel { Size = new Size(800, 450), BackColor = Color.LightGray };
            pnlForm.Location = new Point((1280 - 800) / 2, 20);

            Label lblTitle = new Label { Text = "ĐẶT SÂN TRỰC TIẾP", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(300, 10), AutoSize = true };

            // Các trường nhập liệu
            txtHoTen = CreateInput(pnlForm, "Họ và tên", 50);
            CreateInput(pnlForm, "Ngày sinh", 90); // Demo, không xử lý logic
            txtCMND = CreateInput(pnlForm, "Số CMND/CCCD", 130);
            txtSDT = CreateInput(pnlForm, "Số điện thoại", 170);

            // Dòng chọn ngày giờ
            dtpNgay = new DateTimePicker { Location = new Point(50, 240), Width = 150, Format = DateTimePickerFormat.Short };
            cboLoaiSan = new ComboBox { Location = new Point(220, 240), Width = 150 };
            cboLoaiSan.Items.AddRange(new string[] { "Bóng đá mini", "Cầu lông", "Tennis" });
            cboLoaiSan.SelectedIndex = 0;

            dtpStart = new DateTimePicker { Location = new Point(400, 240), Width = 100, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            dtpEnd = new DateTimePicker { Location = new Point(520, 240), Width = 100, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            dtpEnd.Value = DateTime.Now.AddHours(1);

            pnlForm.Controls.AddRange(new Control[] { dtpNgay, cboLoaiSan, dtpStart, dtpEnd });

            txtDichVu = CreateInput(pnlForm, "Dịch vụ kèm theo", 280);

            // BUTTONS
            Button btnCheck = new Button { Text = "Kiểm tra sân trống", Location = new Point(150, 350), Size = new Size(150, 40), BackColor = Color.Gray, ForeColor = Color.White };
            btnCheck.Click += BtnCheck_Click;

            Button btnConfirm = new Button { Text = "Xác nhận", Location = new Point(320, 350), Size = new Size(150, 40), BackColor = Color.Gray, ForeColor = Color.White };
            btnConfirm.Click += BtnConfirm_Click;

            Button btnFind = new Button { Text = "Tìm tài khoản", Location = new Point(500, 350), Size = new Size(150, 40), BackColor = Color.Gray, ForeColor = Color.White };
            btnFind.Click += BtnFind_Click;

            pnlForm.Controls.AddRange(new Control[] { lblTitle, btnCheck, btnConfirm, btnFind });

            // 2. DANH SÁCH SÂN TRỐNG (Bên dưới)
            Label lblList = new Label { Text = "Danh sách sân trống:", Location = new Point(240, 480), ForeColor = Color.White, AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            pnlSanTrong = new FlowLayoutPanel { Location = new Point(240, 510), Size = new Size(800, 200), AutoScroll = true };

            this.Controls.Add(pnlForm);
            this.Controls.Add(lblList);
            this.Controls.Add(pnlSanTrong);
        }

        private TextBox CreateInput(Panel p, string placeholder, int y)
        {
            TextBox txt = new TextBox { Location = new Point(50, y), Width = 700, Font = new Font("Segoe UI", 11) };
            txt.PlaceholderText = placeholder;
            p.Controls.Add(txt);
            return txt;
        }

        // --- LOGIC 1: TÌM TÀI KHOẢN ---
        private void BtnFind_Click(object sender, EventArgs e)
        {
            FormFindCustomer frm = new FormFindCustomer();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _selectedMaKH = frm.SelectedMaKH;
                txtHoTen.Text = frm.SelectedTenKH;
                txtSDT.Text = frm.SelectedSDT;
                txtCMND.Text = "Đã có trong hệ thống";
                txtHoTen.Enabled = false; // Khóa lại không cho sửa
            }
        }

        // --- LOGIC 2: KIỂM TRA SÂN TRỐNG ---
        private void BtnCheck_Click(object sender, EventArgs e)
        {
            pnlSanTrong.Controls.Clear();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Query tìm sân KHÔNG bị trùng giờ
                string sql = @"
                    SELECT MaSan, TenCoSo 
                    FROM SanTheThao s JOIN CoSo c ON s.MaCoSo = c.MaCoSo
                    WHERE LoaiSan = @Loai 
                    AND MaSan NOT IN (
                        SELECT MaSan FROM PhieuDatSan 
                        WHERE (@Start < GioKetThuc AND @End > GioBatDau)
                        AND TrangThaiThanhToan != N'Đã hủy'
                    )";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Loai", cboLoaiSan.SelectedItem.ToString());
                // Gộp ngày + giờ
                DateTime start = dtpNgay.Value.Date + dtpStart.Value.TimeOfDay;
                DateTime end = dtpNgay.Value.Date + dtpEnd.Value.TimeOfDay;

                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string maSanHienTai = reader["MaSan"].ToString();
                    string tenCoSo = reader["TenCoSo"].ToString();
                    // --------------------------------------------------------

                    Button btnSan = new Button
                    {
                        Text = maSanHienTai + "\n" + tenCoSo, // Dùng biến cục bộ
                        Size = new Size(200, 60),
                        BackColor = Color.LightGreen,
                        Tag = maSanHienTai // Dùng biến cục bộ
                    };

                    // Click chọn sân
                    btnSan.Click += (s, args) => {
                        foreach (Control c in pnlSanTrong.Controls) c.BackColor = Color.LightGreen;
                        btnSan.BackColor = Color.Orange;

                        btnSan.Tag = "SELECTED_" + maSanHienTai;
                        // ---------------------
                    };
                    pnlSanTrong.Controls.Add(btnSan);
                }
            }
        }

        // --- LOGIC 3: XÁC NHẬN ĐẶT ---
        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            // 1. Tìm sân đã chọn
            string maSanChon = "";
            foreach (Control c in pnlSanTrong.Controls)
            {
                if (c.Tag.ToString().StartsWith("SELECTED_"))
                {
                    maSanChon = c.Tag.ToString().Replace("SELECTED_", "");
                    break;
                }
            }
            if (maSanChon == "") { MessageBox.Show("Vui lòng chọn sân trống bên dưới!"); return; }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // 2. Nếu khách vãng lai (chưa có MaKH) -> Tạo KH mới
                    if (string.IsNullOrEmpty(_selectedMaKH))
                    {
                        _selectedMaKH = "VL" + DateTime.Now.ToString("ddHHmm");
                        string sqlKH = "INSERT INTO KhachHang (MaKhachHang, HoTen, SoDienThoai, CMND, NgaySinh) VALUES (@Ma, @Ten, @SDT, @CMND, GETDATE())";
                        SqlCommand cmdKH = new SqlCommand(sqlKH, conn, trans);
                        cmdKH.Parameters.AddWithValue("@Ma", _selectedMaKH);
                        cmdKH.Parameters.AddWithValue("@Ten", txtHoTen.Text);
                        cmdKH.Parameters.AddWithValue("@SDT", txtSDT.Text);
                        cmdKH.Parameters.AddWithValue("@CMND", txtCMND.Text);
                        cmdKH.ExecuteNonQuery();
                    }

                    // 3. Tạo phiếu đặt
                    string maPhieu = "TT" + DateTime.Now.ToString("ddHHmmss");
                    string sqlPhieu = @"INSERT INTO PhieuDatSan 
                                      (MaPhieuDat, MaKhachHang, MaSan, MaNhanVien, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
                                      VALUES (@Ma, @KH, @San, @NV, @Start, @End, N'Đã thanh toán', N'Trực tiếp')";

                    SqlCommand cmd = new SqlCommand(sqlPhieu, conn, trans);
                    cmd.Parameters.AddWithValue("@Ma", maPhieu);
                    cmd.Parameters.AddWithValue("@KH", _selectedMaKH);
                    cmd.Parameters.AddWithValue("@San", maSanChon);
                    object valNV = SessionData.CurrentUserID;
                    if (valNV == null) 
                        valNV = DBNull.Value;

                    cmd.Parameters.AddWithValue("@NV", valNV);
                    cmd.Parameters.AddWithValue("@Start", dtpNgay.Value.Date + dtpStart.Value.TimeOfDay);
                    cmd.Parameters.AddWithValue("@End", dtpNgay.Value.Date + dtpEnd.Value.TimeOfDay);

                    cmd.ExecuteNonQuery();
                    trans.Commit();

                    MessageBox.Show("Đặt sân thành công!");
                    pnlSanTrong.Controls.Clear(); // Reset
                    _selectedMaKH = "";
                    txtHoTen.Enabled = true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
    }
}