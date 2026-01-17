using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    /// <summary>
    /// Màn hình Quản lý xem trạng thái sân - dùng cho Scenario 10 T2
    /// Theo tài liệu 9-10.md:
    /// Tick "Fix Mode" = READ COMMITTED → phải chờ T1, thấy data cũ (không Dirty Read)
    /// Không tick = READ UNCOMMITTED → thấy Dirty Data
    /// </summary>
    public class UC_Manager_CourtStatus : UserControl
    {
        private ComboBox cboCoSo;
        private DataGridView gridSan;
        private CheckBox chkFixMode;
        private Button btnRefresh;

        public UC_Manager_CourtStatus()
        {
            InitializeComponent();
            LoadCoSo();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // Title
            Label lblTitle = new Label
            {
                Text = "TRẠNG THÁI SÂN THỂ THAO",
                Font = UIHelper.HeaderFont,
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = UIHelper.SecondaryColor,
                ForeColor = Color.White
            };

            // Panel chứa controls
            Panel pnlControls = new Panel { Height = 100, Dock = DockStyle.Top, Padding = new Padding(20) };

            Label lblCoSo = new Label { Text = "Chọn cơ sở:", Location = new Point(20, 25), AutoSize = true };
            cboCoSo = new ComboBox { Location = new Point(100, 22), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // Checkbox Fix Mode - theo tài liệu 9-10.md
            // Tick = Fix Mode: READ COMMITTED → phải chờ T1
            // Không tick = Demo Error: READ UNCOMMITTED → thấy Dirty Data
            chkFixMode = new CheckBox
            {
                Text = "Fix Mode (READ COMMITTED - chờ T1)",
                Location = new Point(320, 25),
                AutoSize = true,
                Checked = false, // Mặc định là Demo Error
                ForeColor = Color.Blue,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            btnRefresh = new Button { Text = "Xem trạng thái sân", Location = new Point(20, 60), Size = new Size(150, 35) };
            UIHelper.StyleButton(btnRefresh, true);
            btnRefresh.Click += BtnRefresh_Click;

            pnlControls.Controls.AddRange(new Control[] { lblCoSo, cboCoSo, chkFixMode, btnRefresh });

            // Grid hiển thị sân
            gridSan = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false
            };

            this.Controls.Add(gridSan);
            this.Controls.Add(pnlControls);
            this.Controls.Add(lblTitle);
        }

        private void LoadCoSo()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT MaCoSo, TenCoSo FROM CoSo", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(reader);

                cboCoSo.DisplayMember = "TenCoSo";
                cboCoSo.ValueMember = "MaCoSo";
                cboCoSo.DataSource = dt;
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (cboCoSo.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn cơ sở!");
                return;
            }

            string maCoSo = cboCoSo.SelectedValue.ToString();
            
            // Đọc trạng thái Fix Mode từ checkbox
            // Tick (isFixMode = true) = Fix Mode → READ COMMITTED → phải chờ
            // Không tick (isFixMode = false) = Demo Error → READ UNCOMMITTED → thấy Dirty Data
            bool isFixMode = chkFixMode.Checked;

            // Hiển thị trạng thái đang chờ
            btnRefresh.Enabled = false;
            if (isFixMode)
            {
                btnRefresh.Text = "Đang chờ T1 (READ COMMITTED)...";
            }
            else
            {
                btnRefresh.Text = "Đang đọc (READ UNCOMMITTED)...";
            }
            Application.DoEvents();

            try
            {
                // Tick (Fix Mode): sp_XemThongTinSan_AnToan (READ COMMITTED) → phải chờ T1 xong
                // Không tick (Demo Error): sp_XemThongTinSan_CoLoi (READ UNCOMMITTED) → thấy Dirty Data
                string spName = isFixMode ? "sp_XemThongTinSan_AnToan" : "sp_XemThongTinSan_CoLoi";

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(spName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 60;
                        cmd.Parameters.AddWithValue("@MaCoSo", maCoSo);

                        SqlParameter outParam = new SqlParameter("@KetQua", SqlDbType.NVarChar, 500)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outParam);

                        // Execute and load data into grid
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        gridSan.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
                btnRefresh.Text = "Xem trạng thái sân";
            }
        }
    }
}
