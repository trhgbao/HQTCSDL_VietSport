using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VietSportSystem
{
    public class UC_Tech_Maintenance : UserControl
    {
        private FlowLayoutPanel pnlGrid;

        public UC_Tech_Maintenance()
        {
            InitializeComponent();
            LoadBadFields();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            Label lblTitle = new Label { Text = "Danh sách sân cần bảo trì", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter };
            pnlGrid = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
            this.Controls.Add(pnlGrid);
            this.Controls.Add(lblTitle);
        }

        private void LoadBadFields()
        {
            pnlGrid.Controls.Clear();
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // --- SỬA CÂU SQL NÀY: Chỉ lấy từ SanTheThao, không JOIN nữa ---
                    string sql = @"
                SELECT MaSan, LoaiSan, GhiChu 
                FROM SanTheThao 
                WHERE TinhTrang = N'Bảo trì'";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        // Kiểm tra null cho cột GhiChu để tránh lỗi
                        string ghiChu = reader["GhiChu"] != DBNull.Value ? reader["GhiChu"].ToString() : "Không có mô tả";

                        pnlGrid.Controls.Add(CreateDetailCard(
                            reader["MaSan"].ToString(),
                            reader["LoaiSan"].ToString(),
                            ghiChu,
                            "Xem trong ghi chú" // Vì không có cột ngày riêng
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu bảo trì: " + ex.Message);
            }
        }

        private Panel CreateDetailCard(string maSan, string loaiSan, string loi, string ngayKet)
        {
            Panel pnl = new Panel { Size = new Size(600, 200), BackColor = Color.LightGray, Margin = new Padding(20) };

            // Chia đôi: Trái (Ảnh) - Phải (Thông tin)
            PictureBox pic = new PictureBox { Size = new Size(250, 200), Dock = DockStyle.Left, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Silver };
            string pathImg = Application.StartupPath + $"\\Images\\{maSan}.jpg";
            if (System.IO.File.Exists(pathImg)) pic.Image = Image.FromFile(pathImg);

            Panel pnlInfo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblTitle = new Label { Text = $"Tên sân: {maSan} - {loaiSan}", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };
            Label lblDes = new Label { Text = $"Mô tả: {loi}", Dock = DockStyle.Top, Height = 60 };
            Label lblDate = new Label
            {
                Text = $"Ghi chú: {ngayKet}", // Hiển thị nguyên văn text
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            pnlInfo.Controls.AddRange(new Control[] { lblDate, lblDes, lblTitle });

            // Click để sửa trạng thái
            pnl.Cursor = Cursors.Hand;
            pnl.Click += (s, e) => {
                FormUpdateStatus frm = new FormUpdateStatus(maSan, "Bảo trì", loi);
                frm.ShowDialog();
                LoadBadFields();
            };

            pnl.Controls.Add(pnlInfo);
            pnl.Controls.Add(pic);
            return pnl;
        }
    }
}