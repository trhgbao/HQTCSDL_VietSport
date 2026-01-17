using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace VietSportSystem
{
    public class UC_Tech_AllFields : UserControl
    {
        private FlowLayoutPanel pnlGrid;
        private CheckBox chkFixMode;
        
        // Static để chia sẻ trạng thái với UC_Manager_CourtStatus
        // Theo tài liệu 9-10.md:
        // Tick = Fix Mode (READ COMMITTED, chờ T1)
        // Không tick = Demo Error (READ UNCOMMITTED, Dirty Read)
        public static bool IsFixMode { get; private set; } = false;

        public UC_Tech_AllFields()
        {
            InitializeComponent();
            LoadFields();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            
            // Panel header chứa title và checkbox
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.WhiteSmoke };
            
            Label lblTitle = new Label 
            { 
                Text = "Danh sách sân", 
                Font = new Font("Segoe UI", 16, FontStyle.Bold), 
                Location = new Point(20, 10),
                AutoSize = true
            };
            
            // Theo tài liệu 9-10.md:
            // Tick = Fix Mode: Quản lý dùng READ COMMITTED → phải chờ T1
            // Không tick = Demo Error: Quản lý dùng READ UNCOMMITTED → thấy Dirty Data
            chkFixMode = new CheckBox
            {
                Text = "⚠️ Chưa bật Fix Mode (Dirty Read có thể xảy ra)",
                Location = new Point(20, 45),
                AutoSize = true,
                Checked = false, // Mặc định là Demo Error (không tick)
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            chkFixMode.CheckedChanged += (s, e) => {
                IsFixMode = chkFixMode.Checked;
                UpdateCheckboxAppearance();
            };
            
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(chkFixMode);

            pnlGrid = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlHeader);
        }
        
        private void UpdateCheckboxAppearance()
        {
            if (chkFixMode.Checked)
            {
                // Fix Mode: Quản lý sẽ phải chờ, không đọc được Dirty Data
                chkFixMode.Text = "✅ Fix Mode (Quản lý phải chờ, không Dirty Read)";
                chkFixMode.ForeColor = Color.Green;
            }
            else
            {
                // Demo Error: Quản lý sẽ đọc được Dirty Data
                chkFixMode.Text = "⚠️ Chưa bật Fix Mode (Dirty Read có thể xảy ra)";
                chkFixMode.ForeColor = Color.Red;
            }
        }

        private void LoadFields()
        {
            pnlGrid.Controls.Clear();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT MaSan, LoaiSan, TinhTrang, GhiChu, c.TenCoSo FROM SanTheThao s JOIN CoSo c ON s.MaCoSo = c.MaCoSo";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string note = reader["GhiChu"] != DBNull.Value ? reader["GhiChu"].ToString() : "";

                            pnlGrid.Controls.Add(CreateCard(
                                reader["MaSan"].ToString(),
                                reader["LoaiSan"].ToString(),
                                reader["TenCoSo"].ToString(),
                                reader["TinhTrang"].ToString(),
                                note
                            ));
                        }
                    }
                }
            }
        }

        private Panel CreateCard(string maSan, string loaiSan, string coSo, string tinhTrang, string note)
        {
            Panel pnl = new Panel { Size = new Size(300, 250), BackColor = Color.WhiteSmoke, Margin = new Padding(20) };

            PictureBox pic = new PictureBox { Size = new Size(300, 150), Dock = DockStyle.Top, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Silver };
            string pathImg = Application.StartupPath + $"\\Images\\{maSan}.jpg";
            if (System.IO.File.Exists(pathImg)) pic.Image = Image.FromFile(pathImg);

            Label lblInfo = new Label
            {
                Text = $"Tên sân: {maSan} - {loaiSan}\nCơ sở: {coSo}",
                Location = new Point(10, 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            Label lblStatus = new Label
            {
                Text = "Tình trạng: " + tinhTrang,
                Location = new Point(10, 200),
                AutoSize = true,
                ForeColor = tinhTrang == "Bảo trì" ? Color.Red : Color.Green
            };

            pnl.Cursor = Cursors.Hand;
            pnl.Click += (s, e) => OpenUpdateForm(maSan, tinhTrang, note);
            pic.Click += (s, e) => OpenUpdateForm(maSan, tinhTrang, note);

            Button btnDemoConflict = new Button
            {
                Text = "⚡ Bảo trì ngay",
                Size = new Size(100, 30),
                Location = new Point(180, 215),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8)
            };

            btnDemoConflict.Click += (s, e) => {
                try
                {
                    // Luôn gọi sp_CapNhatBaoTriSan (ROLLBACK sau 10s)
                    // để demo Dirty Read scenario
                    lblStatus.Text = "Đang bảo trì... (ROLLBACK sau 10s)";
                    lblStatus.ForeColor = Color.Orange;
                    btnDemoConflict.Enabled = false;
                    Application.DoEvents();
                    
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        
                        // Luôn dùng sp_CapNhatBaoTriSan (UPDATE + WAIT 10s + ROLLBACK)
                        // Mode chỉ ảnh hưởng tới cách Manager đọc data
                        SqlCommand cmd = new SqlCommand("sp_CapNhatBaoTriSan", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 60;
                        cmd.Parameters.AddWithValue("@MaSan", maSan);
                        
                        SqlParameter outParam = new SqlParameter("@KetQua", SqlDbType.NVarChar, 200);
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);
                        
                        cmd.ExecuteNonQuery();
                        
                        // Sau khi SP chạy xong (đã ROLLBACK), refresh lại
                        LoadFields();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                finally
                {
                    btnDemoConflict.Enabled = true;
                }
            };

            pnl.Controls.Add(btnDemoConflict);
            pnl.Controls.AddRange(new Control[] { pic, lblInfo, lblStatus });
            return pnl;
        }

        private void OpenUpdateForm(string maSan, string status, string note)
        {
            FormUpdateStatus frm = new FormUpdateStatus(maSan, status, note);
            frm.ShowDialog();
            LoadFields();
        }
    }
}