using System;
using System.Drawing;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;

namespace VietSportSystem
{
    public class UC_SanDetail : UserControl
    {
        private MainForm _mainForm;
        private string _maSan;
        private SanInfo _info; // Lưu thông tin sân lấy được

        public UC_SanDetail(MainForm main, string maSan)
        {
            _mainForm = main;
            _maSan = maSan;
            InitializeComponent();
            LoadSanDetailFromDB();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            // Nút quay lại
            Button btnBack = new Button { Text = "← Quay lại", Location = new Point(20, 20), Size = new Size(100, 40) };
            UIHelper.StyleButton(btnBack, false);
            btnBack.Click += (s, e) => _mainForm.LoadView(new UC_HomeBanner(_mainForm));
            this.Controls.Add(btnBack);
        }

        private void LoadSanDetailFromDB()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT s.MaSan, s.LoaiSan, s.TinhTrang, s.SucChua, g.DonGia, c.TenCoSo, c.DiaChi
                        FROM SanTheThao s
                        JOIN CoSo c ON s.MaCoSo = c.MaCoSo
                        LEFT JOIN GiaThueSan g ON s.MaCoSo = g.MaCoSo AND s.LoaiSan = g.LoaiSan
                        WHERE s.MaSan = @Ma";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Ma", _maSan);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Map dữ liệu vào object SanInfo để dùng lại cho màn hình Booking
                                _info = new SanInfo();
                                _info.TenSan = reader["MaSan"].ToString() + " - " + reader["TenCoSo"].ToString();
                                _info.LoaiSan = reader["LoaiSan"].ToString();
                                _info.GiaTien = reader["DonGia"] != DBNull.Value ? Convert.ToDecimal(reader["DonGia"]) : 0;

                                string diaChi = reader["DiaChi"].ToString();
                                string sucChua = reader["SucChua"].ToString();
                                string tinhTrang = reader["TinhTrang"].ToString();

                                RenderLayout(_info, diaChi, sucChua, tinhTrang);
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy thông tin sân này!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void RenderLayout(SanInfo info, string diaChi, string sucChua, string tinhTrang)
        {
            // Bố cục chia đôi: Trái (Ảnh) - Phải (Thông tin)
            Panel pnlMain = new Panel { Size = new Size(1000, 600) };
            pnlMain.Location = new Point((this.Width - 1000) / 2, 80);
            this.Resize += (s, e) => pnlMain.Left = (this.Width - pnlMain.Width) / 2;

            // 1. Ảnh lớn bên trái
            PictureBox pic = new PictureBox { Size = new Size(500, 400), Location = new Point(0, 0), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };

            // Load ảnh theo mã sân
            string maSanCode = info.TenSan.Split('-')[0].Trim();
            string path = Application.StartupPath + $"\\Images\\{maSanCode}.jpg";
            if (File.Exists(path)) pic.Image = Image.FromFile(path);
            else pic.BackColor = Color.LightGray;

            // 2. Thông tin bên phải
            Label lblName = new Label { Text = info.TenSan, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(530, 0), ForeColor = UIHelper.PrimaryColor };

            Label lblDetail = new Label
            {
                Text = $"📍 Địa chỉ: {diaChi}\n\n" +
                       $"⚽ Loại sân: {info.LoaiSan}\n\n" +
                       $"👥 Sức chứa: {sucChua} người\n\n" +
                       $"⚡ Trạng thái: {tinhTrang}\n\n" +
                       $"💰 Đơn giá: {info.GiaTien:N0} VNĐ/giờ",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Location = new Point(530, 60)
            };

            // Nút Đặt sân
            Button btnBook = new Button { Text = "ĐẶT SÂN NGAY", Size = new Size(200, 60), Location = new Point(530, 340) };
            UIHelper.StyleButton(btnBook, true);
            btnBook.Font = new Font("Segoe UI", 14, FontStyle.Bold);

            btnBook.Click += (s, e) => {
                // Đã check login ở ngoài Home rồi, nhưng check lại cho chắc
                if (SessionData.IsLoggedIn())
                    _mainForm.LoadView(new UC_BookingConfirm(_mainForm, info));
                else
                    MessageBox.Show("Vui lòng đăng nhập!");
            };

            pnlMain.Controls.AddRange(new Control[] { pic, lblName, lblDetail, btnBook });
            this.Controls.Add(pnlMain);
        }
    }
}