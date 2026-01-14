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
        private Button btnOK;
        private Button btnCancel;
        private Label lblHint;

        public List<ServiceItem> SelectedServices { get; private set; } = new List<ServiceItem>();

        public FormSelectService()
        {
            Text = "Chọn dịch vụ đi kèm";
            Size = new Size(850, 550);
            StartPosition = FormStartPosition.CenterParent;

            InitializeComponent();
            LoadServices();
        }

        private void InitializeComponent()
        {
            BackColor = Color.White;

            // Header hướng dẫn
            lblHint = new Label
            {
                Text = "Hướng dẫn: Tick vào cột 'Chọn' và nhập 'Số lượng'.",
                AutoSize = true,
                Location = new Point(12, 12),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.DimGray
            };

            // GridView
            grid = new DataGridView
            {
                Location = new Point(12, 40),
                Size = new Size(810, 400),
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            // Xử lý sự kiện tick checkbox để cập nhật trạng thái ngay lập tức
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // Tránh crash khi nhập sai kiểu dữ liệu số
            grid.DataError += (s, e) => { e.ThrowException = false; };

            // Buttons
            btnOK = new Button
            {
                Text = "Xác Nhận",
                Size = new Size(120, 40),
                Location = new Point(550, 460),
                BackColor = UIHelper.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "Hủy",
                Size = new Size(120, 40),
                Location = new Point(690, 460),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblHint);
            Controls.Add(grid);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
        }

        private void LoadServices()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Lấy thêm SoLuongTon để kiểm tra
                string sql = @"SELECT MaDichVu, TenDichVu, DonGia, DonViTinh, SoLuongTon 
                               FROM dbo.DichVu 
                               ORDER BY TenDichVu";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.Fill(dt);
            }

            // Thêm cột Checkbox và Số lượng mua
            if (!dt.Columns.Contains("Chon"))
                dt.Columns.Add("Chon", typeof(bool));
            if (!dt.Columns.Contains("SoLuongMua"))
                dt.Columns.Add("SoLuongMua", typeof(int));

            // Set giá trị mặc định
            foreach (DataRow r in dt.Rows)
            {
                r["Chon"] = false;
                r["SoLuongMua"] = 1;
            }

            grid.DataSource = dt;

            // Định dạng cột hiển thị
            grid.Columns["Chon"].HeaderText = "Chọn";
            grid.Columns["Chon"].Width = 50;
            grid.Columns["Chon"].DisplayIndex = 0;

            grid.Columns["MaDichVu"].HeaderText = "Mã DV";
            grid.Columns["MaDichVu"].ReadOnly = true;

            grid.Columns["TenDichVu"].HeaderText = "Tên Dịch Vụ";
            grid.Columns["TenDichVu"].ReadOnly = true;

            grid.Columns["DonGia"].HeaderText = "Đơn Giá";
            grid.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            grid.Columns["DonGia"].ReadOnly = true;

            grid.Columns["DonViTinh"].HeaderText = "ĐVT";
            grid.Columns["DonViTinh"].ReadOnly = true;

            grid.Columns["SoLuongTon"].HeaderText = "Tồn Kho";
            grid.Columns["SoLuongTon"].ReadOnly = true;
            grid.Columns["SoLuongTon"].DefaultCellStyle.ForeColor = Color.Red;

            grid.Columns["SoLuongMua"].HeaderText = "Số Lượng Mua";
            grid.Columns["SoLuongMua"].DefaultCellStyle.BackColor = Color.LightYellow; // Tô màu ô nhập liệu
            grid.Columns["SoLuongMua"].DisplayIndex = 7; // Đẩy về cuối

            // Logic tự động: Nếu là DV_VIP thì số lượng luôn là 1
            grid.CellEndEdit += (s, e) =>
            {
                if (grid.Columns[e.ColumnIndex].Name == "SoLuongMua")
                {
                    var row = grid.Rows[e.RowIndex];
                    string ma = row.Cells["MaDichVu"].Value?.ToString() ?? "";
                    if (string.Equals(ma, "DV_VIP", StringComparison.OrdinalIgnoreCase))
                        row.Cells["SoLuongMua"].Value = 1;
                }
            };
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            var dt = grid.DataSource as DataTable;
            if (dt == null) return;

            var picked = new List<ServiceItem>();

            foreach (DataGridViewRow row in grid.Rows)
            {
                // Kiểm tra có tick chọn không
                bool isSelected = row.Cells["Chon"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["Chon"].Value);

                if (isSelected)
                {
                    // Lấy số lượng mua
                    int qty = 0;
                    if (row.Cells["SoLuongMua"].Value != DBNull.Value)
                        int.TryParse(row.Cells["SoLuongMua"].Value.ToString(), out qty);

                    if (qty <= 0) qty = 1; // Mặc định ít nhất là 1

                    // Validate tồn kho
                    int stock = Convert.ToInt32(row.Cells["SoLuongTon"].Value);
                    string tenDV = row.Cells["TenDichVu"].Value.ToString();

                    if (qty > stock)
                    {
                        MessageBox.Show($"Sản phẩm '{tenDV}' chỉ còn tồn {stock}, bạn không thể mua {qty}!",
                                        "Lỗi kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // Dừng lại không cho lưu
                    }

                    // Logic đặc biệt cho VIP (nếu cần)
                    string maDV = row.Cells["MaDichVu"].Value.ToString();
                    if (string.Equals(maDV, "DV_VIP", StringComparison.OrdinalIgnoreCase)) qty = 1;

                    // Thêm vào list kết quả
                    picked.Add(new ServiceItem
                    {
                        MaDV = maDV,
                        TenDV = tenDV,
                        SoLuong = qty,
                        DonGia = Convert.ToDecimal(row.Cells["DonGia"].Value)
                    });
                }
            }

            if (picked.Count == 0)
            {
                MessageBox.Show("Bạn chưa chọn dịch vụ nào.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectedServices = picked;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}