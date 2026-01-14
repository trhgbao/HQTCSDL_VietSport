using System;
using System.Drawing;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;
using VietSportSystem.View.Staff.Receptionist; // Để dùng FormSelectService
using System.Linq; // Để dùng Any()

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
        private Label lblServiceList;

        // Demo Checkboxes
        private CheckBox chkConflictDemo; // Demo Xung đột (Nam)
        private CheckBox chkFixVip;       // Demo VIP (Vu)

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

            Panel pnlContainer = new Panel { Size = new Size(900, 600), BackColor = Color.White };
            pnlContainer.Location = new Point((this.Width - 900) / 2, 50);
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

            // --- CỘT TRÁI ---
            Panel pnlLeft = new Panel { Dock = DockStyle.Fill };
            Label lblSan = new Label { Text = $"SÂN: {_sanInfo.TenSan}", Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true, ForeColor = UIHelper.PrimaryColor };
            Label lblGia = new Label { Text = $"Đơn giá: {_sanInfo.GiaTien:N0} VNĐ/giờ", Location = new Point(0, 30), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Italic) };

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

            // Dịch vụ
            Label lblDVTitle = new Label { Text = "Dịch vụ đi kèm:", Location = new Point(0, 190), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Button btnService = new Button { Text = "➕ Thêm Nước/Dụng cụ", Location = new Point(0, 215), Size = new Size(180, 35) };
            UIHelper.StyleButton(btnService, false);
            btnService.Click += BtnService_Click;

            lblServiceList = new Label { Text = "Chưa chọn dịch vụ nào", Location = new Point(200, 220), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.DimGray };

            // Checkbox demo VIP (Vu)
            chkFixVip = new CheckBox { Text = "Bật FIX VIP (xung đột 14)", Location = new Point(0, 255), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UIHelper.SecondaryColor };

            Label lblNote = new Label { Text = "Ghi chú:", Location = new Point(0, 285), AutoSize = true };
            txtNote = new TextBox { Location = new Point(0, 310), Width = 480, Height = 60, Multiline = true, BorderStyle = BorderStyle.FixedSingle };

            pnlLeft.Controls.AddRange(new Control[] { lblSan, lblGia, grpTime, lblDVTitle, btnService, lblServiceList, chkFixVip, lblNote, txtNote });

            // --- CỘT PHẢI ---
            Panel pnlRight = new Panel { Dock = DockStyle.Fill };
            Label lblPayTitle = new Label { Text = "THANH TOÁN", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 0), AutoSize = true };

            lblTotalPrice = new Label { Text = "0 VNĐ", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(20, 40), AutoSize = true };

            Label lblVoucher = new Label { Text = "Mã giảm giá:", Location = new Point(20, 100), AutoSize = true };
            txtVoucher = new TextBox { Location = new Point(20, 125), Width = 180, Font = UIHelper.MainFont };
            Button btnApply = new Button { Text = "Áp dụng", Location = new Point(210, 124), Size = new Size(80, 29) };
            UIHelper.StyleButton(btnApply, false);
            btnApply.Click += (s, e) => MessageBox.Show("Mã giảm giá không tồn tại!");

            // Checkbox demo Xung đột (Nam)
            chkConflictDemo = new CheckBox { Text = "Demo: Gây xung đột", Location = new Point(20, 170), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = UIHelper.SecondaryColor };

            Button btnConfirm = new Button { Text = "XÁC NHẬN ĐẶT", Location = new Point(20, 215), Size = new Size(270, 50) };
            UIHelper.StyleButton(btnConfirm, true);
            btnConfirm.Click += BtnConfirm_Click;

            pnlRight.Controls.AddRange(new Control[] { lblPayTitle, lblTotalPrice, lblVoucher, txtVoucher, btnApply, chkConflictDemo, btnConfirm });

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
                else { lblDuration.Text = $"({hours} giờ)"; currentTotalCourt = (decimal)hours * _sanInfo.GiaTien; }
            }
            lblTotalPrice.Text = (currentTotalCourt + currentTotalService).ToString("N0") + " VNĐ";
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (currentTotalCourt <= 0) { MessageBox.Show("Vui lòng chọn thời gian hợp lệ!"); return; }
            if (!SessionData.IsLoggedIn()) { MessageBox.Show("Vui lòng đăng nhập!"); return; }

            try
            {
                string maSanThuc = _sanInfo.TenSan.Split('-')[0].Trim();

                // 1. XỬ LÝ ĐẶT SÂN (Logic của Nam)
                string? msg;
                if (chkConflictDemo.Checked)
                {
                    msg = DatabaseHelper.DatSan_GayXungDot(SessionData.CurrentUserID, maSanThuc, dtpStart.Value, dtpEnd.Value);
                }
                else
                {
                    msg = DatabaseHelper.DatSan_KiemTraGioiHan(SessionData.CurrentUserID, maSanThuc, dtpStart.Value, dtpEnd.Value, "Online");
                }

                if (!string.IsNullOrEmpty(msg))
                {
                    bool isFailure = msg.StartsWith("Thất bại", StringComparison.OrdinalIgnoreCase);
                    if (chkConflictDemo.Checked && !isFailure)
                        MessageBox.Show(msg, "Kết quả (demo xung đột)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                    {
                        MessageBox.Show(msg, "Không thể đặt sân", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // 2. XỬ LÝ DỊCH VỤ (Logic kết hợp)
                foreach (var item in _selectedServices)
                {
                    // Nếu là VIP (Logic Vu) -> Bỏ qua, xử lý sau ở Payment
                    if (string.Equals(item.MaDV, "DV_VIP", StringComparison.OrdinalIgnoreCase)) continue;

                    // Nếu là DV thường (Logic Nam) -> Trừ kho ngay
                    string? msgDV = DatabaseHelper.ThueDungCu(item.MaDV, item.SoLuong);
                    if (!string.IsNullOrEmpty(msgDV))
                        MessageBox.Show($"Lỗi trừ kho {item.TenDV}: {msgDV}");
                }

                // 3. XỬ LÝ VIP CONTEXT (Logic Vu)
                bool hasVip = _selectedServices.Any(s => string.Equals(s.MaDV, "DV_VIP", StringComparison.OrdinalIgnoreCase));
                if (hasVip)
                {
                    BookingContext.VipSelected = true;
                    BookingContext.VipStart = dtpEnd.Value;
                    BookingContext.VipEnd = dtpEnd.Value.AddMinutes(30);
                    BookingContext.VipUseFix = chkFixVip.Checked;
                }
                else BookingContext.ClearVip();

                // 4. CHUYỂN TRANG
                _mainForm.LoadView(new UC_Payment(_mainForm, null, currentTotalCourt + currentTotalService));
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
    }
}