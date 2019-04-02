using System.Drawing;
using System.Windows.Forms;

namespace TorPdos
{
    public class MyForm : Form
    {
        Label lblUsername = new Label
        {
            Location = new Point(20, 20),
            Height = 40, Width = 200,
            Font = new Font("Consolas", 20, FontStyle.Regular),
            Text = "Username",
            ForeColor = ColorTranslator.FromHtml("#CC7178")
        };

        Label lblPassword = new Label()
        {
            Location = new Point(20, 70),
            Height = 40, Width = 200,
            Font = new Font("Consolas", 20, FontStyle.Regular),
            Text = "Password:",
            ForeColor = ColorTranslator.FromHtml("#CC7178")
        };
        Label lblYouDidIt = new Label()
        {
            Location = new Point(100, 100),
            Height = 40, Width = 200,
            Text = "You did it o/",
            ForeColor = ColorTranslator.FromHtml("#CC7178"),
            Font = new Font("Consolas", 20, FontStyle.Regular)
        };
        Label lblNope = new Label()
        {
            Location = new Point(60, 110),
            Height = 40, Width = 300,
            Text = "Wrong username or password",
            ForeColor = ColorTranslator.FromHtml("#FFF8F7"),
            Font = new Font("Consolas", 15, FontStyle.Regular)
        };
        TextBox txtUsername = new TextBox()
        {
            Location = new Point(170, 20),
            Height = 50,Width = 150,
            Font = new Font("Consolas", 15, FontStyle.Regular),
            MaxLength = 15,
            ForeColor = ColorTranslator.FromHtml("#320117"),
            BackColor = ColorTranslator.FromHtml("#FFD9DA")
        };
        TextBox txtPassword = new TextBox()
        {
            Location = new Point(170, 70),
            Height = 50, Width = 150,
            Font = new Font("Consolas", 15, FontStyle.Regular),
            MaxLength = 20,
            PasswordChar = '*',
            ForeColor = ColorTranslator.FromHtml("#320117"),
            BackColor = ColorTranslator.FromHtml("#FFD9DA")
        };
        Button btnLogin = new Button()
        {
            Location = new Point(70, 150),
            Width = 250,
            Height = 70,
            Text = "Login",
            Font = new Font("Consolas", 25, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml("#F3E1DD")
        };

        public MyForm()
        {
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            Width = 400;
            Height = 300;
            Name = "TorPdos";
            Text = "TorPdos";
            ResumeLayout(false);
            BackColor = ColorTranslator.FromHtml("#320117");
            Icon = new Icon("TorPdos.ico");

            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);

            btnLogin.Click += new System.EventHandler(BtnClickLogin);
        }

        public void BtnClickLogin(object sender, System.EventArgs e)
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