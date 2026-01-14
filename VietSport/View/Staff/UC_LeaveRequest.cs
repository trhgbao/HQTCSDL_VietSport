using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_LeaveRequest : UserControl
    {
        private DateTimePicker dtpNgayNghi;
        private TextBox txtLyDo;
        private TextBox txtNguoiDuyet, txtNguoiThay; // Thêm ô nhập để linh hoạt
        private CheckBox chkFix;
        private Button btnSend;
        private Label lblMsg;

        public UC_LeaveRequest()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Header
            Label lblTitle = new Label
            {
                Text = "ĐƠN XIN NGHỈ PHÉP",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = UIHelper.HeaderFont,
                BackColor = UIHelper.SecondaryColor,
                ForeColor = Color.White
            };

            // Container
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(50) };

            // GroupBox
            GroupBox grpInfo = new GroupBox { Text = "Thông tin đơn", Dock = DockStyle.Top, Height = 350, Font = new Font("Segoe UI", 11) };

            // Các controls
            Label lbl1 = new Label { Text = "Ngày xin nghỉ:", Location = new Point(30, 40), AutoSize = true };
            dtpNgayNghi = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 150, Location = new Point(180, 35), Value = DateTime.Today.AddDays(1) };

            chkFix = new CheckBox { Text = "Bật FIX (Scenario 8)", AutoSize = true, Location = new Point(400, 38), ForeColor = Color.Blue };

            Label lbl2 = new Label { Text = "Lý do nghỉ:", Location = new Point(30, 90), AutoSize = true };
            txtLyDo = new TextBox { Width = 500, Height = 60, Location = new Point(180, 90), Multiline = true, Text = "Bận việc cá nhân", BorderStyle = BorderStyle.FixedSingle };

            // Thêm ô chọn người duyệt/thay thế (Để demo cho tiện)
            Label lbl3 = new Label { Text = "Người duyệt (Mã):", Location = new Point(30, 170), AutoSize = true };
            txtNguoiDuyet = new TextBox { Location = new Point(180, 165), Width = 150, Text = "NV_QL" }; // Mặc định gửi cho Quản lý

            Label lbl4 = new Label { Text = "Người thay (Mã):", Location = new Point(30, 210), AutoSize = true };
            txtNguoiThay = new TextBox { Location = new Point(180, 205), Width = 150, Text = "NV_LT" };

            // Nút gửi
            btnSend = new Button { Text = "Gửi đơn ngay", Width = 200, Height = 40, Location = new Point(180, 260) };
            UIHelper.StyleButton(btnSend, true);
            btnSend.Click += async (s, e) => await SendLeaveAsync();

            lblMsg = new Label { AutoSize = true, Location = new Point(180, 310), ForeColor = Color.DimGray, Font = new Font("Segoe UI", 10, FontStyle.Italic) };

            grpInfo.Controls.AddRange(new Control[] { lbl1, dtpNgayNghi, chkFix, lbl2, txtLyDo, lbl3, txtNguoiDuyet, lbl4, txtNguoiThay, btnSend, lblMsg });

            pnlMain.Controls.Add(grpInfo);
            this.Controls.Add(pnlMain);
            this.Controls.Add(lblTitle);
        }

        private async Task SendLeaveAsync()
        {
            // Lấy mã nhân viên đang đăng nhập
            string maNV = SessionData.CurrentUserID;
            if (string.IsNullOrEmpty(maNV))
            {
                MessageBox.Show("Không xác định được nhân viên. Vui lòng đăng nhập lại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool useFix = chkFix.Checked;
            string proc = useFix ? "dbo.sp_FIX08_Employee_RequestLeave" : "dbo.sp_ERR08_Employee_RequestLeave_BUG";

            DateTime ngay = dtpNgayNghi.Value.Date;
            string lyDo = txtLyDo.Text.Trim();
            string nguoiDuyet = txtNguoiDuyet.Text.Trim();
            string nguoiThay = txtNguoiThay.Text.Trim();

            SetBusy(true, "Đang gửi đơn...");

            try
            {
                string resultMsg = await Task.Run(() =>
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    using (SqlCommand cmd = new SqlCommand(proc, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaNhanVien", maNV);
                        cmd.Parameters.AddWithValue("@MaNguoiDuyet", nguoiDuyet);
                        cmd.Parameters.AddWithValue("@MaNguoiThayThe", nguoiThay);
                        cmd.Parameters.Add("@NgayNghi", SqlDbType.Date).Value = ngay;
                        cmd.Parameters.AddWithValue("@LyDo", lyDo);

                        conn.Open();
                        object o = cmd.ExecuteScalar();
                        return o?.ToString() ?? "OK";
                    }
                });

                lblMsg.ForeColor = Color.SeaGreen;
                lblMsg.Text = $"✅ {resultMsg}";
                MessageBox.Show(resultMsg, "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = Color.Firebrick;
                lblMsg.Text = "❌ " + ex.Message;
                MessageBox.Show(ex.Message, "Lỗi xin nghỉ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, "");
            }
        }

        private void SetBusy(bool busy, string msg)
        {
            btnSend.Enabled = !busy;
            dtpNgayNghi.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(msg))
            {
                lblMsg.ForeColor = Color.DimGray;
                lblMsg.Text = msg;
            }
        }
    }
}