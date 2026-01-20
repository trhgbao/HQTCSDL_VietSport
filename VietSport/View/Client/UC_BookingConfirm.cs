using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using VietSportSystem.View.Staff.Receptionist;

namespace VietSportSystem
{
    public class UC_BookingConfirm : UserControl
    {
        private MainForm _mainForm;
        private SanInfo _sanInfo;

        // Controls cơ bản
        private DateTimePicker dtpStart, dtpEnd;
        private Label lblDuration, lblTotalPrice;
        private TextBox txtNote, txtVoucher;
        private Label lblServiceList;
        private Label lblGia;

        // --- CÁC CHECKBOX DEMO ---
        private CheckBox chkDemoDirectVsOnline; // Demo 1: Direct vs Online (Procedure Gộp)
        private CheckBox chkDemoPhantom;        // Demo 5: Phantom Read
        private CheckBox chkDemoLostUpdate;     // Demo 6: Lost Update
        private CheckBox chkNonRepeatableDemo;  // Demo 3: Non-Repeatable Read
        private CheckBox chkDemoBaoTri;

        // Variables
        private decimal currentTotalCourt = 0;
        private decimal currentTotalService = 0;
        private List<ServiceItem> _selectedServices = new List<ServiceItem>();

        public UC_BookingConfirm(MainForm main, SanInfo san)
        {
            _mainForm = main;
            _sanInfo = san;
            InitializeComponent();
            CalculateTotal();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.WhiteSmoke;
            this.Dock = DockStyle.Fill;

            Panel pnlContainer = new Panel { Size = new Size(950, 600), BackColor = Color.White };
            pnlContainer.Location = new Point((this.Width - 950) / 2, 50);
            this.Resize += (s, e) => { pnlContainer.Left = (this.Width - pnlContainer.Width) / 2; };

            Label lblTitle = new Label
            {
                Text = "XÁC NHẬN ĐẶT SÂN",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = UIHelper.HeaderFont,
                BackColor = UIHelper.SecondaryColor,
                ForeColor = Color.White
            };
            pnlContainer.Controls.Add(lblTitle);

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(20, 30, 20, 20)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // ================= CỘT TRÁI =================
            Panel pnlLeft = new Panel { Dock = DockStyle.Fill };
            Label lblSan = new Label { Text = $"SÂN: {_sanInfo.TenSan}", Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, ForeColor = UIHelper.PrimaryColor };
            lblGia = new Label { Text = $"Đơn giá: {_sanInfo.GiaTien:N0} VNĐ/giờ", Location = new Point(0, 30), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Italic) };

            GroupBox grpTime = new GroupBox { Text = "Thời gian đặt", Location = new Point(0, 70), Size = new Size(480, 100), Font = new Font("Segoe UI", 10) };
            Label lblS = new Label { Text = "Bắt đầu:", Location = new Point(20, 35), AutoSize = true };
            dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Location = new Point(90, 32), Width = 160 };
            dtpStart.Value = DateTime.Now.AddHours(1).Date.AddHours(DateTime.Now.Hour + 1);
            dtpStart.ValueChanged += Time_Changed;

            Label lblE = new Label { Text = "Kết thúc:", Location = new Point(20, 70), AutoSize = true };
            dtpEnd = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Location = new Point(90, 67), Width = 160 };
            dtpEnd.Value = dtpStart.Value.AddHours(1);
            dtpEnd.ValueChanged += Time_Changed;

            lblDuration = new Label { Text = "(1 giờ)", Location = new Point(270, 67), AutoSize = true, ForeColor = Color.Blue, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            grpTime.Controls.AddRange(new Control[] { lblS, dtpStart, lblE, dtpEnd, lblDuration });

            // --- Dịch vụ ---
            Label lblDVTitle = new Label { Text = "Dịch vụ đi kèm:", Location = new Point(0, 190), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Button btnService = new Button { Text = "➕ Thêm Nước/Dụng cụ", Location = new Point(0, 215), Size = new Size(180, 35) };
            UIHelper.StyleButton(btnService, false);
            btnService.Click += BtnService_Click;

            lblServiceList = new Label { Text = "Chưa chọn dịch vụ nào", Location = new Point(200, 220), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.DimGray };

            // [DEMO 9] Non-Repeatable Read
            chkNonRepeatableDemo = new CheckBox
            {
                Text = "Demo 3: Thay đổi giá (Non-Repeatable Read)",
                Location = new Point(0, 260),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.OrangeRed
            };

            Label lblNote = new Label { Text = "Ghi chú:", Location = new Point(0, 290), AutoSize = true };
            txtNote = new TextBox { Location = new Point(0, 315), Width = 480, Height = 60, Multiline = true, BorderStyle = BorderStyle.FixedSingle };

            pnlLeft.Controls.AddRange(new Control[] { lblSan, lblGia, grpTime, lblDVTitle, btnService, lblServiceList, chkNonRepeatableDemo, lblNote, txtNote });

            // ================= CỘT PHẢI =================
            Panel pnlRight = new Panel { Dock = DockStyle.Fill };
            Label lblPayTitle = new Label { Text = "THANH TOÁN", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 0), AutoSize = true };

            lblTotalPrice = new Label { Text = "0 VNĐ", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(20, 40), AutoSize = true };

            Label lblVoucher = new Label { Text = "Mã giảm giá:", Location = new Point(20, 100), AutoSize = true };
            txtVoucher = new TextBox { Location = new Point(20, 125), Width = 180, Font = UIHelper.MainFont };
            Button btnApply = new Button { Text = "Áp dụng", Location = new Point(210, 124), Size = new Size(80, 29) };
            UIHelper.StyleButton(btnApply, false);
            btnApply.Click += (s, e) => MessageBox.Show("Mã giảm giá không tồn tại!");

            // [DEMO 1] Direct vs Online (Mới)
            chkDemoDirectVsOnline = new CheckBox
            {
                Text = "Demo 1: Xung đột Trực tiếp vs Online",
                Location = new Point(20, 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Blue
            };

            // [DEMO 2] Non-Repeatable 
            chkDemoBaoTri = new CheckBox
            {
                Text = "Demo 2: Đặt sân vs Bảo trì (Non-Repeatable)",
                // Chỉnh lại toạ độ Y của các checkbox khác nếu cần để không đè lên nhau
                Location = new Point(20, 235),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Purple // Màu tím để phân biệt
            };



            // [DEMO 5] Phantom Read
            chkDemoPhantom = new CheckBox
            {
                Text = "Demo 5: Xung đột Đặt sân (Phantom Read)",
                Location = new Point(20, 185),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UIHelper.SecondaryColor
            };

            // [DEMO 6] Lost Update
            chkDemoLostUpdate = new CheckBox
            {
                Text = "Demo 6: Xung đột Tồn kho (Lost Update)",
                Location = new Point(20, 210),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            Button btnConfirm = new Button { Text = "XÁC NHẬN ĐẶT", Location = new Point(20, 280), Size = new Size(270, 50) };
            UIHelper.StyleButton(btnConfirm, true);
            btnConfirm.Click += BtnConfirm_Click;

            pnlRight.Controls.AddRange(new Control[] { lblPayTitle, lblTotalPrice, lblVoucher, txtVoucher, btnApply, chkDemoDirectVsOnline, chkDemoBaoTri, chkDemoPhantom, chkDemoLostUpdate, btnConfirm });

            grid.Controls.Add(pnlLeft, 0, 0);
            grid.Controls.Add(pnlRight, 1, 0);

            pnlContainer.Controls.Add(lblTitle);
            pnlContainer.Controls.Add(grid);
            lblTitle.BringToFront();

            this.Controls.Add(pnlContainer);
        }

        private void BtnService_Click(object sender, EventArgs e)
        {
            FormSelectService frm = new FormSelectService();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _selectedServices = frm.SelectedServices;
                currentTotalService = 0;
                string displayList = "";
                foreach (var item in _selectedServices)
                {
                    currentTotalService += item.ThanhTien;
                    displayList += $"{item.TenDV} (x{item.SoLuong}), ";
                }

                if (_selectedServices.Count > 0)
                {
                    lblServiceList.Text = displayList.TrimEnd(',', ' ') + $"\nCộng thêm: {currentTotalService:N0} VNĐ";
                    lblServiceList.ForeColor = Color.Blue;
                }
                else
                {
                    lblServiceList.Text = "Chưa chọn dịch vụ nào";
                    lblServiceList.ForeColor = Color.DimGray;
                }
                CalculateTotal();
            }
        }

        private void Time_Changed(object sender, EventArgs e) => CalculateTotal();

        private string GetKhungGio(DateTime dt)
        {
            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday) return "Cuối tuần";
            if (dt.Hour >= 17 && dt.Hour <= 22) return "Giờ cao điểm";
            return "Ngày thường";
        }

        private void CalculateTotal()
        {
            if (dtpEnd.Value <= dtpStart.Value)
            {
                lblDuration.Text = "(Lỗi thời gian)";
                currentTotalCourt = 0;
            }
            else
            {
                double hours = Math.Round((dtpEnd.Value - dtpStart.Value).TotalHours, 1);
                if (hours < 0.5) { lblDuration.Text = "(Tối thiểu 30p)"; currentTotalCourt = 0; }
                else
                {
                    lblDuration.Text = $"({hours} giờ)";

                    // --- LOGIC DEMO 9: NON-REPEATABLE READ ---
                    decimal giaThue = 0;
                    string maSanThuc = _sanInfo.TenSan.Contains("-") ? _sanInfo.TenSan.Split('-')[0].Trim() : _sanInfo.TenSan;
                    string khungGio = GetKhungGio(dtpStart.Value);

                    try
                    {
                        using (SqlConnection conn = DatabaseHelper.GetConnection())
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conn;

                                if (chkNonRepeatableDemo.Checked)
                                {
                                    string sqlDemo = @"
                                    SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 
                                    BEGIN TRANSACTION;
                                    DECLARE @Gia1 decimal(18,0);
                                    SELECT @Gia1 = DonGia FROM GiaThueSan WHERE MaCoSo=(SELECT MaCoSo FROM SanTheThao WHERE MaSan=@MaSan) AND LoaiSan=(SELECT LoaiSan FROM SanTheThao WHERE MaSan=@MaSan) AND KhungGio=@KhungGio;
                                    
                                    WAITFOR DELAY '00:00:10'; -- Chờ 10s để T2 update

                                    DECLARE @Gia2 decimal(18,0);
                                    SELECT @Gia2 = DonGia FROM GiaThueSan WHERE MaCoSo=(SELECT MaCoSo FROM SanTheThao WHERE MaSan=@MaSan) AND LoaiSan=(SELECT LoaiSan FROM SanTheThao WHERE MaSan=@MaSan) AND KhungGio=@KhungGio;
                                    
                                    COMMIT TRANSACTION;
                                    SELECT CAST(@Gia1 AS VARCHAR) + '|' + CAST(@Gia2 AS VARCHAR); 
                                    ";
                                    cmd.CommandText = sqlDemo;
                                    cmd.Parameters.AddWithValue("@MaSan", maSanThuc);
                                    cmd.Parameters.AddWithValue("@KhungGio", khungGio);

                                    object result = cmd.ExecuteScalar();
                                    if (result != null)
                                    {
                                        string[] parts = result.ToString().Split('|');
                                        decimal g1 = decimal.Parse(parts[0]);
                                        decimal g2 = decimal.Parse(parts[1]);
                                        giaThue = g2;
                                        if (g1 != g2)
                                        {
                                            MessageBox.Show($"🔥 NON-REPEATABLE READ DETECTED!\nLần 1: {g1:N0}\nLần 2: {g2:N0}", "Demo Result");
                                        }
                                    }
                                }
                                else
                                {
                                    giaThue = _sanInfo.GiaTien;
                                }
                            }
                        }
                    }
                    catch { giaThue = _sanInfo.GiaTien; }

                    lblGia.Text = $"Đơn giá: {giaThue:N0} VNĐ/giờ ({khungGio})";
                    currentTotalCourt = (decimal)hours * giaThue;
                }
            }
            lblTotalPrice.Text = (currentTotalCourt + currentTotalService).ToString("N0") + " VNĐ";
        }

        // =================================================================================
        // MAIN LOGIC: XỬ LÝ ĐẶT SÂN
        // =================================================================================
        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (currentTotalCourt <= 0) { MessageBox.Show("Vui lòng chọn thời gian hợp lệ!"); return; }
            if (!SessionData.IsLoggedIn()) { MessageBox.Show("Vui lòng đăng nhập!"); return; }

            // Nếu demo 9 đang chạy
            if (chkNonRepeatableDemo.Checked)
            {
                MessageBox.Show("App sẽ treo 10 giây để tính giá (Demo 9).\nHãy Update SQL trong lúc này!", "Thông báo");
                CalculateTotal();
            }

            try
            {
                string maSanThuc = _sanInfo.TenSan.Split('-')[0].Trim();
                string? msg = "";
                bool isSuccess = false;

                // -----------------------------------------------------------
                // 1. XỬ LÝ ĐẶT SÂN (SCENARIO 1, 5, NORMAL)
                // -----------------------------------------------------------

                if (chkDemoDirectVsOnline.Checked)
                {
                    // === SCENARIO 1: TRỰC TIẾP vs ONLINE (Sử dụng Procedure GỘP) ===
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        // Gọi Procedure mới gộp: sp_DatSan_Scenario1
                        using (SqlCommand cmd = new SqlCommand("sp_DatSan_Scenario1", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            string maPhieuRandom = "P_D1_" + DateTime.Now.Ticks.ToString().Substring(10);

                            cmd.Parameters.AddWithValue("@MaKhachHang", SessionData.CurrentUserID);
                            cmd.Parameters.AddWithValue("@MaSan", maSanThuc);
                            cmd.Parameters.AddWithValue("@GioBatDau", dtpStart.Value);
                            cmd.Parameters.AddWithValue("@GioKetThuc", dtpEnd.Value);
                            cmd.Parameters.AddWithValue("@MaPhieuDat", maPhieuRandom);

                            // QUAN TRỌNG: @IsFix = 0 để chạy Mode Lỗi (Read Committed) cho Demo
                            // Nếu muốn test Fix, bạn có thể sửa số này thành 1 (hoặc thêm checkbox khác)
                            cmd.Parameters.AddWithValue("@IsFix", 0);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    int ketQua = Convert.ToInt32(reader["KetQua"]);
                                    msg = reader["ThongBao"].ToString();
                                    isSuccess = (ketQua == 1);
                                }
                            }
                        }
                    }

                    if (isSuccess)
                        MessageBox.Show(msg, "Kết quả Demo 1 (Mode Lỗi)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                    {
                        MessageBox.Show(msg, "Đặt sân thất bại (Demo 1)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (chkDemoBaoTri.Checked)
                {
                    MessageBox.Show("Hệ thống sẽ dừng 10s để kiểm tra trạng thái sân.\n\n👉 Trong lúc này, hãy dùng máy khác set trạng thái sân thành 'Bảo trì'!",
                                    "Hướng dẫn Demo 2", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("sp_Demo_DatSan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            // Tạo mã phiếu ngẫu nhiên
                            string maPhieu = "D2" + DateTime.Now.ToString("HHmmss");

                            cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            cmd.Parameters.AddWithValue("@MaKH", SessionData.CurrentUserID);
                            cmd.Parameters.AddWithValue("@MaSan", maSanThuc);
                            cmd.Parameters.AddWithValue("@GioBatDau", dtpStart.Value);
                            cmd.Parameters.AddWithValue("@GioKetThuc", dtpEnd.Value);

                            try
                            {
                                // Thực thi
                                cmd.ExecuteNonQuery();

                                // Nếu chạy qua dòng này nghĩa là không bị lỗi -> Thành công
                                MessageBox.Show("Đặt sân thành công! (Trạng thái sân bình thường)", "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Chuyển trang thanh toán (Logic cũ)
                                decimal totalForDemo = currentTotalCourt + currentTotalService;
                                _mainForm.LoadView(new UC_Payment(_mainForm, null, totalForDemo));
                                return; // Return luôn để không chạy phần tính dịch vụ bên dưới (Demo này chỉ test sân)
                            }
                            catch (SqlException sqlEx)
                            {
                                // Bắt lỗi 50001 hoặc 50002 từ SQL ném ra
                                if (sqlEx.Number == 50002 || sqlEx.Message.Contains("Bảo trì"))
                                {
                                    MessageBox.Show($"❌ PHÁT HIỆN XUNG ĐỘT (Non-Repeatable Read):\n{sqlEx.Message}", "Demo 2 Thành công", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return; // Dừng lại, không chuyển trang
                                }
                                else
                                {
                                    throw; // Lỗi khác thì ném tiếp
                                }
                            }
                        }
                    }
                }
                else if (chkDemoPhantom.Checked)
                {
                    // === SCENARIO 5: PHANTOM READ ===
                    msg = DatabaseHelper.DatSan_GayXungDot(SessionData.CurrentUserID, maSanThuc, dtpStart.Value, dtpEnd.Value);

                    if (!string.IsNullOrEmpty(msg))
                    {
                        bool isFailure = msg.StartsWith("Thất bại", StringComparison.OrdinalIgnoreCase);
                        if (!isFailure)
                            MessageBox.Show(msg, "Kết quả (Demo Phantom Read)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                        {
                            MessageBox.Show(msg, "Không thể đặt sân", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }
                else
                {
                    // === BÌNH THƯỜNG / AN TOÀN ===
                    msg = DatabaseHelper.DatSan_KiemTraGioiHan(SessionData.CurrentUserID, maSanThuc, dtpStart.Value, dtpEnd.Value, "Online");

                    if (!string.IsNullOrEmpty(msg))
                    {
                        // Lỗi thật hoặc Demo thất bại -> Dừng
                        MessageBox.Show(msg, "Không thể đặt sân", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // -----------------------------------------------------------
                // 2. XỬ LÝ DỊCH VỤ (SCENARIO 6 - LOST UPDATE)
                // -----------------------------------------------------------
                foreach (var item in _selectedServices)
                {
                    // [LOGIC VU] Nếu là VIP -> Bỏ qua ở đây, xử lý sau ở bước Thanh toán
                    if (string.Equals(item.MaDV, "DV_VIP", StringComparison.OrdinalIgnoreCase)) continue;

                    string? msgDV;
                    if (chkDemoLostUpdate.Checked)
                    {
                        msgDV = DatabaseHelper.ThueDungCu_GayXungDot(item.MaDV, item.SoLuong);
                        if (!string.IsNullOrEmpty(msgDV))
                            MessageBox.Show($"[Demo Lost Update] {item.TenDV}:\n{msgDV}", "Kết quả Trừ kho", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        msgDV = DatabaseHelper.ThueDungCu(item.MaDV, item.SoLuong);
                        if (!string.IsNullOrEmpty(msgDV))
                            MessageBox.Show($"Lỗi trừ kho {item.TenDV}: {msgDV}", "Lỗi kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                // -----------------------------------------------------------
                // 3. XỬ LÝ VIP CONTEXT (Đơn giản hóa, bỏ Scenario 14)
                // -----------------------------------------------------------
                bool hasVip = _selectedServices.Any(s => string.Equals(s.MaDV, "DV_VIP", StringComparison.OrdinalIgnoreCase));
                if (hasVip)
                {
                    // Lưu trạng thái vào Context để màn hình Payment xử lý tranh chấp 14
                    BookingContext.VipSelected = true;
                    BookingContext.VipStart = dtpEnd.Value;
                    BookingContext.VipEnd = dtpEnd.Value.AddMinutes(30);
                }
                else
                {
                    BookingContext.ClearVip();
                }

                // -----------------------------------------------------------
                // 4. CHUYỂN TRANG THANH TOÁN
                // -----------------------------------------------------------
                decimal finalTotal = currentTotalCourt + currentTotalService;
                _mainForm.LoadView(new UC_Payment(_mainForm, null, finalTotal));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }
    }
}