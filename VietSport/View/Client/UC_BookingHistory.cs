using System;
using System.Drawing;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_BookingHistory : UserControl
    {
        private MainForm _mainForm;
        private FlowLayoutPanel pnlList;

        public UC_BookingHistory(MainForm main)
        {
            _mainForm = main;
            InitializeComponent();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            Label lblTitle = new Label { Text = "LỊCH SỬ ĐẶT SÂN CỦA BẠN", Dock = DockStyle.Top, Font = UIHelper.HeaderFont, Height = 60, TextAlign = ContentAlignment.MiddleCenter };

            pnlList = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            this.Controls.Add(pnlList);
            this.Controls.Add(lblTitle);
        }

        private void LoadHistory()
        {
            pnlList.Controls.Clear();
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT p.MaPhieuDat, s.LoaiSan, p.GioBatDau, p.GioKetThuc, p.TrangThaiThanhToan, c.TenCoSo
                    FROM PhieuDatSan p
                    JOIN SanTheThao s ON p.MaSan = s.MaSan
                    JOIN CoSo c ON s.MaCoSo = c.MaCoSo
                    WHERE p.MaKhachHang = @KH
                    ORDER BY p.GioBatDau DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@KH", SessionData.CurrentUserID);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pnlList.Controls.Add(CreateHistoryItem(
                        reader["MaPhieuDat"].ToString(),
                        reader["LoaiSan"].ToString(),
                        reader["TenCoSo"].ToString(),
                        DateTime.Parse(reader["GioBatDau"].ToString()),
                        DateTime.Parse(reader["GioKetThuc"].ToString()),
                        reader["TrangThaiThanhToan"].ToString()
                    ));
                }
            }
        }

        private Panel CreateHistoryItem(string maPhieu, string loaiSan, string coSo, DateTime start, DateTime end, string status)
        {
            Panel pnl = new Panel { Size = new Size(900, 150), BackColor = Color.WhiteSmoke, Margin = new Padding(10) };

            // Hình mô phỏng bên trái
            PictureBox pic = new PictureBox { Size = new Size(150, 150), BackColor = Color.Silver, Dock = DockStyle.Left };

            // Thông tin
            Label lblInfo = new Label
            {
                Text = $"{loaiSan} - {coSo}\nMã phiếu: {maPhieu}\nNgày: {start:dd/MM/yyyy}\nGiờ: {start:HH:mm} - {end:HH:mm}\nTrạng thái: {status}",
                Location = new Point(170, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11)
            };

            // Nút Hủy
            if (status != "Đã hủy" && start > DateTime.Now) // Chỉ cho hủy nếu chưa diễn ra
            {
                Button btnCancel = new Button { Text = "Hủy Đặt Sân", Location = new Point(750, 50), Size = new Size(120, 40), BackColor = Color.IndianRed, ForeColor = Color.White };
                btnCancel.Click += (s, e) => CancelBooking(maPhieu);
                pnl.Controls.Add(btnCancel);
            }

            pnl.Controls.Add(lblInfo);
            pnl.Controls.Add(pic);
            return pnl;
        }

        private void CancelBooking(string maPhieu)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn hủy?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    // Ở đây ta xóa luôn cho gọn, thực tế nên update trạng thái thành 'Đã hủy'
                    // Lưu ý: Phải xóa HoaDon trước nếu có khóa ngoại
                    SqlCommand cmd = new SqlCommand("DELETE FROM PhieuDatSan WHERE MaPhieuDat = @Ma", conn);
                    cmd.Parameters.AddWithValue("@Ma", maPhieu);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Đã hủy thành công!");
                    LoadHistory(); // Load lại danh sách
                }
            }
        }
    }
}