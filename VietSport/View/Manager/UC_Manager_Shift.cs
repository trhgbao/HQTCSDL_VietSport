using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace VietSportSystem
{
    public class UC_Manager_Shift : UserControl
    {
        private DataGridView grid;
        public UC_Manager_Shift()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            Panel pnlStats = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.WhiteSmoke };
            Label lbl = new Label { Text = "Tổng nhân viên đi làm hôm nay: ...", Location = new Point(20, 20), Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true };
            pnlStats.Controls.Add(lbl);

            grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, BackgroundColor = Color.White };
            // Style đen như hình
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.EnableHeadersVisualStyles = false;

            this.Controls.Add(grid);
            this.Controls.Add(pnlStats);
        }

        private void LoadData()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Join bảng Ca trực và Nhân viên
                string sql = @"SELECT nv.HoTen AS 'Nhân viên', pc.ThoiGianBatDau, pc.ThoiGianKetThuc
                               FROM PhanCongCaTruc pc
                               JOIN NhanVien nv ON pc.MaNhanVien = nv.MaNhanVien";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
            }
        }
    }
}