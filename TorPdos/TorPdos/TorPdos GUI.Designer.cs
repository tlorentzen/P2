﻿using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ID_lib;
using System.Configuration;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace TorPdos{

    public class MyForm : Form{
        private static readonly string backgroundColour = "#320117",
            lblColour = "#CC7178",
            btnColour = "#FFF8F7",
            txtColour = "#FFD9DA";
        private static RegistryKey MyReg = Registry.CurrentUser.OpenSubKey("TorPdos\\TorPdos\\TorPdos\\1.2.1.1",true);

        private string path;
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
            Height = 40, Width = 250,
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
        Label lblGoBack = new Label{
            Location = new Point(300, 230),
            Height = 40,
            Width = 150,
            Font = new Font("Consolas", 12, FontStyle.Regular),
            Text = "Go Back",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblOkay = new Label(){
            Location = new Point(300, 230),
            Height = 40,
            Width = 150,
            Font = new Font("Consolas", 12, FontStyle.Regular),
            Text = "Okay",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        TextBox txtUsername = new TextBox(){
            Location = new Point(170, 20),
            Height = 50, Width = 150,
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
        TextBox txtPath = new TextBox(){
            Location = new Point(70, 70),
            Height = 50, Width = 250,
            Font = new Font("Consolas", 15, FontStyle.Regular),
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
        Button btnCreate = new Button(){
            Location = new Point(70, 150),
            Width = 250, Height = 70,
            Text = "Create",
            Font = new Font("Consolas", 25, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnBrowse = new Button(){
            Location = new Point(70, 120),
            Width = 250, Height = 70,
            Text = "Browse",
            Font = new Font("Consolas", 25, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour),
            //Image =
        };
        NotifyIcon noiTorPdos = new NotifyIcon(){
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


            if (MyReg.GetValue("Path") == null)
            {
                FirstStartUp(); 
            } else{
                path = MyReg.GetValue("Path").ToString()+"\\.hidden";
                Login(); 
            }

            noiTorPdos.DoubleClick += noiTorPdosDoubleClick;
            Resize += MyformResize;
            
            
            
        }

        private void LblOkayClick(object sender, System.EventArgs e){
            MyReg.SetValue("Path", PathName()+"\\");
            path = PathName()+@"\.hidden";
            
            if(!Directory.Exists(PathName() + @"\.hidden"))
            {
                DirectoryInfo di = Directory.CreateDirectory(PathName() + @"\.hidden");
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            Controls.Clear();
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnCreate);
            Controls.Add(lblGoBack);
            btnCreate.Click += BtnCreateClick;
            AcceptButton = btnCreate;
        }

        private void BtnBrowseClick(object sender, System.EventArgs e){
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()){
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtPath.Text = fbd.SelectedPath;
            }
        }

        private void LblGoBackClick(object sender, System.EventArgs e){
            FirstStartUp();
        }

        public void Login(){  
            Controls.Clear();
            txtUsername.Text = IDHandler.GetUUID(path);
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            txtPassword.Text = "";
            Controls.Add(btnLogin);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);
            Controls.Add(lblGoBack);
            btnLogin.Click += BtnClickLogin;
            lblGoBack.Click += LblGoBackClick;
            AcceptButton = btnLogin;
        }

        public void FirstStartUp(){
            lblOkay.Click += LblOkayClick;
            btnBrowse.Click += BtnBrowseClick;
            Controls.Clear();
            Controls.Add(btnBrowse);
            Controls.Add(txtPath);
            Controls.Add(lblOkay);
        }
        private void BtnCreateClick(object sender, System.EventArgs e){
            string uuid = IDHandler.CreateUser(path, txtPassword.Text);
            Login();
            if (MyReg.GetValue("UUID") == null) return;
            txtUsername.Text = MyReg.GetValue("UUID").ToString();
        }

        void BtnClickLogin(object sender, System.EventArgs e){
            string uuid = txtUsername.Text, pass = txtPassword.Text;
            if (IDHandler.IsValidUser(path, uuid, pass)){
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
            } else if (this.WindowState == FormWindowState.Normal){
                noiTorPdos.Visible = false;
            }
        }

        void noiTorPdosDoubleClick(object sender, System.EventArgs e){
            Show();
            noiTorPdos.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        public string PathName(){
            return txtPath.Text;
        }
    }
}
