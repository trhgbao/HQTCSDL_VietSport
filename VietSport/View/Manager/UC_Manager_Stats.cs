using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VietSportSystem
{
    public class UC_Manager_Stats : UserControl
    {
        // Controls
        private Panel pnlTop;
        private DateTimePicker dtpNgay;
        private Button btnThongKe;
        private CheckBox chkBaoCaoNhatQuan; // FIX mode
        private ProgressBar prgLoading;
        private Label lblStatus;

        private DataGridView dgvHoaDon;
        private Label lblTongDoanhThu;
        private Label lblTongLabel;

        public UC_Manager_Stats()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // ===== Top bar =====
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(12)
            };

            var lblNgay = new Label
            {
                Text = "Ngày báo cáo:",
                AutoSize = true,
                Location = new Point(12, 18),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            dtpNgay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Width = 140,
                Location = new Point(lblNgay.Right + 10, 14)
            };

            chkBaoCaoNhatQuan = new CheckBox
            {
                Text = "Bật Fix lỗi",
                AutoSize = true,
                Location = new Point(dtpNgay.Right + 20, 16),
                Checked = false
            };

            btnThongKe = new Button
            {
                Text = "Thống kê",
                Width = 90,
                Height = 32,
                Location = new Point(chkBaoCaoNhatQuan.Right + 40, 12)
            };
            btnThongKe.Click += async (s, e) => await LoadReportAsync();

            prgLoading = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Visible = false,
                Width = 160,
                Height = 18,
                Location = new Point(btnThongKe.Right + 20, 16)
            };

            lblStatus = new Label
            {
                Text = "",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(prgLoading.Right + 12, 18),
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            pnlTop.Controls.Add(lblNgay);
            pnlTop.Controls.Add(dtpNgay);
            pnlTop.Controls.Add(chkBaoCaoNhatQuan);
            pnlTop.Controls.Add(btnThongKe);
            pnlTop.Controls.Add(prgLoading);
            pnlTop.Controls.Add(lblStatus);

            // ===== Summary row =====
            var pnlSummary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            lblTongLabel = new Label
            {
                Text = "Tổng doanh thu:",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(12, 14)
            };

            lblTongDoanhThu = new Label
            {
                Text = "0",
                AutoSize = true,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(lblTongLabel.Right + 40, 8)
            };

            pnlSummary.Controls.Add(lblTongLabel);
            pnlSummary.Controls.Add(lblTongDoanhThu);

            // ===== Grid =====
            dgvHoaDon = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Layout
            this.Controls.Add(dgvHoaDon);
            this.Controls.Add(pnlSummary);
            this.Controls.Add(pnlTop);
        }

        private async Task LoadReportAsync()
        {
            DateTime ngay = dtpNgay.Value.Date;
            bool useFix = chkBaoCaoNhatQuan.Checked;

            // Tên SP: bạn tạo theo hướng nghiệp vụ thật
            // BUG: danh sách đọc trước, tổng tính sau, có WAITFOR trong SP
            // FIX: SERIALIZABLE để đảm bảo nhất quán
            string proc = useFix ? "sp_BaoCaoDoanhThu_FIX" : "sp_BaoCaoDoanhThu_BUG";

            SetLoading(true, useFix
                ? "Đang chạy báo cáo nhất quán..."
                : "Đang chạy báo cáo...");

            try
            {
                var result = await Task.Run(() => ExecReport(proc, ngay));

                // Bind UI
                dgvHoaDon.DataSource = result.InvoiceTable;
                lblTongDoanhThu.Text = result.TotalRevenue.ToString("N0") + " đ";

                // Gợi ý hiển thị tình huống #7 bằng mắt:
                // BUG mode có thể thấy tổng != tổng từ danh sách nếu thu ngân insert trong lúc WAITFOR
                if (!useFix)
                {
                    decimal sumFromList = 0;
                    foreach (DataRow row in result.InvoiceTable.Rows)
                    {
                        if (row.Table.Columns.Contains("TongTien") && row["TongTien"] != DBNull.Value)
                            sumFromList += Convert.ToDecimal(row["TongTien"]);
                    }

                    if (sumFromList != result.TotalRevenue)
                    {
                        lblStatus.Text = $"⚠ Không nhất quán: Tổng theo danh sách = {sumFromList:N0}đ, Tổng báo cáo = {result.TotalRevenue:N0}đ";
                        lblStatus.ForeColor = Color.DarkOrange;
                    }
                    else
                    {
                        lblStatus.Text = "Báo cáo ổn (không phát hiện lệch tại thời điểm này).";
                        lblStatus.ForeColor = Color.DimGray;
                    }
                }
                else
                {
                    lblStatus.Text = "Báo cáo nhất quán (FIX).";
                    lblStatus.ForeColor = Color.SeaGreen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thống kê: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi thống kê.";
                lblStatus.ForeColor = Color.Firebrick;
            }
            finally
            {
                SetLoading(false, "");
            }
        }

        private void SetLoading(bool isLoading, string statusText)
        {
            prgLoading.Visible = isLoading;
            btnThongKe.Enabled = !isLoading;
            dtpNgay.Enabled = !isLoading;
            chkBaoCaoNhatQuan.Enabled = !isLoading;
            lblStatus.Text = statusText;
            if (!isLoading)
                lblStatus.ForeColor = Color.DimGray;
        }

        // Kết quả trả về từ SP: 2 result set
        private (DataTable InvoiceTable, decimal TotalRevenue) ExecReport(string procName, DateTime ngay)
        {
            var ds = new DataSet();
            decimal total = 0m;

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(procName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Ngay", SqlDbType.Date).Value = ngay.Date;
                cmd.CommandTimeout = 120;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(ds); // ds.Tables[0] = list, ds.Tables[1] = total (nếu SP trả 2 result-set)
                }
            }

            DataTable invoiceTable = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();

            // Bắt tổng từ result-set thứ 2
            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
            {
                // Có thể cột tên TongDoanhThu hoặc cột 0
                if (ds.Tables[1].Columns.Contains("TongDoanhThu"))
                {
                    total = Convert.ToDecimal(ds.Tables[1].Rows[0]["TongDoanhThu"]);
                }
                else
                {
                    total = Convert.ToDecimal(ds.Tables[1].Rows[0][0]);
                }
            }

            // (Tùy chọn) sanity check: nếu total vẫn 0 nhưng list có dữ liệu, log để biết SP không trả RS2
            // Bạn có thể bỏ phần này sau khi ổn
            if (total == 0m && invoiceTable.Columns.Contains("TongTien"))
            {
                decimal sumFromList = 0m;
                foreach (DataRow row in invoiceTable.Rows)
                {
                    if (row["TongTien"] != DBNull.Value)
                        sumFromList += Convert.ToDecimal(row["TongTien"]);
                }
                // Nếu sumFromList > 0 mà total = 0 => 99% SP không trả result-set tổng / app gọi nhầm SP/DB
                // Bạn có thể debug bằng cách hiển thị lblStatus
            }

            return (invoiceTable, total);
        }
    }
}
