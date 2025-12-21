using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_SearchResult : UserControl
    {
        private MainForm _mainForm;
        private FlowLayoutPanel pnlContainer;

        public UC_SearchResult(MainForm main, List<SanInfo> data)
        {
            _mainForm = main;
            InitializeComponent();
            RenderData(data);
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.WhiteSmoke;

            // Tiêu đề
            Label lblTitle = new Label
            {
                Text = "KẾT QUẢ TÌM KIẾM SÂN BÃI",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = UIHelper.SecondaryColor,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 60
            };

            // Container chứa các thẻ sân (FlowLayout để tự xuống dòng)
            pnlContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            this.Controls.Add(pnlContainer);
            this.Controls.Add(lblTitle);
        }

        private void RenderData(List<SanInfo> data)
        {
            pnlContainer.Controls.Clear();
            foreach (var item in data)
            {
                pnlContainer.Controls.Add(CreateSanCard(item));
            }
        }

        // Tạo thẻ Sân (Card) giống Wireframe 1
        private Panel CreateSanCard(SanInfo info)
        {
            Panel pnlCard = new Panel
            {
                Size = new Size(500, 200),
                BackColor = Color.White,
                Margin = new Padding(20)
            };

            // 1. Xử lý Hình ảnh Dynamic
            PictureBox pic = new PictureBox
            {
                Size = new Size(200, 200),
                Location = new Point(0, 0),
                BackColor = Color.WhiteSmoke, // Màu nền nếu ảnh trong suốt
                SizeMode = PictureBoxSizeMode.Zoom // Co giãn ảnh đẹp
            };

            try
            {
                // Logic: Lấy Mã sân từ chuỗi "SAN01 - VietSport..."
                // Cắt chuỗi tại dấu gạch ngang và lấy phần đầu tiên
                string maSan = info.TenSan.Split('-')[0].Trim();

                // Đường dẫn ảnh: thư mục chạy + Images + Mã sân + .jpg
                string pathImage = System.Windows.Forms.Application.StartupPath + $"\\Images\\{maSan}.jpg";
                string pathDefault = System.Windows.Forms.Application.StartupPath + "\\Images\\default.jpg";

                if (System.IO.File.Exists(pathImage))
                {
                    // Nếu có ảnh riêng (SAN01.jpg) -> Load ảnh đó
                    pic.Image = Image.FromFile(pathImage);
                }
                else
                {
                    // Nếu không có ảnh riêng -> Kiểm tra loại sân để lấy ảnh chung
                    // Ví dụ: Không có SAN05.jpg, nhưng là sân "Bóng đá mini" -> Load bongda.jpg
                    string pathLoai = System.Windows.Forms.Application.StartupPath + "\\Images\\bongda.jpg"; // Demo

                    // Logic mở rộng (Tùy chọn):
                    if (info.LoaiSan.Contains("Bóng đá")) pathLoai = System.Windows.Forms.Application.StartupPath + "\\Images\\bongda.jpg";
                    else if (info.LoaiSan.Contains("Tennis")) pathLoai = System.Windows.Forms.Application.StartupPath + "\\Images\\tennis.jpg";
                    else if (info.LoaiSan.Contains("Cầu lông")) pathLoai = System.Windows.Forms.Application.StartupPath + "\\Images\\caulong.jpg";

                    if (System.IO.File.Exists(pathLoai))
                        pic.Image = Image.FromFile(pathLoai);
                    else if (System.IO.File.Exists(pathDefault))
                        pic.Image = Image.FromFile(pathDefault); // Cuối cùng mới dùng default
                }
            }
            catch
            {
                // Nếu lỗi quá thì để trống hoặc đổ màu
                pic.BackColor = Color.Gray;
            }

            // 2. Thông tin (Bên phải)
            Label lblName = new Label
            {
                Text = info.TenSan,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(220, 20),
                AutoSize = true
            };

            Label lblAddress = new Label
            {
                Text = "Địa chỉ: " + info.TenSan, // Tạm lấy tên làm địa chỉ demo
                Font = new Font("Segoe UI", 10),
                Location = new Point(220, 60),
                AutoSize = true,
                ForeColor = Color.DimGray
            };

            Label lblPrice = new Label
            {
                Text = "Giá: " + info.GiaTien.ToString("N0") + " VNĐ/giờ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(220, 100),
                AutoSize = true,
                ForeColor = Color.Red
            };

            Button btnBook = new Button { Text = "ĐẶT NGAY" };
            UIHelper.StyleButton(btnBook, true);
            btnBook.Location = new Point(350, 140);

            // Sự kiện bấm đặt -> Chuyển sang màn hình Booking
            btnBook.Click += (s, e) => _mainForm.LoadView(new UC_BookingConfirm(_mainForm, info));

            pnlCard.Controls.AddRange(new Control[] { pic, lblName, lblAddress, lblPrice, btnBook });
            return pnlCard;
        }
    }
}