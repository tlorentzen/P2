using System.Drawing;
using System.Windows.Forms;

namespace TorPdos
{
    public class MyForm : Form
    {
        Label lblUsername = new Label();
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
            this.BackColor = ColorTranslator.FromHtml("#A2BCE0");
            this.Icon = new Icon("TorPdos.ico");

            lblUsername.Location = new Point(20, 20);
            lblUsername.Height = 40;
            lblUsername.Width = 200;
            lblUsername.Font = new Font("Arial", 20, FontStyle.Regular);
            lblUsername.Text = "Username:";
            lblUsername.ForeColor = ColorTranslator.FromHtml("#0B5563");

            txtUsername.Location = new Point(170, 20);
            txtUsername.Height = 50;
            txtUsername.Width = 150;
            txtUsername.Font = new Font("Arial", 15, FontStyle.Regular);
            txtUsername.MaxLength = 15;
            txtUsername.ForeColor = ColorTranslator.FromHtml("#5299D3");

            lblPassword.Location = new Point(20, 70);
            lblPassword.Height = 40;
            lblPassword.Width = 200;
            lblPassword.Font = new Font("Arial", 20, FontStyle.Regular);
            lblPassword.Text = "Password:";
            lblPassword.ForeColor = ColorTranslator.FromHtml("#0B5563");

            txtPassword.Location = new Point(170, 70);
            txtPassword.Height = 50;
            txtPassword.Width = 150;
            txtPassword.Font = new Font("Arial", 15, FontStyle.Regular);
            txtPassword.MaxLength = 15;
            txtPassword.PasswordChar = '*';
            txtPassword.ForeColor = ColorTranslator.FromHtml("#5299D3");

            btnLogin.Location = new Point(70, 120);
            btnLogin.Width = 250;
            btnLogin.Height = 100;
            btnLogin.Text = "Login";
            btnLogin.Font = new Font("Arial", 25, FontStyle.Regular);
            btnLogin.ForeColor = ColorTranslator.FromHtml("#5E5C6C");
            btnLogin.Click += new System.EventHandler(BtnClickLogin);


            lblYouDidIt.Location = new Point(100, 100);
            lblYouDidIt.Height = 40;
            lblYouDidIt.Width = 200;
            lblYouDidIt.Text = "You did it o/";
            lblYouDidIt.ForeColor = ColorTranslator.FromHtml("#0B5563");
            lblYouDidIt.Font = new Font("Arial", 20, FontStyle.Regular);

            lblNope.Location = new Point(60, 50);
            lblNope.Height = 40;
            lblNope.Width = 300;
            lblNope.Text = "Wrong username or password";
            lblNope.ForeColor = ColorTranslator.FromHtml("#0B5563");
            lblNope.Font = new Font("Arial", 15, FontStyle.Regular);

            btnTryAgain.Location = new Point(70, 120);
            btnTryAgain.Width = 250;
            btnTryAgain.Height = 100;
            btnTryAgain.Text = "Try Again";
            btnTryAgain.Font = new Font("Arial", 25, FontStyle.Regular);
            btnTryAgain.ForeColor = ColorTranslator.FromHtml("#5E5C6C");
            btnTryAgain.Click += new System.EventHandler(BtnClickTryAgain);

            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);
        }

        void BtnClickLogin(object sender, System.EventArgs e)
        {
            if (txtPassword.Text == "Welcome" && txtUsername.Text == "Hello")
            {
                Controls.Add(lblYouDidIt);
                Controls.Remove(btnLogin);
                Controls.Remove(txtPassword);
                Controls.Remove(txtUsername);
                Controls.Remove(lblUsername);
                Controls.Remove(lblPassword);
            } else
            {
                Controls.Add(lblNope);
                Controls.Add(btnTryAgain);
                Controls.Remove(btnLogin);
                Controls.Remove(txtPassword);
                Controls.Remove(txtUsername);
                Controls.Remove(lblUsername);
                Controls.Remove(lblPassword);
            }
        }

        void BtnClickTryAgain(object sender, System.EventArgs e)
        {
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);
            Controls.Remove(lblNope);
            Controls.Remove(btnTryAgain);
        }
    }
}