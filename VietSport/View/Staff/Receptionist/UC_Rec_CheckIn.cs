using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_Rec_CheckIn : UserControl
    {
        private TextBox txtSearch;
        private FlowLayoutPanel pnlList;

        public UC_Rec_CheckIn()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(40, 40, 40);

            // Search Bar
            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };
            txtSearch = new TextBox { Location = new Point(100, 25), Width = 400, Font = new Font("Segoe UI", 12) };
            Button btnSearch = new Button { Text = "Tìm kiếm", Location = new Point(520, 24), Size = new Size(100, 30) };
            btnSearch.Click += (s, e) => LoadBookings(txtSearch.Text);

            pnlSearch.Controls.Add(new Label { Text = "Tên KH:", Location = new Point(20, 28) });
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnSearch);

            // List
            pnlList = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(50) };

            this.Controls.Add(pnlList);
            this.Controls.Add(pnlSearch);

            LoadBookings("");
        }

        private void LoadBookings(string keyword)
        {
            pnlList.Controls.Clear();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
            SELECT p.MaPhieuDat, kh.HoTen, s.LoaiSan, s.MaSan, p.GioBatDau, p.GioKetThuc, p.TrangThaiThanhToan
            FROM PhieuDatSan p
            JOIN KhachHang kh ON p.MaKhachHang = kh.MaKhachHang
            JOIN SanTheThao s ON p.MaSan = s.MaSan
            WHERE (kh.HoTen LIKE @Key OR p.MaPhieuDat LIKE @Key)
            AND p.TrangThaiThanhToan != N'Đã hủy'
            AND (@Key != '' OR CONVERT(date, p.GioBatDau) = CONVERT(date, GETDATE()))";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Key", "%" + keyword + "%");
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    pnlList.Controls.Add(CreateBookingCard(
                        reader["MaPhieuDat"].ToString(),
                        reader["HoTen"].ToString(),
                        reader["MaSan"].ToString() + " - " + reader["LoaiSan"].ToString(),
                        DateTime.Parse(reader["GioBatDau"].ToString()),
                        DateTime.Parse(reader["GioKetThuc"].ToString()),
                        reader["TrangThaiThanhToan"].ToString()
                    ));
                }
            }
        }

        private Panel CreateBookingCard(string maPhieu, string tenKH, string san, DateTime start, DateTime end, string status)
        {
            Panel pnl = new Panel { Size = new Size(800, 150), BackColor = Color.LightGray, Margin = new Padding(0, 0, 0, 20) };

            // Cột Trái (Tên sân, Tên khách)
            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 400, BackColor = Color.Silver };
            Label lblSan = new Label { Text = san, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            Label lblKH = new Label { Text = tenKH, Font = new Font("Segoe UI", 12), Location = new Point(20, 70), AutoSize = true, BackColor = Color.White, Padding = new Padding(5) };

            // Nút Hủy
            Button btnCancel = new Button { Text = "Hủy Đặt Sân", Location = new Point(200, 70), Size = new Size(120, 35) };
            btnCancel.Click += (s, e) => UpdateStatus(maPhieu, "Đã hủy");

            pnlLeft.Controls.AddRange(new Control[] { lblSan, lblKH, btnCancel });

            // Cột Phải (Giờ, Nút xác nhận)
            Label lblTime = new Label { Text = $"{start:HH:mm} - {end:HH:mm}\n{start:dd/MM/yyyy}", Location = new Point(420, 20), Font = new Font("Segoe UI", 12) };

            Button btnConfirm = new Button { Text = "Xác Nhận (Check-in)", Location = new Point(420, 80), Size = new Size(150, 40), BackColor = Color.DarkGray };
            btnConfirm.Click += (s, e) => UpdateStatus(maPhieu, "Đã thanh toán");

            pnl.Controls.AddRange(new Control[] { pnlLeft, lblTime, btnConfirm });
            return pnl;
        }

        private void UpdateStatus(string maPhieu, string status)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE PhieuDatSan SET TrangThaiThanhToan = @Stt WHERE MaPhieuDat = @Ma";
                if (status == "Đang sử dụng")
                {
                    // Logic Check-in: Có thể cập nhật thêm bảng Checkin nếu cần
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Stt", status);
                cmd.Parameters.AddWithValue("@Ma", maPhieu);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Cập nhật thành công: " + status);
                LoadBookings(""); // Load lại danh sách
            }
        }
    }
}