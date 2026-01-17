using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using VietSportSystem;

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
                RowHeadersVisible = false,
                AutoGenerateColumns = true // Đảm bảo tự động tạo cột từ DataSource
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
                r["SoLuongMua"] = 1; // Giá trị mặc định là 1
            }

            // Đăng ký sự kiện để định dạng cột sau khi binding hoàn tất
            grid.DataBindingComplete += Grid_DataBindingComplete;
            
            grid.DataSource = dt;
        }

        private void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Định dạng cột hiển thị - lưu reference để tránh race condition
            var colChon = grid.Columns.Contains("Chon") ? grid.Columns["Chon"] : null;
            if (colChon != null)
            {
                colChon.HeaderText = "Chọn";
                colChon.Width = 50;
                colChon.DisplayIndex = 0;
            }

            var colMaDV = grid.Columns.Contains("MaDichVu") ? grid.Columns["MaDichVu"] : null;
            if (colMaDV != null)
            {
                colMaDV.HeaderText = "Mã DV";
                colMaDV.ReadOnly = true;
            }

            var colTenDV = grid.Columns.Contains("TenDichVu") ? grid.Columns["TenDichVu"] : null;
            if (colTenDV != null)
            {
                colTenDV.HeaderText = "Tên Dịch Vụ";
                colTenDV.ReadOnly = true;
            }

            var colDonGia = grid.Columns.Contains("DonGia") ? grid.Columns["DonGia"] : null;
            if (colDonGia != null)
            {
                colDonGia.HeaderText = "Đơn Giá";
                colDonGia.DefaultCellStyle.Format = "N0";
                colDonGia.ReadOnly = true;
            }

            var colDonViTinh = grid.Columns.Contains("DonViTinh") ? grid.Columns["DonViTinh"] : null;
            if (colDonViTinh != null)
            {
                colDonViTinh.HeaderText = "ĐVT";
                colDonViTinh.ReadOnly = true;
            }

            var colSoLuongTon = grid.Columns.Contains("SoLuongTon") ? grid.Columns["SoLuongTon"] : null;
            if (colSoLuongTon != null)
            {
                colSoLuongTon.HeaderText = "Tồn Kho";
                colSoLuongTon.ReadOnly = true;
                colSoLuongTon.DefaultCellStyle.ForeColor = Color.Red;
            }

            var colSoLuongMua = grid.Columns.Contains("SoLuongMua") ? grid.Columns["SoLuongMua"] : null;
            if (colSoLuongMua != null)
            {
                colSoLuongMua.HeaderText = "Số Lượng Mua";
                colSoLuongMua.DefaultCellStyle.BackColor = Color.LightYellow; // Tô màu ô nhập liệu
                // Đặt DisplayIndex là cột cuối cùng (số cột - 1)
                colSoLuongMua.DisplayIndex = grid.Columns.Count - 1;
            }

            // Validation: Số lượng mua phải >= 1
            grid.CellValidating += (s, e) =>
            {
                if (grid.Columns[e.ColumnIndex].Name == "SoLuongMua")
                {
                    if (e.FormattedValue != null && !string.IsNullOrWhiteSpace(e.FormattedValue.ToString()))
                    {
                        if (int.TryParse(e.FormattedValue.ToString(), out int qty))
                        {
                            if (qty < 1)
                            {
                                e.Cancel = true;
                                grid.Rows[e.RowIndex].ErrorText = "Số lượng mua phải >= 1";
                            }
                            else
                            {
                                grid.Rows[e.RowIndex].ErrorText = string.Empty;
                            }
                        }
                    }
                }
            };

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

                    // Đảm bảo số lượng >= 1
                    if (qty < 1) qty = 1;

                    // Validate tồn kho
                    int stock = Convert.ToInt32(row.Cells["SoLuongTon"].Value);
                    string tenDV = row.Cells["TenDichVu"].Value.ToString();
                    string maDV = row.Cells["MaDichVu"].Value.ToString();

                    if (qty > stock)
                    {
                        MessageBox.Show($"Sản phẩm '{tenDV}' chỉ còn tồn {stock}, bạn không thể mua {qty}!",
                                        "Lỗi kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // Dừng lại không cho lưu
                    }

                    // Logic đặc biệt cho VIP (nếu cần) - đã xử lý ở trên

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