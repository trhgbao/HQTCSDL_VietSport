using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class FormFindCustomer : Form
    {
        public string SelectedMaKH { get; private set; }
        public string SelectedTenKH { get; private set; }
        public string SelectedSDT { get; private set; }

        public string SelectedCMND { get; private set; }

        private DataGridView grid;
        private TextBox txtSearch;

        public FormFindCustomer()
        {
            this.Size = new Size(800, 500);
            this.Text = "Tìm tên tài khoản";
            this.StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            txtSearch = new TextBox { Location = new Point(20, 15), Width = 300, Font = new Font("Segoe UI", 12) };
            txtSearch.PlaceholderText = "Nhập tên hoặc SĐT...";

            Button btnSearch = new Button { Text = "Tìm kiếm", Location = new Point(330, 14), Size = new Size(100, 30) };
            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);

            pnlTop.Controls.AddRange(new Control[] { txtSearch, btnSearch });

            grid = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            // Style đen như hình
            grid.BackgroundColor = Color.FromArgb(40, 40, 40);
            grid.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            grid.DefaultCellStyle.ForeColor = Color.White;
            grid.CellDoubleClick += Grid_CellDoubleClick;

            this.Controls.Add(grid);
            this.Controls.Add(pnlTop);

            LoadData("");
        }

        private void LoadData(string keyword)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT MaKhachHang, HoTen, SoDienThoai, CMND, Email FROM KhachHang WHERE HoTen LIKE @Key OR SoDienThoai LIKE @Key";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@Key", "%" + keyword + "%");
                DataTable dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
            }
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                SelectedMaKH = grid.Rows[e.RowIndex].Cells["MaKhachHang"].Value.ToString();
                SelectedTenKH = grid.Rows[e.RowIndex].Cells["HoTen"].Value.ToString();
                SelectedSDT = grid.Rows[e.RowIndex].Cells["SoDienThoai"].Value.ToString();
                SelectedCMND = grid.Rows[e.RowIndex].Cells["CMND"].Value.ToString();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}