using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_FastSearch : UserControl
    {
        private MainForm _mainForm;

        // --- KHAI BÁO BIẾN TOÀN CỤC (Để dùng được ở mọi nơi trong file) ---
        private ComboBox cboCity;
        private ComboBox cboType;
        private DateTimePicker dtpStart; // <--- Sửa lỗi CS0103 tại đây
        private DateTimePicker dtpEnd;
        private MonthCalendar calendar;

        public UC_FastSearch(MainForm main)
        {
            _mainForm = main;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.WhiteSmoke;
            this.Padding = new Padding(30);

            // 1. Tiêu đề
            Label lblTitle = new Label
            {
                Text = "HỖ TRỢ TÌM KIẾM SÂN BÃI NHANH",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = UIHelper.SecondaryColor,
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Panel pnlTitleWrap = new Panel { Dock = DockStyle.Top, Height = 60 };
            pnlTitleWrap.Controls.Add(lblTitle);

            // 2. Container bộ lọc
            Panel pnlFilterBox = new Panel
            {
                Dock = DockStyle.Top,
                Height = 350,
                BackColor = Color.White,
            };
            Panel pnlBorder = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = Color.LightGray };
            pnlFilterBox.Controls.Add(pnlBorder);

            // 3. Grid Layout
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(20)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            // --- CỘT 1: THÀNH PHỐ & LOẠI ---
            Panel pnlCol1 = new Panel { Dock = DockStyle.Fill };
            Label lblCity = CreateLabel("Chọn Thành phố:");
            cboCity = CreateComboBox(new string[] { "Hồ Chí Minh", "Hà Nội", "Đà Nẵng", "Cần Thơ" }); // Gán vào biến toàn cục
            cboCity.Top = 30;

            Label lblType = CreateLabel("Chọn Loại sân:");
            lblType.Top = 80;
            cboType = CreateComboBox(new string[] { "Bóng đá mini", "Cầu lông", "Tennis", "Bóng rổ", "Futsal" }); // Gán vào biến toàn cục
            cboType.Top = 110;

            pnlCol1.Controls.AddRange(new Control[] { lblCity, cboCity, lblType, cboType });

            // --- CỘT 2: NGÀY ---
            Panel pnlCol2 = new Panel { Dock = DockStyle.Fill };
            Label lblDate = CreateLabel("Chọn Ngày:");
            calendar = new MonthCalendar // Gán vào biến toàn cục
            {
                Location = new Point(0, 30),
                MaxSelectionCount = 1,
                ShowTodayCircle = true
            };
            pnlCol2.Controls.AddRange(new Control[] { lblDate, calendar });

            // --- CỘT 3: GIỜ ---
            Panel pnlCol3 = new Panel { Dock = DockStyle.Fill };
            Label lblTime = CreateLabel("Khung giờ:");

            Label lblStart = new Label { Text = "Bắt đầu:", Location = new Point(0, 40), AutoSize = true, Font = UIHelper.MainFont };
            dtpStart = CreateTimePicker(65); // Gán vào biến toàn cục

            Label lblEnd = new Label { Text = "Kết thúc:", Location = new Point(0, 100), AutoSize = true, Font = UIHelper.MainFont };
            dtpEnd = CreateTimePicker(125); // Gán vào biến toàn cục
            dtpEnd.Value = dtpStart.Value.AddHours(1);

            pnlCol3.Controls.AddRange(new Control[] { lblTime, lblStart, dtpStart, lblEnd, dtpEnd });

            // --- CỘT 4: NÚT TÌM ---
            Panel pnlCol4 = new Panel { Dock = DockStyle.Fill };
            Button btnSearch = new Button { Text = "🔍 TÌM KIẾM" };
            UIHelper.StyleButton(btnSearch, true);
            btnSearch.Size = new Size(180, 50);
            btnSearch.Location = new Point(10, 80);
            btnSearch.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSearch.Click += BtnSearch_Click;

            pnlCol4.Controls.Add(btnSearch);

            // Add vào Grid
            grid.Controls.Add(pnlCol1, 0, 0);
            grid.Controls.Add(pnlCol2, 1, 0);
            grid.Controls.Add(pnlCol3, 2, 0);
            grid.Controls.Add(pnlCol4, 3, 0);

            pnlFilterBox.Controls.Add(grid);
            this.Controls.Add(pnlFilterBox);
            this.Controls.Add(pnlTitleWrap);
        }

        // --- SỰ KIỆN TÌM KIẾM (Đã sửa logic chọn giờ) ---
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string city = cboCity.SelectedItem?.ToString() ?? "";
            string type = cboType.SelectedItem?.ToString() ?? "";

            // Lấy ngày và giờ từ các biến toàn cục
            DateTime selectedDate = calendar.SelectionStart;
            int hour = dtpStart.Value.Hour;
            DayOfWeek day = selectedDate.DayOfWeek;

            // 1. Logic Xác định khung giờ
            string khungGioCanTim = "Ngày thường";

            if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
            {
                khungGioCanTim = "Cuối tuần";
            }
            else if (hour >= 17 && hour <= 21) // Ví dụ cao điểm: 17h - 21h
            {
                khungGioCanTim = "Giờ cao điểm";
            }
            else
            {
                khungGioCanTim = "Ngày thường";
            }

            // 2. Kết nối Database
            List<SanInfo> ketQuaThat = new List<SanInfo>();

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Query có DISTINCT và Filter theo KhungGio
                    string query = @"
                        SELECT DISTINCT s.MaSan, s.LoaiSan, s.TinhTrang, g.DonGia, c.TenCoSo
                        FROM SanTheThao s
                        JOIN CoSo c ON s.MaCoSo = c.MaCoSo
                        LEFT JOIN GiaThueSan g ON s.MaCoSo = g.MaCoSo 
                                               AND s.LoaiSan = g.LoaiSan 
                                               AND g.KhungGio = @Khung
                        WHERE c.ThanhPho LIKE @City 
                        AND s.LoaiSan LIKE @Type
                        AND s.TinhTrang = N'Còn trống'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@City", "%" + city + "%");
                        cmd.Parameters.AddWithValue("@Type", "%" + type + "%");
                        cmd.Parameters.AddWithValue("@Khung", khungGioCanTim);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SanInfo info = new SanInfo();
                                info.TenSan = reader["MaSan"].ToString() + " - " + reader["TenCoSo"].ToString();
                                info.LoaiSan = reader["LoaiSan"].ToString();
                                info.TrangThai = reader["TinhTrang"].ToString();
                                info.KhungGio = khungGioCanTim; // Hiển thị khung giờ đã chọn

                                if (reader["DonGia"] != DBNull.Value)
                                    info.GiaTien = Convert.ToDecimal(reader["DonGia"]);
                                else
                                    info.GiaTien = 0;

                                ketQuaThat.Add(info);
                            }
                        }
                    }
                }

                // 3. Hiển thị kết quả
                if (ketQuaThat.Count > 0)
                {
                    _mainForm.LoadView(new UC_SearchResult(_mainForm, ketQuaThat));
                }
                else
                {
                    MessageBox.Show($"Không tìm thấy sân '{type}' nào tại '{city}' trong khung giờ '{khungGioCanTim}'!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper Methods
        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(0, 0)
            };
        }

        private ComboBox CreateComboBox(string[] items)
        {
            ComboBox cbo = new ComboBox();
            cbo.Items.AddRange(items);
            if (items.Length > 0) cbo.SelectedIndex = 0;
            cbo.Font = new Font("Segoe UI", 11F);
            cbo.Width = 200;
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo.FlatStyle = FlatStyle.System;
            return cbo;
        }

        private DateTimePicker CreateTimePicker(int topY)
        {
            return new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Font = new Font("Segoe UI", 12F),
                Width = 120,
                Location = new Point(0, topY)
            };
        }
    }
}