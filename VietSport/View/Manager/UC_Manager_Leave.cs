using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks; // Thêm thư viện để chạy đa luồng
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_Manager_Leave : UserControl
    {
        private DataGridView grid;
        // Thêm 2 control phục vụ demo
        private CheckBox chkLostUpdateDemo;
        private Label lblStatus;

        public UC_Manager_Leave()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;

            // --- PANEL THỐNG KÊ (Giữ nguyên form cũ, thêm nút demo vào đây) ---
            Panel pnlStats = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.WhiteSmoke };

            Label lbl = new Label { Text = "Đơn chờ duyệt: ...\nĐơn đã duyệt: ...", Location = new Point(20, 10), AutoSize = true, Font = new Font("Segoe UI", 11) };
            pnlStats.Controls.Add(lbl);

            // 1. Thêm Checkbox Demo vào Panel cũ
            chkLostUpdateDemo = new CheckBox
            {
                Text = "Demo Lost Update (Tắt khóa - Gây lỗi)",
                Location = new Point(20, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Red
            };
            pnlStats.Controls.Add(chkLostUpdateDemo);

            // 2. Thêm Label trạng thái để biết đang chạy ngầm
            lblStatus = new Label
            {
                Text = "",
                Location = new Point(300, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Blue
            };
            pnlStats.Controls.Add(lblStatus);

            // --- GRID VIEW (Giữ nguyên form cũ) ---
            grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, RowHeadersVisible = false };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.EnableHeadersVisualStyles = false;

            // Thêm nút Duyệt
            DataGridViewButtonColumn btnApprove = new DataGridViewButtonColumn();
            btnApprove.Name = "Approve";
            btnApprove.HeaderText = "Duyệt";
            btnApprove.Text = "✓";
            btnApprove.UseColumnTextForButtonValue = true;

            // Thêm nút Từ chối
            DataGridViewButtonColumn btnReject = new DataGridViewButtonColumn();
            btnReject.Name = "Reject";
            btnReject.HeaderText = "Từ chối";
            btnReject.Text = "X";
            btnReject.UseColumnTextForButtonValue = true;

            grid.Columns.Add(btnApprove);
            grid.Columns.Add(btnReject);

            grid.CellContentClick += Grid_CellContentClick;

            this.Controls.Add(grid);
            this.Controls.Add(pnlStats);
        }

        private void LoadData()
        {
            // Thêm check Invoke để tránh lỗi khi gọi từ luồng khác
            if (this.InvokeRequired) { this.Invoke(new Action(LoadData)); return; }

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT d.MaDon, nv.HoTen, d.LyDo, d.NgayGui, d.TrangThaiDuyet
                                   FROM DonNghiPhep d
                                   JOIN NhanVien nv ON d.MaNhanVien = nv.MaNhanVien";
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    grid.DataSource = dt;
                }
            }
            catch { }
        }

        // Sửa hàm này thành async void để chạy được Task
        private async void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1. Kiểm tra click hợp lệ
            if (e.RowIndex < 0) return;

            string action = "";
            if (grid.Columns[e.ColumnIndex].Name == "Approve") action = "Đã duyệt";
            else if (grid.Columns[e.ColumnIndex].Name == "Reject") action = "Từ chối";

            // 2. Nếu người dùng nhấn nút xử lý
            if (action != "")
            {
                string maDon = grid.Rows[e.RowIndex].Cells["MaDon"].Value.ToString();

                // Hỏi xác nhận (Giữ nguyên logic cũ)
                if (MessageBox.Show($"Bạn chắc chắn muốn chuyển trạng thái thành: {action.ToUpper()}?",
                                    "Xác nhận duyệt", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                // --- LOGIC MỚI: CHẠY ĐA LUỒNG ĐỂ KHÔNG ĐƠ APP ---
                bool isDemoError = chkLostUpdateDemo.Checked;
                lblStatus.Text = $" Đang xử lý đơn {maDon}... (Vui lòng chờ 10s)";
                grid.Enabled = false; // Khóa lưới tạm thời

                try
                {
                    // Chạy task ngầm để gọi SQL (Treo 10s ở dưới nền, UI vẫn sống)
                    await Task.Run(() =>
                    {
                        using (SqlConnection conn = DatabaseHelper.GetConnection())
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("Usp_DuyetNghiPhep", conn);
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@MaDon", maDon);
                            cmd.Parameters.AddWithValue("@TrangThaiDuyet", action);

                            // Nếu tích Checkbox -> Gửi UseLock = 0 (Lỗi), ngược lại là 1 (An toàn)
                            cmd.Parameters.AddWithValue("@UseLock", !isDemoError);

                            cmd.ExecuteNonQuery(); // Lệnh này sẽ treo 10s
                        }
                    });

                    // Sau khi chạy xong 10s
                    MessageBox.Show("Xử lý thành công: " + action);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xử lý: " + ex.Message);
                }
                finally
                {
                    // QUAN TRỌNG: Load lại data để thấy kết quả cuối cùng
                    lblStatus.Text = " Đã xong.";
                    grid.Enabled = true;
                    LoadData();
                }
            }
        }
    }
}
