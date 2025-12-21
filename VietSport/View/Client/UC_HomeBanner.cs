using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO; // Để đọc file ảnh
using System.Windows.Forms;

namespace VietSportSystem
{
    // Class lưu thông tin 1 slide
    public class SlideItem
    {
        public Image Image { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RelatedSanID { get; set; }
    }

    public class UC_HomeBanner : UserControl
    {
        private MainForm _mainForm;
        private PictureBox picSlide;
        private Label lblSlideTitle;
        private Label lblSlideDesc;
        private System.Windows.Forms.Timer timerSlide; // Timer tự động chạy
        private List<SlideItem> slides;
        private int currentIndex = 0;

        public UC_HomeBanner(MainForm main = null) // main có thể null để designer không lỗi
        {
            _mainForm = main;
            InitializeComponent();
            LoadSlides(); // Nạp dữ liệu ảnh

            // Bắt đầu chạy slide
            if (slides.Count > 0)
            {
                ShowSlide(0);
                timerSlide.Start();
            }
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // 1. PictureBox hiển thị ảnh lớn
            picSlide = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom, // Co giãn ảnh vừa khung
                Cursor = Cursors.Hand // Hiện bàn tay khi trỏ vào
            };
            picSlide.Click += PicSlide_Click; // Sự kiện bấm vào ảnh

            // 2. Nút Next / Prev (Vẽ đè lên ảnh)
            Button btnPrev = CreateNavButton("<", 20);
            btnPrev.Click += (s, e) => ChangeSlide(-1);

            Button btnNext = CreateNavButton(">", Screen.PrimaryScreen.Bounds.Width - 100); // Áng chừng vị trí
            btnNext.Anchor = AnchorStyles.Right; // Neo phải
            btnNext.Click += (s, e) => ChangeSlide(1);

            // 3. Panel chứa thông tin (Overlay mờ bên dưới)
            Panel pnlInfo = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.FromArgb(150, 0, 0, 0) // Màu đen trong suốt
            };

            lblSlideTitle = new Label
            {
                Text = "Title",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 15),
                BackColor = Color.Transparent
            };

            lblSlideDesc = new Label
            {
                Text = "Description",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(30, 60),
                BackColor = Color.Transparent
            };

            pnlInfo.Controls.Add(lblSlideTitle);
            pnlInfo.Controls.Add(lblSlideDesc);
            picSlide.Controls.Add(pnlInfo); // Nhúng panel vào trong picbox để đè lên ảnh
            picSlide.Controls.Add(btnPrev);
            picSlide.Controls.Add(btnNext);

            this.Controls.Add(picSlide);

            // 4. Timer tự động chuyển
            timerSlide = new System.Windows.Forms.Timer();
            timerSlide.Interval = 3000; // 3 giây
            timerSlide.Tick += (s, e) => ChangeSlide(1); // Tự động Next
        }

        private Button CreateNavButton(string text, int x)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(50, 50),
                Location = new Point(x, 300), // Vị trí giữa theo chiều dọc
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 255, 255, 255), // Trắng mờ
                Font = new Font("Arial", 20, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadSlides()
        {
            slides = new List<SlideItem>();
            string path = Application.StartupPath + "\\Images\\";

            // Slide 1: Sân bóng (Gắn với SAN01)
            slides.Add(new SlideItem
            {
                Image = File.Exists(path + "SAN01.jpg") ? Image.FromFile(path + "SAN01.jpg") : CreatePlaceholderImage("SAN01", Color.Green),
                Title = "SÂN BÓNG ĐÁ VIETSPORT Q1",
                Description = "Sân cỏ nhân tạo tiêu chuẩn FIFA tại Quận 1.",
                RelatedSanID = "SAN01" // <--- QUAN TRỌNG: Phải khớp mã trong SQL
            });

            slides.Add(new SlideItem
            {
                Image = File.Exists(path + "SAN02.jpg") ? Image.FromFile(path + "SAN02.jpg") : CreatePlaceholderImage("SAN02", Color.Green),
                Title = "SÂN BÓNG ĐÁ VIETSPORT Q2",
                Description = "Sân cỏ nhân tạo tiêu chuẩn FIFA tại Quận 2.",
                RelatedSanID = "SAN02" // <--- QUAN TRỌNG: Phải khớp mã trong SQL
            });

            // Slide 2: Sân Tennis (Gắn với SAN04 - Giả sử bạn có sân này)
            slides.Add(new SlideItem
            {
                Image = File.Exists(path + "SAN03.jpg") ? Image.FromFile(path + "SAN03.jpg") : CreatePlaceholderImage("SAN03", Color.Blue),
                Title = "SÂN TENNIS THỦ ĐỨC",
                Description = "Mặt sân cứng, đèn chiếu sáng hiện đại.",
                RelatedSanID = "SAN03" // <--- Mã sân Tennis
            });

            slides.Add(new SlideItem
            {
                Image = File.Exists(path + "SAN04.jpg") ? Image.FromFile(path + "SAN04.jpg") : CreatePlaceholderImage("SAN04", Color.Blue),
                Title = "SÂN TENNIS BÌNH THẠNH",
                Description = "Mặt sân cứng, đèn chiếu sáng hiện đại.",
                RelatedSanID = "SAN04" // <--- Mã sân Tennis
            });
        }

        // Hàm tạo ảnh giả lập (Để code chạy được ngay mà không cần bạn chép file)
        private Image CreatePlaceholderImage(string text, Color color)
        {
            Bitmap bmp = new Bitmap(1280, 720);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(color);

                // Vẽ chữ giữa hình
                using (Font font = new Font("Arial", 50, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, Brushes.White, (1280 - textSize.Width) / 2, (720 - textSize.Height) / 2);
                }

                // Vẽ trang trí
                g.DrawRectangle(new Pen(Color.White, 10), 50, 50, 1180, 620);
            }
            return bmp;
        }

        private void ChangeSlide(int step)
        {
            currentIndex += step;
            // Xử lý vòng lặp (Cuối quay về Đầu và ngược lại)
            if (currentIndex >= slides.Count) currentIndex = 0;
            if (currentIndex < 0) currentIndex = slides.Count - 1;

            ShowSlide(currentIndex);
        }

        private void ShowSlide(int index)
        {
            if (slides.Count == 0) return;
            var item = slides[index];

            picSlide.Image = item.Image;
            lblSlideTitle.Text = item.Title;
            lblSlideDesc.Text = item.Description;

            // Reset timer để tránh việc vừa bấm Next xong nó lại tự nhảy tiếp
            timerSlide.Stop();
            timerSlide.Start();
        }

        // --- SỰ KIỆN CLICK VÀO ẢNH ---
        private void PicSlide_Click(object sender, EventArgs e)
        {
            if (slides.Count == 0) return;

            // Lấy mã sân của hình đang hiện
            string selectedSanID = slides[currentIndex].RelatedSanID;

            // KIỂM TRA ĐĂNG NHẬP
            if (SessionData.IsLoggedIn())
            {
                // Đã đăng nhập -> Chuyển sang trang Chi tiết sân
                if (_mainForm != null)
                    _mainForm.LoadView(new UC_SanDetail(_mainForm, selectedSanID));
            }
            else
            {
                // Chưa đăng nhập -> Hiện form yêu cầu
                FormLoginRequired frmAsk = new FormLoginRequired();
                frmAsk.ShowDialog();

                if (frmAsk.UserChoice == "Login")
                {
                    _mainForm.LoadView(new UC_Login(_mainForm));
                }
                else if (frmAsk.UserChoice == "Register")
                {
                    _mainForm.LoadView(new UC_Register(_mainForm));
                }
                // Nếu User đóng form (Cancel) thì không làm gì cả, vẫn ở lại trang chủ
            }
        }
    }
}