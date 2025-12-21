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
            if (e.RowIndex < 0) return;

            string maDon = grid.Rows[e.RowIndex].Cells["MaDon"].Value.ToString();
            string action = "";

            if (grid.Columns[e.ColumnIndex].Name == "Approve") action = "Đã duyệt";
            else if (grid.Columns[e.ColumnIndex].Name == "Reject") action = "Từ chối";

            if (action != "")
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE DonNghiPhep SET TrangThaiDuyet = @Stt WHERE MaDon = @Ma", conn);
                    cmd.Parameters.AddWithValue("@Stt", action);
                    cmd.Parameters.AddWithValue("@Ma", maDon);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Đã cập nhật trạng thái: " + action);
                    LoadData(); // Load lại
                }
            }
        }
    }
}