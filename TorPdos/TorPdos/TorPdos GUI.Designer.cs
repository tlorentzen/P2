using System.Drawing;
using System.Windows.Forms;

namespace TorPdos
{
    public class MyForm : Form
    {
        Label lblPortal = new Label();
        public MyForm()
        {
            this.SuspendLayout();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.Width = 500;
            this.Height = 300;
            this.Name = "TorPdos";
            this.Text = "TorPdos";
            this.ResumeLayout(false);
            this.BackColor = ColorTranslator.FromHtml("#A2BCE0");
            this.Icon = new Icon("TorPdos.ico");

            lblPortal.Location = new Point(20, 20);
            lblPortal.Height = 200;
            lblPortal.Width = 200;
            lblPortal.Font = new Font("Arial", 12, FontStyle.Regular);
            lblPortal.Text = "TorPdos for days";
            lblPortal.ForeColor = ColorTranslator.FromHtml("#0B5563");

            Controls.Add(lblPortal);

        }
    }
}