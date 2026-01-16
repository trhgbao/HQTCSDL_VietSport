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

        public UC_Tech_AllFields()
        {
            InitializeComponent();
            LoadFields();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            Label lblTitle = new Label { Text = "Danh sách sân", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter };

            pnlGrid = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            this.Controls.Add(pnlGrid);
            this.Controls.Add(lblTitle);
        }

        private void LoadFields()
        {
            pnlGrid.Controls.Clear();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Lấy thêm s.GhiChu
                string sql = "SELECT MaSan, LoaiSan, TinhTrang, GhiChu, c.TenCoSo FROM SanTheThao s JOIN CoSo c ON s.MaCoSo = c.MaCoSo";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Xử lý null cho GhiChu
                            string note = reader["GhiChu"] != DBNull.Value ? reader["GhiChu"].ToString() : "";

                            pnlGrid.Controls.Add(CreateCard(
                                reader["MaSan"].ToString(),
                                reader["LoaiSan"].ToString(),
                                reader["TenCoSo"].ToString(),
                                reader["TinhTrang"].ToString(),
                                note // <--- Truyền thêm note vào hàm CreateCard
                            ));
                        }
                    }
                }
            }
        }

        private Panel CreateCard(string maSan, string loaiSan, string coSo, string tinhTrang, string note)
        {
            Panel pnl = new Panel { Size = new Size(300, 250), BackColor = Color.WhiteSmoke, Margin = new Padding(20) };

            // Ảnh (Trên)
            PictureBox pic = new PictureBox { Size = new Size(300, 150), Dock = DockStyle.Top, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Silver };
            string pathImg = Application.StartupPath + $"\\Images\\{maSan}.jpg";
            if (System.IO.File.Exists(pathImg)) pic.Image = Image.FromFile(pathImg);

            // Thông tin (Dưới)
            Label lblInfo = new Label
            {
                Text = $"Tên sân: {maSan} - {loaiSan}\nCơ sở: {coSo}",
                Location = new Point(10, 160),
                Size = new Size(280, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            Label lblStatus = new Label
            {
                Text = "Tình trạng: " + tinhTrang,
                Location = new Point(10, 200),
                Size = new Size(280, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = tinhTrang == "Bảo trì" ? Color.Red : Color.Green
            };

            // Sự kiện Click vào Panel để đổi trạng thái
            pnl.Cursor = Cursors.Hand;
            // Truyền note vào OpenUpdateForm
            pnl.Click += (s, e) => OpenUpdateForm(maSan, tinhTrang, note);
            pic.Click += (s, e) => OpenUpdateForm(maSan, tinhTrang, note);

            // Button đã bỏ theo yêu cầu

            pnl.Controls.AddRange(new Control[] { pic, lblInfo, lblStatus });
            return pnl;
        }

        private void OpenUpdateForm(string maSan, string status, string note)
        {
            // Truyền đủ 3 tham số
            FormUpdateStatus frm = new FormUpdateStatus(maSan, status, note);
            frm.ShowDialog();
            LoadFields();
        }
    }
}