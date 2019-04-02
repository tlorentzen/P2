using System.Drawing;
using System.Windows.Forms;

namespace TorPdos
{
    public class MyForm : Form
    {
        Label lblUsername = new Label {
            Location = new Point(20, 20),
            Height = 40, Width = 200,
            Font = new Font("Arial", 20, FontStyle.Regular),
            Text = "Username",
            ForeColor = ColorTranslator.FromHtml("#CC7178")
        };
        Label lblPassword = new Label();
        Label lblYouDidIt = new Label();
        Label lblNope = new Label();
        TextBox txtUsername = new TextBox();
        TextBox txtPassword = new TextBox();
        Button btnLogin = new Button();
        Button btnTryAgain = new Button();

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
            this.BackColor = ColorTranslator.FromHtml("#320117");
            this.Icon = new Icon("TorPdos.ico");

            txtUsername.Location = new Point(170, 20);
            txtUsername.Height = 50;
            txtUsername.Width = 150;
            txtUsername.Font = new Font("Arial", 15, FontStyle.Regular);
            txtUsername.MaxLength = 15;
            txtUsername.ForeColor = ColorTranslator.FromHtml("#320117");
            txtUsername.BackColor = ColorTranslator.FromHtml("#FFD9DA");

            lblPassword.Location = new Point(20, 70);
            lblPassword.Height = 40;
            lblPassword.Width = 200;
            lblPassword.Font = new Font("Arial", 20, FontStyle.Regular);
            lblPassword.Text = "Password:";
            lblPassword.ForeColor = ColorTranslator.FromHtml("#CC7178");

            txtPassword.Location = new Point(170, 70);
            txtPassword.Height = 50;
            txtPassword.Width = 150;
            txtPassword.Font = new Font("Arial", 15, FontStyle.Regular);
            txtPassword.MaxLength = 15;
            txtPassword.PasswordChar = '*';
            txtPassword.ForeColor = ColorTranslator.FromHtml("#320117");
            txtPassword.BackColor = ColorTranslator.FromHtml("#FFD9DA");

            btnLogin.Location = new Point(70, 150);
            btnLogin.Width = 250;
            btnLogin.Height = 70;
            btnLogin.Text = "Login";
            btnLogin.Font = new Font("Arial", 25, FontStyle.Regular);
            btnLogin.ForeColor = ColorTranslator.FromHtml("#F3E1DD");
            btnLogin.Click += new System.EventHandler(BtnClickLogin);


            lblYouDidIt.Location = new Point(100, 100);
            lblYouDidIt.Height = 40;
            lblYouDidIt.Width = 200;
            lblYouDidIt.Text = "You did it o/";
            lblYouDidIt.ForeColor = ColorTranslator.FromHtml("#CC7178");
            lblYouDidIt.Font = new Font("Arial", 20, FontStyle.Regular);

            lblNope.Location = new Point(60, 110);
            lblNope.Height = 40;
            lblNope.Width = 300;
            lblNope.Text = "Wrong username or password";
            lblNope.ForeColor = ColorTranslator.FromHtml("#FFF8F7");
            lblNope.Font = new Font("Arial", 15, FontStyle.Regular);

            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);
        }

        void BtnClickLogin(object sender, System.EventArgs e)
        {
            if (txtPassword.Text == "Password" && txtUsername.Text == "Admin")
            {
                Controls.Add(lblYouDidIt);
                Controls.Remove(btnLogin);
                Controls.Remove(txtPassword);
                Controls.Remove(txtUsername);
                Controls.Remove(lblUsername);
                Controls.Remove(lblPassword);
                Controls.Remove(lblNope);
            } else
            {
                Controls.Add(lblNope);
            }
        }
    }
}