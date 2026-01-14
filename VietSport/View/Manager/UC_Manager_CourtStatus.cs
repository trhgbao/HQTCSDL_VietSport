using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    /// <summary>
    /// Màn hình Quản lý xem trạng thái sân - dùng cho Scenario 10 T2
    /// </summary>
    public class UC_Manager_CourtStatus : UserControl
    {
        private ComboBox cboCoSo;
        private DataGridView gridSan;
        private CheckBox chkFixMode;
        private Button btnRefresh;
        private Label lblResult;

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

            chkFixMode = new CheckBox
            {
                Text = "Fix Mode (Read Committed - Chờ T1 commit)",
                Location = new Point(320, 25),
                AutoSize = true,
                ForeColor = Color.Blue,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            btnRefresh = new Button { Text = "Xem trạng thái sân", Location = new Point(600, 20), Size = new Size(150, 35) };
            UIHelper.StyleButton(btnRefresh, true);
            btnRefresh.Click += BtnRefresh_Click;

            lblResult = new Label { Text = "", Location = new Point(20, 65), AutoSize = true, ForeColor = Color.DarkGreen };

            pnlControls.Controls.AddRange(new Control[] { lblCoSo, cboCoSo, chkFixMode, btnRefresh, lblResult });

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
            bool isFixMode = chkFixMode.Checked;

            lblResult.Text = "Đang tải dữ liệu...";
            lblResult.ForeColor = Color.Orange;
            Application.DoEvents();

            try
            {
                // Call SP based on mode
                string spName = isFixMode ? "sp_XemThongTinSan_DaFix" : "sp_XemThongTinSan_CoLoi";

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(spName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 60; // Wait up to 60s for T1 to finish
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

                        string ketQua = outParam.Value?.ToString() ?? "";
                        lblResult.Text = ketQua;
                        lblResult.ForeColor = Color.DarkGreen;
                    }
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi: " + ex.Message;
                lblResult.ForeColor = Color.Red;
            }
        }
    }
}
