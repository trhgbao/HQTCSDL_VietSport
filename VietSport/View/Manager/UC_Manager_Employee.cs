using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace VietSportSystem
{
    public class UC_Manager_Employee : UserControl
    {
        private DataGridView grid;
        public UC_Manager_Employee()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            // 1. Panel Thống kê (Trên cùng)
            Panel pnlStats = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.WhiteSmoke };
            Label lblStats = new Label { Text = "Tổng số nhân viên: ... \nĐang làm việc: ... \nSắp hết hợp đồng: 0", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 11) };
            pnlStats.Controls.Add(lblStats);

            // 2. Grid
            grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
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
                // Query lấy list nhân viên
                SqlDataAdapter da = new SqlDataAdapter("SELECT HoTen, ChucVu, SoDienThoai, CMND, GioiTinh FROM NhanVien", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
            }
        }
    }
}