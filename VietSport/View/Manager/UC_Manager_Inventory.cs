using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace VietSportSystem
{
    public class UC_Manager_Inventory : UserControl
    {
        private DataGridView gridDichVu;
        private ComboBox cboDichVu;
        private TextBox txtSoLuong;
        private Button btnNhapKho;
        private CheckBox chkConflictDemo;

        public UC_Manager_Inventory()
        {
            InitializeComponent();
            LoadDichVu();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // Header
            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ KHO DỊCH VỤ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = UIHelper.SecondaryColor,
                ForeColor = Color.White
            };

            // Panel nhập kho (Bên trên)
            Panel pnlNhapKho = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130, // Tăng chiều cao chút
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(20)
            };

            Label lblChonDV = new Label
            {
                Text = "Chọn dịch vụ:",
                Location = new Point(20, 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            cboDichVu = new ComboBox
            {
                Location = new Point(140, 22),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };

            Label lblSoLuong = new Label
            {
                Text = "Số lượng nhập:",
                Location = new Point(480, 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            txtSoLuong = new TextBox
            {
                Location = new Point(610, 22),
                Width = 120,
                Font = new Font("Segoe UI", 11)
            };

            btnNhapKho = new Button
            {
                Text = "NHẬP KHO",
                Location = new Point(760, 18),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            UIHelper.StyleButton(btnNhapKho, true);
            btnNhapKho.Click += BtnNhapKho_Click;

            // Checkbox demo xung đột
            chkConflictDemo = new CheckBox
            {
                Text = "Demo: Gây xung đột Nhập Kho (Scenario 13)",
                Location = new Point(20, 80),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            pnlNhapKho.Controls.AddRange(new Control[] {
                lblChonDV, cboDichVu, lblSoLuong, txtSoLuong, btnNhapKho, chkConflictDemo
            });

            // DataGridView hiển thị danh sách dịch vụ
            gridDichVu = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            // Cấu hình cột
            gridDichVu.Columns.Add("MaDichVu", "Mã DV");
            gridDichVu.Columns.Add("TenDichVu", "Tên Dịch Vụ");
            gridDichVu.Columns.Add("DonGia", "Đơn Giá");
            gridDichVu.Columns.Add("SoLuongTon", "Tồn Kho");

            gridDichVu.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            gridDichVu.Columns["DonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            gridDichVu.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridDichVu.Columns["SoLuongTon"].DefaultCellStyle.ForeColor = Color.Blue;
            gridDichVu.Columns["SoLuongTon"].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            this.Controls.Add(gridDichVu);
            this.Controls.Add(pnlNhapKho);
            this.Controls.Add(lblTitle);
        }

        private void LoadDichVu()
        {
            gridDichVu.Rows.Clear();
            cboDichVu.Items.Clear();

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT MaDichVu, TenDichVu, DonGia, SoLuongTon FROM DichVu ORDER BY TenDichVu";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string maDV = reader["MaDichVu"].ToString();
                                string tenDV = reader["TenDichVu"].ToString();
                                decimal donGia = Convert.ToDecimal(reader["DonGia"]);
                                int tonKho = Convert.ToInt32(reader["SoLuongTon"]);

                                // Thêm vào grid
                                gridDichVu.Rows.Add(maDV, tenDV, donGia, tonKho);

                                // Thêm vào combobox (format: "Mã - Tên")
                                cboDichVu.Items.Add($"{maDV} - {tenDV}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách dịch vụ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNhapKho_Click(object sender, EventArgs e)
        {
            // Kiểm tra đã chọn dịch vụ
            if (cboDichVu.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn dịch vụ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra số lượng
            if (!int.TryParse(txtSoLuong.Text, out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng hợp lệ (số nguyên dương)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoLuong.Focus();
                return;
            }

            try
            {
                // Lấy mã dịch vụ từ ComboBox (format: "Mã - Tên")
                string selectedText = cboDichVu.SelectedItem?.ToString() ?? "";
                if (string.IsNullOrEmpty(selectedText)) return;

                string maDV = selectedText.Split('-')[0].Trim();

                string? msg;
                bool isConflictDemo = chkConflictDemo.Checked;

                if (isConflictDemo)
                {
                    // Dùng SP gây xung đột Lost Update (Tình huống 13)
                    msg = DatabaseHelper.NhapKho_GayXungDot(maDV, soLuong);
                }
                else
                {
                    // Dùng SP chuẩn có UPDLOCK (trả về null khi thành công)
                    msg = DatabaseHelper.NhapKho(maDV, soLuong);
                }

                // Kiểm tra kết quả: msg == null nghĩa là thành công (cho NhapKho chuẩn)
                bool isSuccess = msg == null || msg.Contains("Thành công", StringComparison.OrdinalIgnoreCase);
                bool isDeadlock = msg != null && msg.Contains("Deadlock", StringComparison.OrdinalIgnoreCase);

                if (isSuccess)
                {
                    if (isConflictDemo)
                        MessageBox.Show($"Demo Xung đột - Nhập kho:\n{msg}\n\n⚠️ Lưu ý: Kiểm tra tồn kho trong DB để thấy Lost Update!",
                            "Kết quả (demo xung đột)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show($"Nhập kho thành công!\nĐã thêm {soLuong} đơn vị vào kho.",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Làm mới danh sách (refresh bảng)
                    LoadDichVu();
                    txtSoLuong.Text = "";
                    cboDichVu.SelectedIndex = -1;
                }
                else if (isDeadlock)
                {
                    MessageBox.Show($"Demo Xung đột:\n{msg}\n\n⚠️ Deadlock xảy ra do nhiều giao dịch cùng cập nhật!",
                        "Deadlock (demo xung đột)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (msg != null)
                {
                    // Có lỗi xảy ra
                    MessageBox.Show($"Lỗi nhập kho: {msg}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}