using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VietSportSystem
{
    public class UC_Manager_Stats : UserControl
    {
        public UC_Manager_Stats()
        {
            InitializeComponent();
            LoadStats();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            // Layout 4 ô vuông
            FlowLayoutPanel pnlCards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 250, Padding = new Padding(20) };

            pnlCards.Controls.Add(CreateCard("Doanh thu hôm nay", "0 VNĐ"));
            pnlCards.Controls.Add(CreateCard("Tỷ lệ lấp sân", "0%"));
            pnlCards.Controls.Add(CreateCard("Nhân viên trực", "3"));
            pnlCards.Controls.Add(CreateCard("Doanh thu tháng", "0 VNĐ"));

            // Grid báo cáo
            DataGridView grid = new DataGridView { Dock = DockStyle.Fill };

            this.Controls.Add(grid);
            this.Controls.Add(pnlCards);
        }

        private Panel CreateCard(string title, string value)
        {
            Panel pnl = new Panel { Size = new Size(300, 150), BackColor = Color.WhiteSmoke, Margin = new Padding(20) };
            Label lblTitle = new Label { Text = title, Location = new Point(20, 20), Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            Label lblVal = new Label { Text = value, Location = new Point(20, 60), Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = UIHelper.PrimaryColor, AutoSize = true };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblVal);
            return pnl;
        }

        private void LoadStats()
        {
            // Kết nối SQL để tính tổng doanh thu từ bảng HoaDon...
            // (Bạn tự bổ sung logic query ở đây tương tự các bài trước)
        }
    }
}