﻿using System;
 using System.Drawing;
 using System.IO;
 using System.Windows.Forms;
 using ID_lib;
 using Microsoft.Win32;
using Index_lib;

 namespace TorPdos{

    public class MyForm : Form{
        private static readonly string backgroundColour = "#FBF9FF",
            lblColour = "#B3B7EE",
            btnColour = "#A2A3BB",
            txtColour = "#9395D3";
        private static RegistryKey MyReg = Registry.CurrentUser.OpenSubKey("TorPdos\\1.1.1.1",true);
        public bool loggedIn = false;

        private string path;
        Label lblUsername = new Label{
            Location = new Point(20, 20),
            Height = 40, Width = 150,
            Font = new Font("Consolas", 20, FontStyle.Regular),
            Text = "UserID",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblPassword = new Label
        {
            Location = new Point(20, 20),
            Height = 40,
            Width = 150,
            Font = new Font("Consolas", 20, FontStyle.Regular),
            Text = "Password",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblConfirmPassword = new Label(){
            Location = new Point(20, 70),
            Height = 40, Width = 150,
            Font = new Font("Consolas", 20, FontStyle.Regular),
            Text = "Confirm:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblLoginPassword = new Label()
        {
            Location = new Point(20, 70),
            Height = 40,
            Width = 150,
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
        Label lblNope2 = new Label()
        {
            Location = new Point(50, 110),
            Height = 40,
            Width = 350,
            Text = "Passwords do not match",
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
        Label lblOkay = new Label()
        {
            Location = new Point(320, 230),
            Height = 40,
            Width = 80,
            Font = new Font("Consolas", 12, FontStyle.Regular),
            Text = "Okay",
            ForeColor = ColorTranslator.FromHtml(lblColour),
        };

        Label lblLogOut = new Label
        {
            Location = new Point(300, 230),
            Height = 40,
            Width = 150,
            Font = new Font("Consolas", 12, FontStyle.Regular),
            Text = "Log out",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        TextBox txtUsername = new TextBox(){
            Location = new Point(170, 20),
            Height = 50, Width = 150,
            Font = new Font("Consolas", 15, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtPassword = new TextBox()
        {
            Location = new Point(170, 20),
            Height = 50, Width = 150,
            Font = new Font("Consolas", 15, FontStyle.Regular),
            PasswordChar = '*',
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtConfirmPassword = new TextBox(){
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
            BackColor = ColorTranslator.FromHtml(txtColour),
            Text = ""
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
        Button btnDownload = new Button()
        {
            Location = new Point(70, 99),
            Width = 250, Height = 70,
            Text = "Donwnload",
            Font = new Font("Consolas", 25, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour),
        };
        NotifyIcon noiTorPdos = new NotifyIcon(){
            Text = "TorPdos",
            Icon = new Icon("TorPdos.ico"),
            Visible = true,
        };
        CheckBox chkCreateFolder = new CheckBox()
        {
            Text = "Make a new folder?",
            Location = new Point(120, 150),
            Font = new Font("Consolas", 12, FontStyle.Regular),
            Height = 100, Width = 200,
            ForeColor = ColorTranslator.FromHtml(lblColour),
            Checked = true
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
                Login(); 
            }

            noiTorPdos.Click += noiTorPdosClick;
            FormClosing += MyFormClosing;
        }
        private void BtnCreateClick(object sender, EventArgs e)
        {
            if (txtPassword.Text == txtConfirmPassword.Text)
            {
                string uuid = IdHandler.createUser(MyReg.GetValue("Path").ToString() + "\\.hidden", txtPassword.Text);
                Login();
                if (MyReg.GetValue("UUID") == null) return;
                txtUsername.Text = MyReg.GetValue("UUID").ToString();
            }
            else
            {
                Controls.Add(lblNope2);
            }
        }

        void BtnClickLogin(object sender, EventArgs e)
        {
            string uuid = txtUsername.Text, pass = txtConfirmPassword.Text;
            if (IdHandler.isValidUser(MyReg.GetValue("Path").ToString() + "\\.hidden", uuid, pass))
            {
                LoggedIn();
                loggedIn = true;
            }
            else
            {
                Controls.Add(lblNope);
            }
        }
        private void LblOkayClick(object sender, EventArgs e){

            string hiddenPath = PathName() + @"\.hidden", newPath = PathName() + @"\TorPdos";
            if(Directory.Exists(PathName()) == true)
            {
                if (!Directory.Exists(hiddenPath) && chkCreateFolder.Checked == false)
                {
                    MyReg.SetValue("Path", PathName() + "\\");
                    HiddenFolder dih = new HiddenFolder(hiddenPath);
                }
                else if (chkCreateFolder.Checked == true)
                {
                    DirectoryInfo di = Directory.CreateDirectory(newPath);
                    MyReg.SetValue("Path", newPath + "\\");
                    HiddenFolder dih = new HiddenFolder(newPath + @"\.hidden");
                }
                Create();
            }
            
        }

        private void BtnBrowseClick(object sender, EventArgs e){
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()){
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtPath.Text = fbd.SelectedPath;
            }
        }

        private void LblGoBackClick(object sender, EventArgs e){
            FirstStartUp();
        }

        public void Login(){  
            Controls.Clear();
            txtUsername.Text = IdHandler.getUuid(MyReg.GetValue("Path").ToString() + "\\.hidden");
            Controls.Add(txtUsername);
            txtConfirmPassword.Text = null;
            Controls.Add(txtConfirmPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblUsername);
            Controls.Add(lblLoginPassword);
            Controls.Add(lblGoBack);
            btnLogin.Click += BtnClickLogin;
            lblGoBack.Click += LblGoBackClick;
            AcceptButton = btnLogin;
        }

        public void FirstStartUp(){
            btnBrowse.Click += BtnBrowseClick;
            Controls.Clear();
            Controls.Add(btnBrowse);
            Controls.Add(txtPath);
            Controls.Add(lblOkay);
            lblOkay.Click += LblOkayClick;
            Controls.Add(chkCreateFolder);
        }

        public void Create()
        {
            Controls.Clear(); 
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(lblConfirmPassword);
            Controls.Add(txtConfirmPassword);
            Controls.Add(btnCreate);
            Controls.Add(lblGoBack);
            lblGoBack.Click += LblGoBackClick;
            btnCreate.Click += BtnCreateClick;
            AcceptButton = btnCreate;
        }
        
        public void LoggedIn()
        {
            Controls.Clear();
            Controls.Add(btnDownload);
            Controls.Add(lblLogOut);
            lblLogOut.Click += LblLogOutClick;
        }

        private void LblLogOutClick(object sender, EventArgs e)
        {
            Login();
            loggedIn = false;
        }   

        void MyFormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing && loggedIn == true)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                noiTorPdos.Visible = false;
                Environment.Exit(0);
            }

        }

        void noiTorPdosClick(object sender, EventArgs e){
            Show();
            WindowState = FormWindowState.Normal;
        }

        public string PathName(){
            return txtPath.Text;
        }
    }
}
