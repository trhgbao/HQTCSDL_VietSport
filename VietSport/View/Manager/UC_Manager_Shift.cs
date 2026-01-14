using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_Manager_Shift : UserControl
    {
        private DataGridView grid;
        private TextBox txtMaNV;
        private DateTimePicker dtpNgay;
        private NumericUpDown nudStartHour, nudEndHour;
        private CheckBox chkFix;
        private Button btnAssign, btnReload;
        private Label lblMsg;

        public UC_Manager_Shift()
        {
            InitializeComponent();
            _ = ReloadAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            // ===== TOP: dùng TableLayout để không che grid =====
            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 2
            };
            top.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // hàng input
            top.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // hàng message
            Controls.Add(top);

            // Hàng 1: inputs
            var row1 = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            var lbl1 = new Label
            {
                Text = "Mã nhân viên:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 6, 6, 0)
            };
            txtMaNV = new TextBox
            {
                Width = 120,
                Text = "NV_KT",
                Margin = new Padding(0, 2, 16, 0)
            };

            var lbl2 = new Label
            {
                Text = "Ngày:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 6, 6, 0)
            };
            dtpNgay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Today.AddDays(1),
                Margin = new Padding(0, 2, 16, 0)
            };

            var lbl3 = new Label
            {
                Text = "Giờ:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 6, 6, 0)
            };
            nudStartHour = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 23,
                Width = 60,
                Value = 8,
                Margin = new Padding(0, 2, 6, 0)
            };
            var lblArrow = new Label
            {
                Text = "→",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 6, 6, 0)
            };
            nudEndHour = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 24,
                Width = 60,
                Value = 12,
                Margin = new Padding(0, 2, 16, 0)
            };

            chkFix = new CheckBox
            {
                Text = "Bật FIX",
                AutoSize = true,
                Margin = new Padding(0, 6, 16, 0)
            };

            btnAssign = new Button
            {
                Text = "Gán ca",
                Width = 90,
                Height = 30,
                Margin = new Padding(0, 2, 8, 0)
            };
            btnAssign.Click += async (s, e) => await AssignShiftAsync();

            btnReload = new Button
            {
                Text = "Tải lại",
                Width = 90,
                Height = 30,
                Margin = new Padding(0, 2, 0, 0)
            };
            btnReload.Click += async (s, e) => await ReloadAsync();

            row1.Controls.AddRange(new Control[]
            {
                lbl1, txtMaNV,
                lbl2, dtpNgay,
                lbl3, nudStartHour, lblArrow, nudEndHour,
                chkFix, btnAssign, btnReload
            });

            // Hàng 2: message
            lblMsg = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Margin = new Padding(0, 8, 0, 0),
                Text = ""
            };

            top.Controls.Add(row1, 0, 0);
            top.Controls.Add(lblMsg, 0, 1);

            // ===== GRID =====
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(grid);

            // Quan trọng: grid phải được Add SAU top (đã làm ở trên),
            // và Dock=Fill thì nó sẽ tự nằm dưới top.
            // Nếu vẫn bị che, gọi BringToFront để chắc chắn:
            grid.BringToFront();
        }

        private DateTime BuildStart()
        {
            var d = dtpNgay.Value.Date;
            return new DateTime(d.Year, d.Month, d.Day, (int)nudStartHour.Value, 0, 0);
        }

        private DateTime BuildEnd()
        {
            var d = dtpNgay.Value.Date;
            int endHour = (int)nudEndHour.Value;
            if (endHour >= 24) return new DateTime(d.Year, d.Month, d.Day, 23, 59, 59);
            return new DateTime(d.Year, d.Month, d.Day, endHour, 0, 0);
        }

        private async Task AssignShiftAsync()
        {
            string maNV = txtMaNV.Text.Trim();
            if (string.IsNullOrWhiteSpace(maNV))
            {
                MessageBox.Show("Vui lòng nhập Mã nhân viên.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime bd = BuildStart();
            DateTime kt = BuildEnd();
            if (kt <= bd)
            {
                MessageBox.Show("Giờ kết thúc phải lớn hơn giờ bắt đầu.", "Sai thời gian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool useFix = chkFix.Checked;
            string proc = useFix ? "dbo.sp_FIX08_Manager_AssignShift" : "dbo.sp_ERR08_Manager_AssignShift_BUG";

            string maQL = "NV_QL";
            if (SessionData.CurrentUserID != null && !string.IsNullOrWhiteSpace(SessionData.CurrentUserID.ToString()))
                maQL = SessionData.CurrentUserID.ToString();

            SetBusy(true, "Đang gán ca...");

            try
            {
                string resultMsg = await Task.Run(() =>
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    using (SqlCommand cmd = new SqlCommand(proc, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaNhanVien", maNV);
                        cmd.Parameters.AddWithValue("@MaQuanLy", maQL);
                        cmd.Parameters.AddWithValue("@BatDau", bd);
                        cmd.Parameters.AddWithValue("@KetThuc", kt);
                        cmd.CommandTimeout = 120;

                        conn.Open();
                        object o = cmd.ExecuteScalar();
                        return o?.ToString() ?? "OK";
                    }
                });

                lblMsg.ForeColor = Color.SeaGreen;
                lblMsg.Text = $"✅ {resultMsg}";
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = Color.Firebrick;
                lblMsg.Text = "❌ " + ex.Message;
                MessageBox.Show(ex.Message, "Lỗi gán ca", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, "");
            }
        }

        private async Task ReloadAsync()
        {
            SetBusy(true, "Đang tải danh sách ca...");
            try
            {
                DataTable dt = await Task.Run(() =>
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        string sql = @"
SELECT pc.MaNhanVien, nv.HoTen, pc.MaQuanLy, pc.ThoiGianBatDau, pc.ThoiGianKetThuc
FROM dbo.PhanCongCaTruc pc
JOIN dbo.NhanVien nv ON pc.MaNhanVien = nv.MaNhanVien
ORDER BY pc.ThoiGianBatDau DESC";
                        SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                        DataTable t = new DataTable();
                        da.Fill(t);
                        return t;
                    }
                });
                grid.DataSource = dt;
                lblMsg.ForeColor = Color.DimGray;
                lblMsg.Text = "Đã tải ca trực.";
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = Color.Firebrick;
                lblMsg.Text = "❌ " + ex.Message;
            }
            finally
            {
                SetBusy(false, "");
            }
        }

        private void SetBusy(bool busy, string msg)
        {
            btnAssign.Enabled = !busy;
            btnReload.Enabled = !busy;
            txtMaNV.Enabled = !busy;
            dtpNgay.Enabled = !busy;
            nudStartHour.Enabled = !busy;
            nudEndHour.Enabled = !busy;
            chkFix.Enabled = !busy;

            if (!string.IsNullOrWhiteSpace(msg))
            {
                lblMsg.ForeColor = Color.DimGray;
                lblMsg.Text = msg;
            }
        }
    }
}
