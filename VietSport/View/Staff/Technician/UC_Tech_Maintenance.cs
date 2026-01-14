using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace VietSportSystem
{
    public class UC_Tech_Maintenance : UserControl
    {
        private FlowLayoutPanel pnlGrid;
        private Button btnRunSystem;

        public UC_Tech_Maintenance()
        {
            InitializeComponent();
            LoadBadFields();
            // --- ĐÃ XÓA 2 DÒNG GỌI DEMO TỰ ĐỘNG Ở ĐÂY ---
            // Chỉ khi nào bấm nút thì mới chạy, không chạy lúc khởi động
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            Label lblTitle = new Label { Text = "Danh sách sân cần bảo trì", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter };

            pnlGrid = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            // Sửa lỗi Shadowing: Bỏ chữ "Button" ở đầu để dùng biến toàn cục
            btnRunSystem = new Button
            {
                Text = "⚙️ Chạy Tool Hủy Tự Động (Demo Race Condition)",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnRunSystem.Click += (s, e) => RunSystemAutoCancel();

            // Thứ tự Add quan trọng để Dock hoạt động đúng:
            // Add Fill trước, sau đó Add Top (nó sẽ đẩy cái Fill xuống dưới)
            // Hoặc add lần lượt và dùng BringToFront
            this.Controls.Add(pnlGrid);
            this.Controls.Add(btnRunSystem); // Nằm trên Grid
            this.Controls.Add(lblTitle);     // Nằm trên cùng
        }

        private void RunSystemAutoCancel()
        {
            // Reset dữ liệu trước khi chạy để đảm bảo có phiếu mà hủy
            ResetDataDemo();

            MessageBox.Show("SYSTEM: Bắt đầu quét... (Sẽ giữ khóa trong 10 giây)\n\n-> Qua bên Lễ tân bấm Check-in NGAY đi!", "Thông báo Demo");

            // Chạy bất đồng bộ để không đơ giao diện
            System.Threading.Tasks.Task.Run(() => {
                try
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("sp_Demo_System_AutoCancel", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaPhieu", "DEMO_RACE");
                        cmd.ExecuteNonQuery(); // Lệnh này sẽ Treo 10s trong SQL
                    }
                    // Dùng Invoke để cập nhật UI từ luồng khác
                    this.Invoke(new Action(() => MessageBox.Show("SYSTEM: Đã HỦY phiếu DEMO_RACE thành công!")));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() => MessageBox.Show("System Error: " + ex.Message)));
                }
            });
        }

        private void ResetDataDemo()
        {
            try
            {
                // Hàm reset nhanh phiếu về trạng thái 'Chưa thanh toán' để test lại
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    // Đảm bảo phiếu tồn tại trước khi update
                    string sqlCheck = "SELECT COUNT(*) FROM PhieuDatSan WHERE MaPhieuDat = 'DEMO_RACE'";
                    SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                    if ((int)cmdCheck.ExecuteScalar() == 0)
                    {
                        // Nếu chưa có thì tạo mới (phòng trường hợp chưa chạy script SQL)
                        string sqlInsert = @"INSERT INTO PhieuDatSan (MaPhieuDat, MaKhachHang, MaSan, GioBatDau, GioKetThuc, TrangThaiThanhToan, KenhDat)
                                             VALUES ('DEMO_RACE', 'KH_TEST', 'SAN01', DATEADD(MINUTE, -30, GETDATE()), DATEADD(MINUTE, 90, GETDATE()), N'Chưa thanh toán', 'Online')";
                        new SqlCommand(sqlInsert, conn).ExecuteNonQuery();
                    }
                    else
                    {
                        // Reset lại trạng thái
                        string sqlUpdate = "UPDATE PhieuDatSan SET TrangThaiThanhToan = N'Chưa thanh toán' WHERE MaPhieuDat = 'DEMO_RACE'";
                        new SqlCommand(sqlUpdate, conn).ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private void LoadBadFields()
        {
            pnlGrid.Controls.Clear();
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Lấy danh sách sân đang bảo trì
                    string sql = @"
                        SELECT MaSan, LoaiSan, GhiChu 
                        FROM SanTheThao 
                        WHERE TinhTrang = N'Bảo trì'";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Kiểm tra null cho cột GhiChu
                                string ghiChu = reader["GhiChu"] != DBNull.Value ? reader["GhiChu"].ToString() : "Không có mô tả";

                                pnlGrid.Controls.Add(CreateDetailCard(
                                    reader["MaSan"].ToString(),
                                    reader["LoaiSan"].ToString(),
                                    ghiChu,
                                    "Xem trong ghi chú"
                                ));
                            }
                        }
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
            Panel pnl = new Panel { Size = new Size(600, 200), BackColor = Color.WhiteSmoke, Margin = new Padding(20), BorderStyle = BorderStyle.FixedSingle };

            // Hình ảnh
            PictureBox pic = new PictureBox { Size = new Size(250, 200), Dock = DockStyle.Left, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Silver };
            string pathImg = Application.StartupPath + $"\\Images\\{maSan}.jpg";
            if (System.IO.File.Exists(pathImg)) pic.Image = Image.FromFile(pathImg);

            // Thông tin
            Panel pnlInfo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblTitle = new Label { Text = $"Tên sân: {maSan} - {loaiSan}", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };
            Label lblDes = new Label { Text = $"Mô tả lỗi: {loi}", Dock = DockStyle.Top, Height = 60 };
            Label lblDate = new Label
            {
                Text = $"{ngayKet}",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            pnlInfo.Controls.AddRange(new Control[] { lblDate, lblDes, lblTitle });

            // Sự kiện Click để sửa trạng thái
            pnl.Cursor = Cursors.Hand;
            EventHandler openEdit = (s, e) => {
                FormUpdateStatus frm = new FormUpdateStatus(maSan, "Bảo trì", loi);
                frm.ShowDialog();
                LoadBadFields();
            };

            pnl.Click += openEdit;
            pic.Click += openEdit;
            foreach (Control c in pnlInfo.Controls) c.Click += openEdit; // Bấm vào chữ cũng nhận

            pnl.Controls.Add(pnlInfo);
            pnl.Controls.Add(pic);
            return pnl;
        }
    }
}