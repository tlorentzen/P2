using System.Drawing;
using System.Windows.Forms;
using ID_lib;

namespace TorPdos{
    

    public class MyForm : Form{
        private static readonly string backgroundColour = "#320117", lblColour = "#CC7178", btnColour = "#FFF8F7", txtColour = "#FFD9DA";
        Label lblUsername = new Label{
            Location = new Point(20, 20),
            Height = 40, Width = 150,
            Font = new Font("Consolas", 20, FontStyle.Regular),
            Text = "UserID",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblPassword = new Label(){
            Location = new Point(20, 70),
            Height = 40, Width = 150,
            Font = new Font("Consolas", 20, FontStyle.Regular),
            Text = "Password:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblYouDidIt = new Label(){
            Location = new Point(100, 100),
            Height = 40, Width = 200,
            Text = "You did it o/",
            ForeColor = ColorTranslator.FromHtml(lblColour),
            Font = new Font("Consolas", 20, FontStyle.Regular)
        };
        Label lblNope = new Label(){
            Location = new Point(50, 110),
            Height = 40, Width = 350,
            Text = "Wrong username or password",
            ForeColor = ColorTranslator.FromHtml(btnColour),
            Font = new Font("Consolas", 15, FontStyle.Regular)
        };
        Label lblCreate = new Label(){
            Location = new Point(275, 240),
            Height = 100, Width = 200,
            Text = "Create new user",
            ForeColor = ColorTranslator.FromHtml(btnColour),
            Font = new Font("Consolas", 8, FontStyle.Regular)
        };
        Label lblKage= new Label{
            Location = new Point(20, 20),
            Height = 40,
            Width = 150,
            Font = new Font("Consolas", 20, FontStyle.Regular),
            Text = "UserID",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        TextBox txtUsername = new TextBox(){
            Location = new Point(170, 20),
            Height = 50,Width = 150,
            Font = new Font("Consolas", 15, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtPassword = new TextBox(){
            Location = new Point(170, 70),
            Height = 50, Width = 150,
            Font = new Font("Consolas", 15, FontStyle.Regular),
            PasswordChar = '*',
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        Button btnLogin = new Button(){
            Location = new Point(70, 150),
            Width = 250, Height = 70,
            Text = "Login",
            Font = new Font("Consolas", 25, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnExisting = new Button(){
            Location = new Point(70, 50),
            Width = 250, Height = 70,
            Text = "Existing User",
            Font = new Font("Consolas", 25, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnNew = new Button(){
            Location = new Point(70, 150),
            Width = 250, Height = 70,
            Text = "New User",
            Font = new Font("Consolas", 25, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnCreate = new Button()
        {
            Location = new Point(70, 150),
            Width = 250, Height = 70,
            Text = "Create",
            Font = new Font("Consolas", 25, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        NotifyIcon noiTorPdos = new NotifyIcon() {
            Text = "TorPdos",
            Icon = new Icon("TorPdos.ico"),
            Visible = false,
        };

        public MyForm(){
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            Width = 400;
            Height = 300;
            Name = "TorPdos";
            Text = "TorPdos";
            ResumeLayout(false);
            BackColor = ColorTranslator.FromHtml(backgroundColour);
            Icon = new Icon("TorPdos.ico");


            Controls.Add(btnExisting);
            Controls.Add(btnNew);

            btnExisting.Click += BtnExisting_Click;
            btnNew.Click += BtnNew_Click;
            btnLogin.Click += BtnClickLogin;
            btnCreate.Click += BtnCreate_Click;
            noiTorPdos.DoubleClick += noiTorPdosDoubleClick;
            Resize += MyformResize;
        }

        public void Login(){
            Controls.Clear();
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);
        }
        private void BtnCreate_Click(object sender, System.EventArgs e)
        {
            string uuid = IDHandler.CreateUser(txtPassword.Text);
            
            Login();
        }

        private void BtnNew_Click(object sender, System.EventArgs e)
        {
            Controls.Clear();
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnCreate);
        }

        private void BtnExisting_Click(object sender, System.EventArgs e){
            Login();
        }

        void BtnClickLogin(object sender, System.EventArgs e){
            string uuid = txtUsername.Text, pass = txtPassword.Text;
            if (IDHandler.IsValidUser(uuid, pass)){
                Controls.Clear();
                Controls.Add(lblYouDidIt);
                
            } else{
                Controls.Add(lblNope);
            }
        }

        void MyformResize(object sender, System.EventArgs e){
            if (this.WindowState == FormWindowState.Minimized){
                Hide();
                noiTorPdos.Visible = true;
            } else if(this.WindowState == FormWindowState.Normal){
                noiTorPdos.Visible = false;
            }
        }

        void noiTorPdosDoubleClick(object sender, System.EventArgs e){
            this.Show();
            noiTorPdos.Visible = false;
            WindowState = FormWindowState.Normal;
        }
    }
}