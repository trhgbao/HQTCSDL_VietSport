using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    /// <summary>
    /// Màn hình Quản trị cập nhật chính sách giảm giá - dùng cho Scenario 9 T1
    /// </summary>
    public class UC_Admin_DiscountPolicy : UserControl
    {
        private DataGridView gridPolicy;
        private ComboBox cboHangTV;
        private TextBox txtDiscount;
        private CheckBox chkDemoMode;
        private Button btnSave;
        private Label lblResult;

        public UC_Admin_DiscountPolicy()
        {
            InitializeComponent();
            LoadPolicy();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // Title
            Label lblTitle = new Label
            {
                Text = "CHÍNH SÁCH GIẢM GIÁ THEO HẠNG THÀNH VIÊN",
                Font = UIHelper.HeaderFont,
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = UIHelper.SecondaryColor,
                ForeColor = Color.White
            };

            // Panel input
            Panel pnlInput = new Panel { Height = 120, Dock = DockStyle.Top, Padding = new Padding(20) };

            Label lblHang = new Label { Text = "Hạng thành viên:", Location = new Point(20, 25), AutoSize = true };
            cboHangTV = new ComboBox
            {
                Location = new Point(140, 22),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboHangTV.Items.AddRange(new object[] { "Standard", "Silver", "Gold", "Platinum" });
            cboHangTV.SelectedIndex = 0;

            Label lblDiscount = new Label { Text = "Mức giảm (%):", Location = new Point(320, 25), AutoSize = true };
            txtDiscount = new TextBox { Location = new Point(420, 22), Width = 80 };
            txtDiscount.Text = "0";

            btnSave = new Button { Text = "Cập nhật", Location = new Point(520, 18), Size = new Size(120, 35) };
            UIHelper.StyleButton(btnSave, true);
            btnSave.Click += BtnSave_Click;

            lblResult = new Label { Text = "", Location = new Point(20, 70), AutoSize = true, ForeColor = Color.DarkGreen };

            pnlInput.Controls.AddRange(new Control[] { lblHang, cboHangTV, lblDiscount, txtDiscount, btnSave, lblResult });

            // Grid
            gridPolicy = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false
            };

            this.Controls.Add(gridPolicy);
            this.Controls.Add(pnlInput);
            this.Controls.Add(lblTitle);
        }

        private void LoadPolicy()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT HangTV, GiamGia, NgayCapNhat FROM ChinhSachGiamGia ORDER BY CASE HangTV WHEN 'Standard' THEN 1 WHEN 'Silver' THEN 2 WHEN 'Gold' THEN 3 WHEN 'Platinum' THEN 4 END";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    gridPolicy.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi load: " + ex.Message;
                lblResult.ForeColor = Color.Red;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cboHangTV.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn hạng thành viên!");
                return;
            }

            string hangTV = cboHangTV.SelectedItem.ToString();
            if (!decimal.TryParse(txtDiscount.Text, out decimal mucGiam) || mucGiam < 0 || mucGiam > 100)
            {
                MessageBox.Show("Mức giảm phải là số từ 0-100!");
                return;
            }

            // Normal update - instant (no delay)
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        IF EXISTS (SELECT 1 FROM ChinhSachGiamGia WHERE HangTV = @Hang)
                            UPDATE ChinhSachGiamGia SET GiamGia = @Giam, NgayCapNhat = GETDATE() WHERE HangTV = @Hang
                        ELSE
                            INSERT INTO ChinhSachGiamGia (HangTV, GiamGia, NgayCapNhat) VALUES (@Hang, @Giam, GETDATE())";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Hang", hangTV);
                    cmd.Parameters.AddWithValue("@Giam", mucGiam);
                    cmd.ExecuteNonQuery();

                    lblResult.Text = $"Đã cập nhật giảm giá {hangTV} = {mucGiam}%";
                    lblResult.ForeColor = Color.DarkGreen;
                    LoadPolicy();
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
