using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem.View.Staff.Receptionist
{
    public class FormSelectService : Form
    {
        private DataGridView grid;
        private Button btnConfirm;

        // List trả về cho form cha
        public List<ServiceItem> SelectedServices { get; private set; } = new List<ServiceItem>();

        public FormSelectService()
        {
            InitializeComponent();
            LoadServices();
        }

        private void InitializeComponent()
        {
            Size = new Size(700, 500);
            Text = "Chọn Dịch Vụ & Sản Phẩm";
            StartPosition = FormStartPosition.CenterParent;

            // Header
            Label lblTitle = new Label { Text = "DANH SÁCH DỊCH VỤ", Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14, FontStyle.Bold) };

            // GridView
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };

            // Cấu hình cột
            grid.Columns.Add("MaDV", "Mã");
            grid.Columns.Add("TenDV", "Tên Dịch Vụ");
            grid.Columns.Add("DonGia", "Đơn Giá");
            grid.Columns.Add("TonKho", "Tồn Kho");

            // Cột nhập số lượng (Cho phép sửa)
            DataGridViewTextBoxColumn colQty = new DataGridViewTextBoxColumn();
            colQty.Name = "SoLuong";
            colQty.HeaderText = "SL Mua";
            colQty.DefaultCellStyle.BackColor = Color.LightYellow; // Tô màu để biết ô này nhập được
            grid.Columns.Add(colQty);

            // Chỉ cho phép sửa cột Số lượng
            grid.ReadOnly = false;
            grid.Columns["MaDV"].ReadOnly = true;
            grid.Columns["TenDV"].ReadOnly = true;
            grid.Columns["DonGia"].ReadOnly = true;
            grid.Columns["TonKho"].ReadOnly = true;

            // Button Confirm
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60 };
            btnConfirm = new Button { Text = "Xác Nhận Chọn", Size = new Size(150, 40), Location = new Point(275, 10), BackColor = UIHelper.PrimaryColor, ForeColor = Color.White };
            btnConfirm.Click += BtnConfirm_Click;
            pnlBottom.Controls.Add(btnConfirm);

            Controls.Add(grid);
            Controls.Add(pnlBottom);
            Controls.Add(lblTitle);
        }

        private void LoadServices()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT MaDichVu, TenDichVu, DonGia, SoLuongTon FROM DichVu";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    grid.Rows.Add(
                        reader["MaDichVu"],
                        reader["TenDichVu"],
                        Convert.ToDecimal(reader["DonGia"]).ToString("N0"),
                        reader["SoLuongTon"],
                        "0" // Mặc định số lượng mua là 0
                    );
                }
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            SelectedServices.Clear();

            foreach (DataGridViewRow row in grid.Rows)
            {
                // Lấy số lượng người dùng nhập
                string sQty = row.Cells["SoLuong"].Value?.ToString();
                int qty = 0;
                int.TryParse(sQty, out qty);

                if (qty > 0)
                {
                    // Validate tồn kho
                    int stock = Convert.ToInt32(row.Cells["TonKho"].Value);
                    if (qty > stock)
                    {
                        MessageBox.Show($"Sản phẩm '{row.Cells["TenDV"].Value}' chỉ còn {stock}, bạn không thể mua {qty}!", "Lỗi kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Add vào list
                    SelectedServices.Add(new ServiceItem
                    {
                        MaDV = row.Cells["MaDV"].Value.ToString(),
                        TenDV = row.Cells["TenDV"].Value.ToString(),
                        DonGia = decimal.Parse(row.Cells["DonGia"].Value.ToString().Replace(".", "").Replace(",", "")), // Parse lại tiền tệ
                        SoLuong = qty
                    });
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}