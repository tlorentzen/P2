﻿using System;
 using System.Drawing;
 using System.IO;
 using System.Windows.Forms;
 using ID_lib;
 using Microsoft.Win32;
using Index_lib;

 namespace TorPdos{

    public class MyForm : Form{
        private static readonly string
            logoColour = "#4D5762",
            backgroundColour = "#FBF9FF",
            lblColour = logoColour,//"#B3B7EE",
            btnColour = logoColour,//"#A2A3BB",
            txtColour = logoColour,//"#9395D3",
            errorColour = "#B20808";
        private static readonly int
            //TorPdos
            fullWidth = 400,
            fullHeight = 300,
            //Box
            boxWidth = 325,
            boxBtnWidth = 85,
            //Confirm Buttons
            confirmBtnW = 100,
            confirmBtnH = 27,
            //Positions
            leftAlign = 30,
            posFirst = 20,
            posSecond = posFirst + 50,
            posThird = posSecond + 50,
            posConfirmW = fullWidth - confirmBtnW - 30,
            posConfirmH = fullHeight - confirmBtnH - 53,
            posCancelW = posConfirmW - confirmBtnW - 15,
            //Text
            textSizeDefault = 12,
            textSizeInput = 14,
            textSizeBtn = textSizeDefault; //25
        private static RegistryKey MyReg = Registry.CurrentUser.OpenSubKey("TorPdos\\1.1.1.1",true);
        public bool loggedIn = false;

        Label lblUsername = new Label{
            Location = new Point(leftAlign, posFirst),
            Height = 25, Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "User ID:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblPassword = new Label
        {
            Location = new Point(leftAlign, posFirst),
            Height = 25, Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "Password:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblConfirmPassword = new Label(){
            Location = new Point(leftAlign, posSecond),
            Height = 25, Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "Confirm password:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblLoginPassword = new Label()
        {
            Location = new Point(leftAlign, posSecond),
            Height = 25,
            Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "Password:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblBrowse = new Label()
        {
            Location = new Point(leftAlign, posSecond),
            Height = 20,
            Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "Choose path:",
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
            Location = new Point(leftAlign, posThird),
            Height = 40, Width = 350,
            Text = " * Invalid username or password.",
            ForeColor = ColorTranslator.FromHtml(errorColour),
            Font = new Font("Consolas", textSizeDefault, FontStyle.Regular)
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
            Location = new Point(posCancelW, posConfirmH),
            Height = 40,
            Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Regular),
            Text = "Change path",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblOkay = new Label()
        {
            Location = new Point(320, 230),
            Height = 40,
            Width = 80,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Regular),
            Text = "Okay",
            ForeColor = ColorTranslator.FromHtml(lblColour),
        };

        Label lblLogOut = new Label
        {
            Location = new Point(300, 230),
            Height = 40,
            Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Regular),
            Text = "Log out",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        TextBox txtUsername = new TextBox(){
            Location = new Point(leftAlign, posFirst + 20),//HERE
            Height = 45, Width = boxWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtPassword = new TextBox()
        {
            Location = new Point(leftAlign, posFirst + 20),
            Height = 45, Width = boxWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            PasswordChar = '*',
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtConfirmPassword = new TextBox(){
            Location = new Point(leftAlign, posSecond + 20),
            Height = 45, Width = boxWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            PasswordChar = '*',
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtPath = new TextBox(){
            Location = new Point(leftAlign, posSecond + 20),
            Height = 45, Width = boxWidth - boxBtnWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour),
            Text = ""
        };
        Button btnConfirmPath = new Button()
        {
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW, Height = confirmBtnH,
            Text = "Confirm",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnLogin = new Button(){
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW, Height = confirmBtnH,
            Text = "Login",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnChangePath = new Button()
        {
            Location = new Point(posCancelW, posConfirmH),
            Width = confirmBtnW,
            Height = confirmBtnH,
            Text = "Back",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnCreate = new Button(){
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW, Height = confirmBtnH,
            Text = "Create",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnBrowse = new Button(){
            Location = new Point(leftAlign + boxWidth - boxBtnWidth, posSecond + 20),
            Width = boxBtnWidth, Height = 27,
            Text = "Browse",
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour),
        };
        Button btnDownload = new Button()
        {
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW, Height = confirmBtnH,
            Text = "Donwnload",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour),
        };
        Button btnLogout = new Button()
        {
            Location = new Point(posCancelW, posConfirmH),
            Width = confirmBtnW,
            Height = confirmBtnH,
            Text = "Logout",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour),
        };
        NotifyIcon noiTorPdos = new NotifyIcon(){
            Text = "TorPdos",
            Icon = new Icon("TorPdos.ico"),
            Visible = true,
        };
        CheckBox chkCreateFolder = new CheckBox()
        {
            Text = "Create new folder?",
            Location = new Point(leftAlign, posThird),
            Font = new Font("Consolas", textSizeDefault, FontStyle.Regular),
            Height = 25, Width = 200,
            ForeColor = ColorTranslator.FromHtml(lblColour),
            Checked = true
        };

        public MyForm(){
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            Width = fullWidth;
            Height = fullHeight;
            Name = "TorPdos";
            Text = "TorPdos";
            ResumeLayout(false);
            BackColor = ColorTranslator.FromHtml(backgroundColour);
            Icon = new Icon("TorPdos.ico");


            if (MyReg.GetValue("Path") == null)
            {
                FirstStartUp(); 
            } else if(MyReg.GetValue("UUID") == null){
                Create();
            } else{
                Login();
            }

            EventHandlers();
        }

        void EventHandlers()
        {
            noiTorPdos.Click += noiTorPdosClick;
            FormClosing += MyFormClosing;
            btnLogin.Click += BtnClickLogin;
            btnBrowse.Click += BtnBrowseClick;
            btnCreate.Click += BtnCreateClick;
            //Replaced:
            //lblGoBack.Click += LblGoBackClick;
            //lblLogOut.Click += LblLogOutClick;
            //lblGoBack.Click += LblGoBackClick;
            //lblOkay.Click += LblOkayClick;
            //With these:
            btnConfirmPath.Click += LblOkayClick;
            btnChangePath.Click += LblGoBackClick;
            btnLogout.Click += LblLogOutClick;
        }
        private void BtnCreateClick(object sender, EventArgs e)
        {
            if (txtPassword.Text == txtConfirmPassword.Text)
            {
                IdHandler.createUser(MyReg.GetValue("Path").ToString() + @"\.hidden\", txtPassword.Text, "NSA_Surveillance_Van_#0216");
                Login();
                if (MyReg.GetValue("UUID") == null) return;
                txtUsername.Text = MyReg.GetValue("UUID").ToString();
            } else{
                Controls.Add(lblNope2);
            }
        }

        void BtnClickLogin(object sender, EventArgs e)
        {
            string uuid = txtUsername.Text, pass = txtConfirmPassword.Text;
            if (IdHandler.isValidUser(MyReg.GetValue("Path").ToString() + @"\.hidden\", uuid, pass))
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

            string hiddenPath = PathName() + @".hidden\", newPath = PathName() + @"TorPdos\";
            if(Directory.Exists(PathName()) == true)
            {
                if (!Directory.Exists(hiddenPath) && chkCreateFolder.Checked == false)
                {
                    MyReg.SetValue("Path", PathName());
                    HiddenFolder dih = new HiddenFolder(hiddenPath);
                }
                else if(Directory.Exists(hiddenPath) && chkCreateFolder.Checked == false)
                {
                    MyReg.SetValue("Path", PathName());
                }
                else if (chkCreateFolder.Checked == true)
                {
                    DirectoryInfo di = Directory.CreateDirectory(newPath);
                    if (MyReg.GetValue("Path").ToString().EndsWith(@"\") == true)
                    {
                        MyReg.SetValue("Path", PathName() + @"\");
                    }
                    else
                    {
                        MyReg.SetValue("Path", newPath + @"\");
                    }
                    
                    HiddenFolder dih = new HiddenFolder(newPath + @".hidden\");
                }

                if (IdHandler.userExists(newPath + @".hidden") || IdHandler.userExists(hiddenPath) == true)
                {
                    Login();
                }
                else
                {
                    Create();
                }
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
            chkCreateFolder.Checked = false;
        }

        public void Login(){  
            Controls.Clear();
            int tabNumber = 0;

            //TXT: Username
            txtUsername.TabIndex = tabNumber++;
            Controls.Add(txtUsername);

            //TXT: Confirm password
            txtConfirmPassword.TabIndex = tabNumber++;
            txtConfirmPassword.Text = null;
            Controls.Add(txtConfirmPassword);

            //BTN: Login
            btnLogin.TabIndex = tabNumber++;
            Controls.Add(btnLogin);

            //BTN: Back
            btnChangePath.TabIndex = tabNumber++;
            Controls.Add(btnChangePath);

            //Labels
            Controls.Add(lblUsername);
            Controls.Add(lblLoginPassword);
            //Controls.Add(lblGoBack);
            
            AcceptButton = btnLogin;

            if(MyReg.GetValue("UUID") != null)
            {
                txtUsername.Text = MyReg.GetValue("UUID").ToString();
            }
        }

        public void FirstStartUp(){
            Controls.Clear();
            int tabNumber = 0;

            //TXT: Browse input
            Controls.Add(txtPath);
            txtPath.TabIndex = tabNumber++;
            
            //BTN: Browse menu
            btnBrowse.TabIndex = tabNumber++;
            Controls.Add(btnBrowse);

            //CHK: Folder option
            chkCreateFolder.TabIndex = tabNumber++;
            Controls.Add(chkCreateFolder);

            //Controls.Add(lblOkay);

            //BTN: Confirm path
            btnConfirmPath.TabIndex = tabNumber++;
            Controls.Add(btnConfirmPath);

            //Labels
            Controls.Add(lblBrowse);

            AcceptButton = btnConfirmPath;

            if (MyReg.GetValue("Path") != null)
            {
                txtPath.Text = MyReg.GetValue("Path").ToString();
            }
        }

        public void Create()
        {
            Controls.Clear();
            int tabNumber = 0;

            //TXT: Password
            txtPassword.TabIndex = tabNumber++;
            Controls.Add(txtPassword);

            //TXT: Confirm password
            txtConfirmPassword.TabIndex = tabNumber++;
            Controls.Add(txtConfirmPassword);

            //BTN: Create
            btnCreate.TabIndex = tabNumber++;
            Controls.Add(btnCreate);

            //Labels
            Controls.Add(lblPassword);
            Controls.Add(lblConfirmPassword);
            Controls.Add(lblGoBack);

            AcceptButton = btnCreate;
        }
        
        public void LoggedIn()
        {
            Controls.Clear();
            int tabNumber = 0;

            //BTN: ???
            btnDownload.TabIndex = tabNumber++;
            Controls.Add(btnDownload);

            //BTN: ??
            btnLogout.TabIndex = tabNumber++;
            Controls.Add(btnLogout);

            //Controls.Add(lblLogOut);      
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
            if(txtPath.Text.EndsWith(@"\"))
            {
                return txtPath.Text;
            }
            else
            {
                return txtPath.Text + @"\";
            }
            
        }
    }
}
