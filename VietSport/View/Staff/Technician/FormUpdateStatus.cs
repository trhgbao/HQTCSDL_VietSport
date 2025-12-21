using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VietSportSystem
{
    public class FormUpdateStatus : Form
    {
        private string _maSan;
        private string _currentStatus;
        private string _currentNote;
        private RadioButton rdoGood, rdoBad;
        private TextBox txtReason;
        private DateTimePicker dtpEnd;
        private GroupBox grpBad;

        public FormUpdateStatus(string maSan, string currentStatus, string currentNote)
        {
            _maSan = maSan;
            _currentStatus = currentStatus;
            _currentNote = currentNote;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 450);
            this.Text = "Cập nhật trạng thái sân: " + _maSan;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label { Text = "CHỌN TRẠNG THÁI", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(150, 20), AutoSize = true };

            // 1. KHỞI TẠO CÁC CONTROL TRƯỚC (QUAN TRỌNG: Phải tạo trước khi dùng)
            rdoGood = new RadioButton { Text = "Hoạt động tốt (Sẵn sàng)", Location = new Point(50, 70), AutoSize = true, Font = new Font("Segoe UI", 11) };
            rdoBad = new RadioButton { Text = "Cần bảo trì / Sửa chữa", Location = new Point(50, 110), AutoSize = true, Font = new Font("Segoe UI", 11) };

            // Tạo GroupBox trước khi gán sự kiện cho RadioButton
            grpBad = new GroupBox { Text = "Thông tin hỏng hóc", Location = new Point(50, 150), Size = new Size(400, 180), Enabled = false };

            Label lblReason = new Label { Text = "Mô tả lỗi:", Location = new Point(20, 30) };
            txtReason = new TextBox { Location = new Point(20, 50), Width = 350, Height = 60, Multiline = true, BorderStyle = BorderStyle.FixedSingle };
            txtReason.Text = _currentNote;

            Label lblDate = new Label { Text = "Dự kiến xong:", Location = new Point(20, 120) };
            dtpEnd = new DateTimePicker { Location = new Point(120, 115), Format = DateTimePickerFormat.Short };

            grpBad.Controls.AddRange(new Control[] { lblReason, txtReason, lblDate, dtpEnd });

            Button btnSave = new Button { Text = "Lưu Trạng Thái", Location = new Point(150, 350), Size = new Size(200, 50) };
            UIHelper.StyleButton(btnSave, true);
            btnSave.Click += BtnSave_Click;

            // 2. GÁN SỰ KIỆN VÀ LOGIC ẨN HIỆN (Sau khi đã tạo hết các biến)
            rdoGood.CheckedChanged += (s, e) => { if (grpBad != null) grpBad.Enabled = false; };
            rdoBad.CheckedChanged += (s, e) => { if (grpBad != null) grpBad.Enabled = true; };

            // Set giá trị mặc định
            if (_currentStatus == "Bảo trì")
            {
                rdoBad.Checked = true;
                grpBad.Enabled = true;
            }
            else
                rdoGood.Checked = true;

            // 3. THÊM VÀO FORM
            this.Controls.AddRange(new Control[] { lblTitle, rdoGood, rdoBad, grpBad, btnSave });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                try
                {
                    string newStatus = rdoGood.Checked ? "Còn trống" : "Bảo trì";
                    string ghiChu = "";

                    if (rdoBad.Checked)
                    {
                        // Nếu bảo trì: Ghi chú = Lý do + Ngày dự kiến
                        ghiChu = $"{txtReason.Text} (Dự kiến xong: {dtpEnd.Value:dd/MM/yyyy})";
                    }
                    else
                    {
                        // Nếu tốt: Xóa ghi chú
                        ghiChu = "";
                    }

                    // --- CÂU SQL MỚI: Chỉ Update bảng SanTheThao ---
                    string sql = "UPDATE SanTheThao SET TinhTrang = @Stt, GhiChu = @Note WHERE MaSan = @Ma";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Stt", newStatus);
                    cmd.Parameters.AddWithValue("@Note", ghiChu);
                    cmd.Parameters.AddWithValue("@Ma", _maSan);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Cập nhật thành công!");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
    }
}