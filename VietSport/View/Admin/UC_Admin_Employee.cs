using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VietSportSystem
{
    public class UC_Admin_Employee : UserControl
    {
        private FlowLayoutPanel pnlList;

        public UC_Admin_Employee()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            Label lblTitle = new Label { Text = "Danh sách nhân viên", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter };
            pnlList = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            this.Controls.Add(pnlList);
            this.Controls.Add(lblTitle);
        }

        private void LoadData()
        {
            pnlList.Controls.Clear();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Lấy cả thông tin lương (Join bảng LuongNhanVien - lấy tháng mới nhất)
                string sql = @"
            SELECT nv.MaNhanVien, nv.HoTen, nv.NgaySinh, nv.GioiTinh, nv.CMND, nv.SoDienThoai, nv.ChucVu, nv.MaCoSo,
                   ISNULL(l.LuongCoBan, 0) as LuongCoBan
            FROM NhanVien nv
            LEFT JOIN LuongNhanVien l ON nv.MaNhanVien = l.MaNhanVien 
            AND l.ThangNam = (SELECT MAX(ThangNam) FROM LuongNhanVien WHERE MaNhanVien = nv.MaNhanVien)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pnlList.Controls.Add(CreateEmpCard(
                                reader["HoTen"].ToString(),
                                DateTime.Parse(reader["NgaySinh"].ToString()), // Cần đảm bảo DB ko null hoặc try-catch
                                reader["GioiTinh"].ToString(),
                                "KTX Khu B", // Địa chỉ (Demo vì bảng NV chưa có cột DiaChi)
                                reader["SoDienThoai"].ToString(),
                                reader["CMND"].ToString(),
                                reader["ChucVu"].ToString(),
                                reader["MaCoSo"].ToString(),
                                reader["MaNhanVien"].ToString(),
                                Convert.ToDecimal(reader["LuongCoBan"])
                            ));
                        }
                    }
                }
            }
        }

        private Panel CreateEmpCard(string name, DateTime dob, string gender, string addr, string phone, string cccd, string role, string branch, string maNV, decimal salary)
        {
            Panel pnl = new Panel { Size = new Size(350, 400), BackColor = Color.WhiteSmoke, Margin = new Padding(20), BorderStyle = BorderStyle.FixedSingle };

            // Ảnh
            PictureBox pic = new PictureBox { Size = new Size(100, 100), Location = new Point(125, 10), BackColor = Color.LightGray, SizeMode = PictureBoxSizeMode.Zoom };

            string pathImg = System.Windows.Forms.Application.StartupPath + $"\\Images\\{maNV}.jpg";

            if (System.IO.File.Exists(pathImg))
                pic.Image = Image.FromFile(pathImg);

            // Tên
            Label lblName = new Label { Text = "Tên: " + name, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 120), AutoSize = true };

            // Các dòng thông tin (Vẽ các dòng kẻ ngang giả lập bằng Label border)
            int y = 160;
            pnl.Controls.AddRange(new Control[] {
                CreateLine($"Ngày sinh: {dob:dd/MM/yyyy}", $"Giới tính: {gender}", y),
                CreateLine($"Địa chỉ: {addr}", "", y + 40),
                CreateLine($"Sđt: {phone}", $"CCCD: {cccd}", y + 80),
                CreateLine($"Chức vụ: {role}", $"Mã CS: {branch}", y + 120),
                CreateLine($"Lương cơ bản: {salary:N0} đ", "", y + 160)
            });

            pnl.Controls.Add(pic);
            pnl.Controls.Add(lblName);
            return pnl;
        }

        // Helper tạo 1 dòng có 2 cột thông tin
        private Panel CreateLine(string text1, string text2, int y)
        {
            Panel p = new Panel { Location = new Point(10, y), Size = new Size(330, 35), BackColor = Color.LightGray };
            Label l1 = new Label { Text = text1, Location = new Point(5, 8), AutoSize = true, Font = new Font("Segoe UI", 9) };
            p.Controls.Add(l1);

            if (!string.IsNullOrEmpty(text2))
            {
                // Vẽ vạch ngăn giữa
                Label line = new Label { Location = new Point(165, 0), Width = 1, Height = 35, BackColor = Color.Black };
                Label l2 = new Label { Text = text2, Location = new Point(170, 8), AutoSize = true, Font = new Font("Segoe UI", 9) };
                p.Controls.Add(line);
                p.Controls.Add(l2);
            }
            return p;
        }
    }
}