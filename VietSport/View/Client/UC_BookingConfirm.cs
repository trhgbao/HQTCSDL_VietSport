using System;
using System.Drawing;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic; // Để dùng List
using VietSportSystem.View.Staff.Receptionist; // Để dùng FormSelectService

namespace VietSportSystem
{
    public class UC_BookingConfirm : UserControl
    {
        private MainForm _mainForm;
        private SanInfo _sanInfo;
        
        // Controls
        private DateTimePicker dtpStart, dtpEnd;
        private Label lblDuration, lblTotalPrice;
        private TextBox txtNote, txtVoucher;
        private Label lblServiceList; // Hiển thị danh sách dịch vụ đã chọn
        private CheckBox chkConflictDemo5; // Demo Xung đột 5: Phantom Read (Đặt sân)
        private CheckBox chkConflictDemo6; // Demo Xung đột 6: Lost Update (Thuê dụng cụ)
        
        // Variables
        private decimal currentTotalCourt = 0; // Tiền sân
        private decimal currentTotalService = 0; // Tiền dịch vụ
        private List<ServiceItem> _selectedServices = new List<ServiceItem>(); // List lưu dịch vụ

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

            // 1. Container chính
            Panel pnlContainer = new Panel { Size = new Size(900, 600), BackColor = Color.White }; // Tăng chiều cao lên 600
            pnlContainer.Location = new Point((this.Width - 900) / 2, 50);
            this.Resize += (s, e) => { pnlContainer.Left = (this.Width - pnlContainer.Width) / 2; };

            // Header
            Label lblTitle = new Label { 
                Text = "XÁC NHẬN ĐẶT SÂN", Dock = DockStyle.Top, Height = 60, 
                TextAlign = ContentAlignment.MiddleCenter, Font = UIHelper.HeaderFont, 
                BackColor = UIHelper.SecondaryColor, ForeColor = Color.White 
            };
            pnlContainer.Controls.Add(lblTitle);

            // Grid chia cột
            TableLayoutPanel grid = new TableLayoutPanel { 
                Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20, 30, 20, 20) 
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // --- CỘT TRÁI (Thông tin & Giờ & Dịch vụ) ---
            Panel pnlLeft = new Panel { Dock = DockStyle.Fill };
            
            Label lblSan = new Label { Text = $"SÂN: {_sanInfo.TenSan}", Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, ForeColor = UIHelper.PrimaryColor };
            Label lblGia = new Label { Text = $"Đơn giá: {_sanInfo.GiaTien:N0} VNĐ/giờ", Location = new Point(0, 30), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Italic) };

            // GroupBox chọn giờ
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

            // --- PHẦN MỚI: CHỌN DỊCH VỤ ---
            Label lblDVTitle = new Label { Text = "Dịch vụ đi kèm:", Location = new Point(0, 190), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            
            Button btnService = new Button { Text = "➕ Thêm Nước/Dụng cụ", Location = new Point(0, 215), Size = new Size(180, 35) };
            UIHelper.StyleButton(btnService, false);
            btnService.Click += BtnService_Click;

            lblServiceList = new Label { 
                Text = "Chưa chọn dịch vụ nào", 
                Location = new Point(200, 220), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.DimGray
            };

            // Ghi chú (Đẩy xuống dưới)
            Label lblNote = new Label { Text = "Ghi chú:", Location = new Point(0, 280), AutoSize = true };
            txtNote = new TextBox { Location = new Point(0, 305), Width = 480, Height = 60, Multiline = true, BorderStyle = BorderStyle.FixedSingle };

            pnlLeft.Controls.AddRange(new Control[] { lblSan, lblGia, grpTime, lblDVTitle, btnService, lblServiceList, lblNote, txtNote });


            // --- CỘT PHẢI (Thanh toán) ---
            Panel pnlRight = new Panel { Dock = DockStyle.Fill };
            Label lblPayTitle = new Label { Text = "THANH TOÁN", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 0), AutoSize = true };
            
            lblTotalPrice = new Label { 
                Text = "0 VNĐ", 
                Font = new Font("Segoe UI", 18, FontStyle.Bold), 
                ForeColor = Color.Red, 
                Location = new Point(20, 40), 
                AutoSize = true 
            };

            Label lblVoucher = new Label { Text = "Mã giảm giá:", Location = new Point(20, 100), AutoSize = true };
            txtVoucher = new TextBox { Location = new Point(20, 125), Width = 180, Font = UIHelper.MainFont };
            Button btnApply = new Button { Text = "Áp dụng", Location = new Point(210, 124), Size = new Size(80, 29) };
            UIHelper.StyleButton(btnApply, false);
            btnApply.Click += (s, e) => MessageBox.Show("Mã giảm giá không tồn tại!");

            // Checkbox demo Xung đột 5: Phantom Read (Đặt sân)
            chkConflictDemo5 = new CheckBox
            {
                Text = "Demo: Xung đột 5",
                Location = new Point(20, 170),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = UIHelper.SecondaryColor
            };

            // Checkbox demo Xung đột 6: Lost Update (Thuê dụng cụ)
            chkConflictDemo6 = new CheckBox
            {
                Text = "Demo: Xung đột 6",
                Location = new Point(20, 200),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };
            
            Button btnConfirm = new Button { Text = "XÁC NHẬN ĐẶT", Location = new Point(20, 215), Size = new Size(270, 50) };
            UIHelper.StyleButton(btnConfirm, true);
            btnConfirm.Click += BtnConfirm_Click;

            pnlRight.Controls.AddRange(new Control[] { lblPayTitle, lblTotalPrice, lblVoucher, txtVoucher, btnApply, chkConflictDemo5, chkConflictDemo6, btnConfirm });

            grid.Controls.Add(pnlLeft, 0, 0);
            grid.Controls.Add(pnlRight, 1, 0);

            pnlContainer.Controls.Add(lblTitle);
            pnlContainer.Controls.Add(grid);
            lblTitle.BringToFront();

            this.Controls.Add(pnlContainer);
        }

        // --- SỰ KIỆN: CHỌN DỊCH VỤ ---
        private void BtnService_Click(object sender, EventArgs e)
        {
            FormSelectService frm = new FormSelectService();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                _selectedServices = frm.SelectedServices;
                
                // Tính lại tổng tiền dịch vụ
                currentTotalService = 0;
                string displayList = "";
                
                foreach (var item in _selectedServices)
                {
                    currentTotalService += item.ThanhTien;
                    displayList += $"{item.TenDV} (x{item.SoLuong}), ";
                }

                // Cập nhật giao diện
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

                CalculateTotal(); // Tính lại Tổng tiền cuối cùng
            }
        }

        // Logic tính tiền (Sân + Dịch vụ)
        private void Time_Changed(object sender, EventArgs e)
        {
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            if (dtpEnd.Value <= dtpStart.Value)
            {
                lblDuration.Text = "(Lỗi thời gian)";
                lblDuration.ForeColor = Color.Red;
                lblTotalPrice.Text = "---";
                currentTotalCourt = 0;
                return;
            }

            TimeSpan span = dtpEnd.Value - dtpStart.Value;
            double hours = Math.Round(span.TotalHours, 1);
            
            if (hours < 0.5) { 
                lblDuration.Text = "(Tối thiểu 30p)";
                currentTotalCourt = 0;
            }
            else
            {
                lblDuration.Text = $"({hours} giờ)";
                currentTotalCourt = (decimal)hours * _sanInfo.GiaTien;
            }

            // TỔNG CỘNG = TIỀN SÂN + TIỀN DỊCH VỤ
            decimal finalTotal = currentTotalCourt + currentTotalService;
            lblTotalPrice.Text = finalTotal.ToString("N0") + " VNĐ";
        }

        // --- SỰ KIỆN: XÁC NHẬN ĐẶT (Có trừ kho) ---
        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (currentTotalCourt <= 0) { MessageBox.Show("Vui lòng chọn thời gian hợp lệ!"); return; }
            if (!SessionData.IsLoggedIn()) { MessageBox.Show("Vui lòng đăng nhập!"); return; }

            // Lưu ý: sp_DatSan_KiemTraGioiHan sẽ tự INSERT vào PhieuDatSan.
            // SP sẽ tự động lấy giới hạn sân từ bảng THAMSO (key MAX_BOOK).
            try
            {
                string maSanThuc = _sanInfo.TenSan.Split('-')[0].Trim();

                string? msg;
                bool isConflictDemo5 = chkConflictDemo5.Checked;

                // Xử lý đặt sân (Xung đột 5: Phantom Read)
                if (isConflictDemo5)
                {
                    msg = DatabaseHelper.DatSan_GayXungDot(
                        maKhachHang: SessionData.CurrentUserID,
                        maSan: maSanThuc,
                        gioBatDau: dtpStart.Value,
                        gioKetThuc: dtpEnd.Value
                    );
                }
                else
                {
                    msg = DatabaseHelper.DatSan_KiemTraGioiHan(
                        maKhachHang: SessionData.CurrentUserID,
                        maSan: maSanThuc,
                        gioBatDau: dtpStart.Value,
                        gioKetThuc: dtpEnd.Value,
                        kenhDat: "Online"
                    );
                }

                if (!string.IsNullOrEmpty(msg))
                {
                    // Với SP gây xung đột 5, msg có thể là Thành công/Thất bại; kiểm tra để quyết định dừng.
                    bool isFailure = msg.StartsWith("Thất bại", StringComparison.OrdinalIgnoreCase);
                    if (isConflictDemo5 && !isFailure)
                    {
                        // Thành công: chỉ hiển thị thông tin, tiếp tục luồng
                        MessageBox.Show(msg, "Kết quả (demo xung đột 5)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(msg, "Không thể đặt sân", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Xử lý trừ kho dịch vụ (Xung đột 6: Lost Update)
                bool isConflictDemo6 = chkConflictDemo6.Checked;
                foreach (var item in _selectedServices)
                {
                    string? msgDV;
                    if (isConflictDemo6)
                    {
                        // Dùng SP gây xung đột Lost Update
                        msgDV = DatabaseHelper.ThueDungCu_GayXungDot(item.MaDV, item.SoLuong);
                        if (!string.IsNullOrEmpty(msgDV))
                        {
                            // SP trả về SELECT KetQua, có thể là "Thành công..." hoặc "Lỗi..." hoặc "Deadlock..."
                            bool isSuccess = msgDV.Contains("Thành công", StringComparison.OrdinalIgnoreCase);
                            bool isDeadlock = msgDV.Contains("Deadlock", StringComparison.OrdinalIgnoreCase);
                            
                            if (isSuccess)
                            {
                                // Thành công: hiển thị thông tin và tiếp tục
                                MessageBox.Show($"Demo Xung đột 6 - {item.TenDV}:\n{msgDV}\n\n⚠️ Lưu ý: Kiểm tra tồn kho trong DB để thấy Lost Update!", 
                                    "Kết quả (demo xung đột 6)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (isDeadlock)
                            {
                                // Deadlock: đây cũng là một dạng xung đột, nhưng SQL Server đã tự động xử lý
                                MessageBox.Show($"Demo Xung đột 6 - {item.TenDV}:\n{msgDV}\n\n⚠️ Deadlock xảy ra do nhiều giao dịch cùng cập nhật!", 
                                    "Deadlock (demo xung đột 6)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                // Không return, tiếp tục với các dịch vụ khác
                            }
                            else
                            {
                                // Lỗi khác (không đủ hàng, v.v.)
                                MessageBox.Show($"Lỗi khi trừ kho dịch vụ {item.TenDV}: {msgDV}", "Lỗi kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return; // Dừng lại nếu lỗi nghiêm trọng
                            }
                        }
                    }
                    else
                    {
                        // Dùng SP chuẩn có UPDLOCK
                        msgDV = DatabaseHelper.ThueDungCu(item.MaDV, item.SoLuong);
                        if (!string.IsNullOrEmpty(msgDV))
                        {
                            MessageBox.Show($"Lỗi khi trừ kho dịch vụ {item.TenDV}: {msgDV}", "Lỗi kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                // Chuyển sang màn thanh toán (không có mã phiếu từ SP, nên chỉ hiển thị tổng tiền)
                decimal finalTotal = currentTotalCourt + currentTotalService;
                _mainForm.LoadView(new UC_Payment(_mainForm, null, finalTotal));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}
