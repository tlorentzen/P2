using System.Drawing;
using System.Windows.Forms;

namespace TorPdos
{
    public class MyForm : Form
    {
        Label lblUsername = new Label();
        Label lblPassword = new Label();
        TextBox txtUsername = new TextBox();
        TextBox txtPassword = new TextBox();
        Button btnLogIn = new Button();

        public MyForm()
        {
            this.SuspendLayout();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.Width = 400;
            this.Height = 300;
            this.Name = "TorPdos";
            this.Text = "TorPdos";
            this.ResumeLayout(false);
            this.BackColor = ColorTranslator.FromHtml("#A2BCE0");
            this.Icon = new Icon("TorPdos.ico");

            lblUsername.Location = new Point(20, 20);
            lblUsername.Height = 40;
            lblUsername.Width = 200;
            lblUsername.Font = new Font("Arial", 20, FontStyle.Regular);
            lblUsername.Text = "Username:";
            lblUsername.ForeColor = ColorTranslator.FromHtml("#0B5563");

            txtUsername.Location = new Point(170, 27);
            txtUsername.Height = 50;
            txtUsername.Width = 150;
            txtUsername.MaxLength = 15;

            lblPassword.Location = new Point(20, 70);
            lblPassword.Height = 40;
            lblPassword.Width = 200;
            lblPassword.Font = new Font("Arial", 20, FontStyle.Regular);
            lblPassword.Text = "Password:";
            lblPassword.ForeColor = ColorTranslator.FromHtml("#0B5563");

            txtPassword.Location = new Point(170, 77);
            txtPassword.Height = 50;
            txtPassword.Width = 150;
            txtPassword.MaxLength = 15;
            txtPassword.PasswordChar = '*';

           // btnLogIn.Location = new Point()

            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);

        }
    }
}