using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace VietSportSystem
{
    public class UC_Manager_Leave : UserControl
    {
        private DataGridView grid;
        public UC_Manager_Leave()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            Panel pnlStats = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.WhiteSmoke };
            Label lbl = new Label { Text = "Đơn chờ duyệt: ...\nĐơn đã duyệt: ...", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 11) };
            pnlStats.Controls.Add(lbl);

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

        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1. Kiểm tra click hợp lệ (không click vào header)
            if (e.RowIndex < 0) return;

            string action = "";
            // Xác định hành động dựa trên tên cột
            if (grid.Columns[e.ColumnIndex].Name == "Approve") action = "Đã duyệt";
            else if (grid.Columns[e.ColumnIndex].Name == "Reject") action = "Từ chối";

            // 2. Nếu người dùng đã nhấn nút xử lý
            if (action != "")
            {
                // Lấy Mã đơn từ dòng hiện tại
                string maDon = grid.Rows[e.RowIndex].Cells["MaDon"].Value.ToString();

                // Hỏi xác nhận lần cuối
                if (MessageBox.Show($"Bạn chắc chắn muốn chuyển trạng thái thành: {action.ToUpper()}?",
                                    "Xác nhận duyệt", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    try
                    {
                        conn.Open();

                        // ================================================================
                        // CHÈN PROCEDURE CỦA BẠN Ở ĐÂY
                        // ================================================================
                        SqlCommand cmd = new SqlCommand("Usp_DuyetNghiPhep", conn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Truyền tham số cho Procedure
                        cmd.Parameters.AddWithValue("@MaDon", maDon);
                        cmd.Parameters.AddWithValue("@TrangThaiDuyet", action); // 'Đã duyệt' hoặc 'Từ chối'

                        // Thực thi
                        cmd.ExecuteNonQuery();

                        // Thông báo thành công
                        MessageBox.Show("Xử lý thành công: " + action);

                        // Load lại danh sách để cập nhật giao diện
                        LoadData();
                    }
                    catch (SqlException ex)
                    {
                        // Bắt lỗi từ SQL (ví dụ: Deadlock hoặc lỗi do bạn RAISERROR)
                        MessageBox.Show("Lỗi xử lý (SQL): " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hệ thống: " + ex.Message);
                    }
                }
            }
        }
    }
}