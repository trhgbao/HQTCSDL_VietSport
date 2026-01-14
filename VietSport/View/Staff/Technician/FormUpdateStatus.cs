using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace VietSportSystem
{
    public class FormUpdateStatus : Form
    {
        private string _maSan;
        private string _currentStatus;
        private string _currentNote;

        // Controls
        private RadioButton rdoGood, rdoBad;
        private TextBox txtReason;
        private DateTimePicker dtpEnd;
        private GroupBox grpBad;
        private CheckBox chkDemoDirty; // Checkbox Demo

        public FormUpdateStatus(string maSan, string currentStatus, string currentNote)
        {
            _maSan = maSan;
            _currentStatus = currentStatus;
            _currentNote = currentNote;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 500); // Tăng chiều cao chút
            this.Text = "Cập nhật trạng thái sân: " + _maSan;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label { Text = "CHỌN TRẠNG THÁI", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(150, 20), AutoSize = true };

            rdoGood = new RadioButton { Text = "Hoạt động tốt (Sẵn sàng)", Location = new Point(50, 70), AutoSize = true, Font = new Font("Segoe UI", 11) };
            rdoBad = new RadioButton { Text = "Cần bảo trì / Sửa chữa", Location = new Point(50, 110), AutoSize = true, Font = new Font("Segoe UI", 11) };

            grpBad = new GroupBox { Text = "Thông tin hỏng hóc", Location = new Point(50, 150), Size = new Size(400, 180), Enabled = false };
            Label lblReason = new Label { Text = "Mô tả lỗi:", Location = new Point(20, 30) };
            txtReason = new TextBox { Location = new Point(20, 50), Width = 350, Height = 60, Multiline = true, BorderStyle = BorderStyle.FixedSingle };
            txtReason.Text = _currentNote;
            Label lblDate = new Label { Text = "Dự kiến xong:", Location = new Point(20, 120) };
            dtpEnd = new DateTimePicker { Location = new Point(120, 115), Format = DateTimePickerFormat.Short };
            grpBad.Controls.AddRange(new Control[] { lblReason, txtReason, lblDate, dtpEnd });

            // --- CHECKBOX DEMO DIRTY READ ---
            chkDemoDirty = new CheckBox
            {
                Text = "Demo: Dirty Read (Update rồi Rollback sau 10s)",
                Location = new Point(50, 350),
                AutoSize = true,
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            // --------------------------------

            Button btnSave = new Button { Text = "Lưu Trạng Thái", Location = new Point(150, 400), Size = new Size(200, 50) };
            UIHelper.StyleButton(btnSave, true);
            btnSave.Click += BtnSave_Click;

            rdoGood.CheckedChanged += (s, e) => { if (grpBad != null) grpBad.Enabled = false; };
            rdoBad.CheckedChanged += (s, e) => { if (grpBad != null) grpBad.Enabled = true; };

            if (_currentStatus == "Bảo trì") rdoBad.Checked = true; else rdoGood.Checked = true;

            this.Controls.AddRange(new Control[] { lblTitle, rdoGood, rdoBad, grpBad, chkDemoDirty, btnSave });
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // --- LOGIC DEMO DIRTY READ ---
            if (chkDemoDirty.Checked)
            {
                // Chỉ cho phép chọn Bảo trì để demo lỗi này
                if (!rdoBad.Checked) { MessageBox.Show("Vui lòng chọn 'Cần bảo trì' để chạy Demo này!"); return; }

                this.Enabled = false; // Khóa màn hình
                this.Text = "Đang chạy Demo Dirty Read (Chờ 10s)...";

                await Task.Run(() =>
                {
                    // Gọi Procedure sp_CapNhatBaoTriSan (Có Delay và Rollback)
                    string result = DatabaseHelper.Sp_CapNhatBaoTriSan(_maSan);
                    this.Invoke(new Action(() => MessageBox.Show(result)));
                });

                this.Close();
                return;
            }

            // --- LOGIC BÌNH THƯỜNG ---
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                try
                {
                    string newStatus = rdoGood.Checked ? "Còn trống" : "Bảo trì";
                    string ghiChu = rdoBad.Checked ? $"{txtReason.Text} (Dự kiến xong: {dtpEnd.Value:dd/MM/yyyy})" : "";

                    string sql = "UPDATE SanTheThao SET TinhTrang = @Stt, GhiChu = @Note WHERE MaSan = @Ma";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Stt", newStatus);
                    cmd.Parameters.AddWithValue("@Note", ghiChu);
                    cmd.Parameters.AddWithValue("@Ma", _maSan);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Cập nhật thành công!");
                    this.Close();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }
    }
}