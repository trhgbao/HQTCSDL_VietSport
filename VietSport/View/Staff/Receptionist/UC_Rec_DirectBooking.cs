using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using VietSportSystem.View.Staff.Receptionist;

namespace VietSportSystem
{
    public class UC_Rec_DirectBooking : UserControl
    {
        // Inputs
        private TextBox txtHoTen, txtSDT, txtCMND;
        private DateTimePicker dtpNgay, dtpStart, dtpEnd;
        private ComboBox cboLoaiSan;
        private FlowLayoutPanel pnlSanTrong; // Danh sách sân trống bên dưới
        private List<ServiceItem> _selectedServices = new List<ServiceItem>();
        private Label lblDichVuSelected;

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

            Label lblDV = new Label { Text = "Dịch vụ kèm theo:", Location = new Point(50, 280), AutoSize = true, Font = new Font("Segoe UI", 10) };
            pnlForm.Controls.Add(lblDV);

            Button btnChonDV = new Button { Text = "➕ Chọn Dịch vụ", Location = new Point(50, 305), Size = new Size(150, 30) };
            btnChonDV.Click += BtnChonDV_Click;
            pnlForm.Controls.Add(btnChonDV);

            lblDichVuSelected = new Label { Text = "Chưa chọn dịch vụ nào", Location = new Point(220, 310), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Italic) };
            pnlForm.Controls.Add(lblDichVuSelected);
            // ------------------------------------------

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

        // 2. Thêm hàm xử lý sự kiện chọn dịch vụ
        private void BtnChonDV_Click(object sender, EventArgs e)
        {
            FormSelectService frm = new FormSelectService();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _selectedServices = frm.SelectedServices;

                // Hiển thị danh sách ra label cho Lễ tân thấy
                if (_selectedServices.Count > 0)
                {
                    string summary = "";
                    decimal totalService = 0;
                    foreach (var item in _selectedServices)
                    {
                        summary += $"{item.TenDV} (x{item.SoLuong}), ";
                        totalService += item.ThanhTien;
                    }
                    lblDichVuSelected.Text = summary.TrimEnd(',', ' ') + $" | Tổng tiền DV: {totalService:N0} đ";
                    lblDichVuSelected.ForeColor = Color.Blue;
                }
                else
                {
                    lblDichVuSelected.Text = "Chưa chọn dịch vụ nào";
                    lblDichVuSelected.ForeColor = Color.Black;
                }
            }
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

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Loai", cboLoaiSan.SelectedItem.ToString());
                    // Gộp ngày + giờ
                    DateTime start = dtpNgay.Value.Date + dtpStart.Value.TimeOfDay;
                    DateTime end = dtpNgay.Value.Date + dtpEnd.Value.TimeOfDay;

                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
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

                    string maPhieu = "TT" + DateTime.Now.ToString("ddHHmmss");
                    string sqlPhieu = @"INSERT INTO PhieuDatSan 
                                      (MaPhieuDat, MaKhachHang, MaSan, MaNhanVien, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
                                      VALUES (@Ma, @KH, @San, @NV, @Start, @End, N'Đã thanh toán', N'Trực tiếp')";

                    SqlCommand cmd = new SqlCommand(sqlPhieu, conn, trans);
                    cmd.Parameters.AddWithValue("@Ma", maPhieu);
                    cmd.Parameters.AddWithValue("@KH", _selectedMaKH);
                    cmd.Parameters.AddWithValue("@San", maSanChon);

                    // Xử lý Null cho mã nhân viên (như bạn đã sửa)
                    object valNV = SessionData.CurrentUserID;
                    if (valNV == null) valNV = DBNull.Value;
                    cmd.Parameters.AddWithValue("@NV", valNV);

                    cmd.Parameters.AddWithValue("@Start", dtpNgay.Value.Date + dtpStart.Value.TimeOfDay);
                    cmd.Parameters.AddWithValue("@End", dtpNgay.Value.Date + dtpEnd.Value.TimeOfDay);

                    cmd.ExecuteNonQuery(); // <-- Xong phần đặt sân

                    // =========================================================================
                    // --- PHẦN MỚI THÊM: CẬP NHẬT KHO DỊCH VỤ ---
                    // =========================================================================
                    if (_selectedServices != null && _selectedServices.Count > 0)
                    {
                        foreach (var item in _selectedServices)
                        {
                            // 1. Trừ số lượng tồn trong bảng DichVu
                            string sqlUpdateStock = "UPDATE DichVu SET SoLuongTon = SoLuongTon - @Qty WHERE MaDichVu = @MaDV";
                            SqlCommand cmdStock = new SqlCommand(sqlUpdateStock, conn, trans); // Dùng chung transaction 'trans'
                            cmdStock.Parameters.AddWithValue("@Qty", item.SoLuong);
                            cmdStock.Parameters.AddWithValue("@MaDV", item.MaDV);
                            cmdStock.ExecuteNonQuery();

                            // 2. (Tùy chọn) Nếu muốn lưu chi tiết hóa đơn dịch vụ thì insert vào bảng ChiTietSuDungDichVu ở đây
                            // Nhưng logic hiện tại chỉ yêu cầu trừ kho nên đoạn trên là đủ.
                        }
                    }
                    // =========================================================================

                    trans.Commit(); // Chốt tất cả thay đổi (Sân + Dịch vụ)

                    MessageBox.Show("Đặt sân và Cập nhật dịch vụ thành công!");

                    // --- RESET GIAO DIỆN ---
                    pnlSanTrong.Controls.Clear();
                    _selectedMaKH = "";
                    txtHoTen.Enabled = true;

                    // Reset luôn phần dịch vụ
                    _selectedServices.Clear();
                    lblDichVuSelected.Text = "Chưa chọn dịch vụ nào";
                    lblDichVuSelected.ForeColor = Color.Black;
                }
                catch (Exception ex)
                {
                    trans.Rollback(); // Nếu lỗi bất cứ đâu (sân hoặc dịch vụ) -> Hủy hết
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
    }
}