using System.Drawing;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_ResetPassword : UserControl
    {
        public UC_ResetPassword()
        {
            this.BackColor = Color.White;
            Label lbl = new Label
            {
                Text = "Màn hình Đổi Mật Khẩu",
                AutoSize = true,
                Location = new Point(50, 50)
            };
            this.Controls.Add(lbl);
        }
    }
}