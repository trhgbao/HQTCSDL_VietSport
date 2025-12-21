using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public static class UIHelper
    {
        // Màu sắc chủ đạo của VietSport (Xanh thể thao & Xám hiện đại)
        public static Color PrimaryColor = Color.FromArgb(41, 128, 185); // Xanh dương
        public static Color SecondaryColor = Color.FromArgb(52, 73, 94); // Xám đậm
        public static Color BackgroundColor = Color.WhiteSmoke;
        public static Font MainFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static Font HeaderFont = new Font("Segoe UI", 14F, FontStyle.Bold);

        // Hàm style cho Button để trông phẳng (Flat)
        public static void StyleButton(Button btn, bool isPrimary = true)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = isPrimary ? PrimaryColor : Color.Gray;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Size = new Size(120, 40);
        }

        // Hàm style cho TextBox
        public static void StyleTextBox(TextBox txt)
        {
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = new Font("Segoe UI", 11F);
            txt.Padding = new Padding(5);
        }
    }
}