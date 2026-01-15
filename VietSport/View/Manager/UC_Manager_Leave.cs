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
        private CheckBox chkLostUpdateDemo; // Declare the missing CheckBox

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

            // Initialize the CheckBox
            chkLostUpdateDemo = new CheckBox
            {
                Text = "Demo Lost Update (Tình huống 12)",
                Location = new Point(20, 150),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UIHelper.SecondaryColor
            };
            pnlStats.Controls.Add(chkLostUpdateDemo);

            grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, RowHeadersVisible = false };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.EnableHeadersVisualStyles = false;

            // Add Approve button
            DataGridViewButtonColumn btnApprove = new DataGridViewButtonColumn();
            btnApprove.Name = "Approve";
            btnApprove.HeaderText = "Duyệt";
            btnApprove.Text = "✓";
            btnApprove.UseColumnTextForButtonValue = true;

            // Add Reject button
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

            string action = "";
            if (grid.Columns[e.ColumnIndex].Name == "Approve") action = "Đã duyệt";
            else if (grid.Columns[e.ColumnIndex].Name == "Reject") action = "Từ chối";

            if (action != "")
            {
                string maDon = grid.Rows[e.RowIndex].Cells["MaDon"].Value.ToString();

                if (MessageBox.Show($"Bạn chắc chắn muốn chuyển trạng thái thành: {action.ToUpper()}?",
                                    "Xác nhận duyệt", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    try
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand("Usp_DuyetNghiPhep", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UseLock", !chkLostUpdateDemo.Checked);
                        cmd.Parameters.AddWithValue("@MaDon", maDon);
                        cmd.Parameters.AddWithValue("@TrangThaiDuyet", action);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Xử lý thành công: " + action);
                        LoadData();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 1205)
                        {
                            MessageBox.Show("Xung đột Deadlock! Hệ thống đã tự động hủy giao dịch này.\n\nVui lòng thử lại sau vài giây.",
                                            "Lỗi Deadlock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Lỗi SQL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
