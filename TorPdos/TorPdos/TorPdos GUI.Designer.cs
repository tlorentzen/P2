using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Index_lib;
using P2P_lib;
using P2P_lib.Handlers;
using P2P_lib.Helpers;

namespace TorPdos
{ 
    public class MyForm : Form
    {
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
            boxHeight = 45,
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
        public bool loggedIn = false;

        public Network _p2P;
        public Index _idx;

        Label lblPassword = new Label
        {
            Location = new Point(leftAlign, posFirst),
            Height = 25,
            Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "Password:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblConfirmPassword = new Label()
        {
            Location = new Point(leftAlign, posSecond),
            Height = 25,
            Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "Confirm password:",
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
        Label lblYouDidIt = new Label()
        {
            Location = new Point(100, 100),
            Height = 40,
            Width = 250,
            Text = "You did it o/",
            ForeColor = ColorTranslator.FromHtml(lblColour),
            Font = new Font("Consolas", 20, FontStyle.Regular)
        };
        Label lblNope = new Label()
        {
            Location = new Point(leftAlign, posThird),
            Height = 40,
            Width = 350,
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
        Label lblCreate = new Label()
        {
            Location = new Point(275, 240),
            Height = 100,
            Width = 200,
            Text = "Create new user",
            ForeColor = ColorTranslator.FromHtml(btnColour),
            Font = new Font("Consolas", 8, FontStyle.Regular)
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
        Label lblUUID = new Label
        {
            Location = new Point(leftAlign, posFirst),
            Height = 25,
            Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "UUID:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        Label lblIP = new Label()
        {
            Location = new Point(leftAlign, posSecond),
            Height = 25,
            Width = 150,
            Font = new Font("Consolas", textSizeDefault, FontStyle.Bold),
            Text = "IP-address:",
            ForeColor = ColorTranslator.FromHtml(lblColour)
        };
        TextBox txtPassword = new TextBox()
        {
            Location = new Point(leftAlign, posFirst + 20),
            Height = boxHeight,
            Width = boxWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            PasswordChar = '*',
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtConfirmPassword = new TextBox()
        {
            Location = new Point(leftAlign, posSecond + 20),
            Height = boxHeight,
            Width = boxWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            PasswordChar = '*',
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtPath = new TextBox()
        {
            Location = new Point(leftAlign, posSecond + 20),
            Height = boxHeight,
            Width = boxWidth - boxBtnWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour),
            Text = ""
        };
        TextBox txtUUID = new TextBox()
        {
            Location = new Point(leftAlign, posFirst + 20),
            Height = boxHeight,
            Width = boxWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        TextBox txtIP = new TextBox()
        {
            Location = new Point(leftAlign, posSecond + 20),
            Height = boxHeight,
            Width = boxWidth,
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(backgroundColour),
            BackColor = ColorTranslator.FromHtml(txtColour)
        };
        Button btnConfirmPath = new Button()
        {
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW,
            Height = confirmBtnH,
            Text = "Confirm",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnLogin = new Button()
        {
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW,
            Height = confirmBtnH,
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
        Button btnCreate = new Button()
        {
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW,
            Height = confirmBtnH,
            Text = "Create",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnBrowse = new Button()
        {
            Location = new Point(leftAlign + boxWidth - boxBtnWidth, posSecond + 20),
            Width = boxBtnWidth,
            Height = 27,
            Text = "Browse",
            Font = new Font("Consolas", textSizeInput, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour),
        };
        Button btnLogout = new Button()
        {
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW,
            Height = confirmBtnH,
            Text = "Logout",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour),
        };
        Button btnOkay = new Button()
        {
            Location = new Point(posConfirmW, posConfirmH),
            Width = confirmBtnW,
            Height = confirmBtnH,
            Text = "Okay",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour)
        };
        Button btnAddPeer = new Button()
        {
            Location = new Point(posCancelW, posConfirmH),
            Width = confirmBtnW,
            Height = confirmBtnH,
            Text = "Add peer",
            Font = new Font("Consolas", textSizeBtn, FontStyle.Regular),
            ForeColor = ColorTranslator.FromHtml(btnColour),
        };
        NotifyIcon noiTorPdos = new NotifyIcon()
        {
            Text = "TorPdos",
            Icon = new Icon("TorPdos.ico"),
            Visible = false,
        };
        CheckBox chkCreateFolder = new CheckBox()
        {
            Text = "Create new folder?",
            Location = new Point(leftAlign, posThird),
            Font = new Font("Consolas", textSizeDefault, FontStyle.Regular),
            Height = 25,
            Width = 200,
            ForeColor = ColorTranslator.FromHtml(lblColour),
            Checked = true
        };


        public MyForm()
        {
            EventHandlers();
            GuiLayout();

            if(!string.IsNullOrEmpty(IdHandler.GetUuid()))
            {
                LoggedIn();
            }
            else
            {
                if (NetworkPorts.IsPortAvailable(25565) == false)
                {
                    Sorry();
                }
                else if (String.IsNullOrEmpty(DiskHelper.GetRegistryValue("Path")) == true || Directory.Exists(DiskHelper.GetRegistryValue("Path")) == false)
                {
                    FirstStartUp();
                }
                else if (File.Exists(DiskHelper.GetRegistryValue("Path") + @".hidden\userdata") == false)
                {
                    Create();
                }
                else
                {
                    Login();
                }
            }
            
            
        }

        void GuiLayout()
        {
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
        }
        void EventHandlers()
        {
            noiTorPdos.Click += noiTorPdosClick;
            FormClosing += MyFormClosing;
            btnLogin.Click += BtnLoginClick;
            btnBrowse.Click += BtnBrowseClick;
            btnCreate.Click += BtnCreateClick;
            btnConfirmPath.Click += BtnConfirmPathClick;
            btnChangePath.Click += BtnChangePathClick;
            btnLogout.Click += BtnLogOutClick;
            btnOkay.Click += BtnOkayClick;
            btnAddPeer.Click += BtnAddPeerClick;
        }

        void IndexEventHandlers()
        {
            _idx.FileAdded += IdxFileAdded;
            _idx.FileChanged += IdxFileChanged;
            _idx.FileDeleted += IdxFileDeleted;
            _idx.FileMissing += IdxFileMissing;
        }

        private void BtnDownloadClick(object sender, EventArgs e)
        {
            //_p2P.DownloadAllFiles();
        }

        private void BtnAddPeerClick(object sender, EventArgs e)
        {
            Controls.Clear();
            Controls.Add(txtUUID);
            Controls.Add(txtIP);
            Controls.Add(lblUUID);
            Controls.Add(lblIP);
            Controls.Add(btnOkay);
        }

        private void BtnOkayClick(object sender, EventArgs e)
        {
            _p2P?.AddPeer(txtUUID.Text, txtIP.Text);
            LoggedIn();
        }

        private void BtnCreateClick(object sender, EventArgs e)
        {
            if (txtPassword.Text == txtConfirmPassword.Text)
            {
                IdHandler.CreateUser(txtPassword.Text);
                Login();
            }
            else
            {
                Controls.Add(lblNope2);
            }
        }

        void BtnLoginClick(object sender, EventArgs e)
        {
            string pass = txtPassword.Text;
            string path = DiskHelper.GetRegistryValue("Path");
            if (IdHandler.IsValidUser(pass))
            {
                
                IdHandler.GetUuid(pass);
                LoggedIn();
                loggedIn = true;
                _idx = new Index(path);
                _idx.Load();
                _idx.Start();
                _p2P = new Network(25565, _idx, path);
                _p2P.Start();
                IndexEventHandlers();
                _idx.MakeIntegrityCheck();
                
                if (!_idx.Load())
                {
                    _idx.BuildIndex();
                }
            }
            else
            {
                Controls.Add(lblNope);
            }
        }
        private void BtnConfirmPathClick(object sender, EventArgs e)
        {

            string hiddenPath = PathName() + @".hidden\", newPath = PathName() + @"TorPdos\";
            if (Directory.Exists(PathName()) == true)
            {
                if (!Directory.Exists(hiddenPath) && chkCreateFolder.Checked == false)
                {
                    DiskHelper.SetRegistryValue("Path", PathName());
                    HiddenFolder dih = new HiddenFolder(hiddenPath);
                }
                else if (Directory.Exists(hiddenPath) && chkCreateFolder.Checked == false)
                {
                    DiskHelper.SetRegistryValue("Path", PathName());
                }
                else if (chkCreateFolder.Checked == true)
                {
                    DiskHelper.SetRegistryValue("Path", newPath);
                    DirectoryInfo di = Directory.CreateDirectory(newPath);
                    HiddenFolder dih = new HiddenFolder(newPath + @".hidden\");
                }

                if (IdHandler.UserExists())
                {
                    Login();
                }
                else
                {
                    Create();
                }
            }
        }
        private void BtnBrowseClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtPath.Text = fbd.SelectedPath;
            }
        }
        private void BtnChangePathClick(object sender, EventArgs e)
        {
            FirstStartUp();
            chkCreateFolder.Checked = false;
        }  
        private void BtnLogOutClick(object sender, EventArgs e)
        {
            _p2P?.SaveFile();
            _p2P?.Stop();
            _idx?.Save();
            _idx?.Stop();
            loggedIn = false;
            Login();
        }
        private void IdxFileMissing(IndexFile file)
        {
            Console.WriteLine(@"File missing init download of " + file.hash);
            _p2P.DownloadFile(file.hash);
        }

        private void IdxFileDeleted(string hash)
        {
            Console.WriteLine(@"Deleted: " + hash);
            _p2P.DeleteFile(hash);
        }

        private void IdxFileAdded(IndexFile idxfile)
        {
            Console.WriteLine(@"Added: " + idxfile.GetHash());

            P2PFile file = new P2PFile(idxfile.GetHash());
            file.AddPath(idxfile.paths);

            _p2P.UploadFile(file);
        }

        private void IdxFileChanged(IndexFile file)
        {
            Console.WriteLine(@"File changed: " + file.hash);
        }
        void MyFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && loggedIn == true)
            {
                e.Cancel = true;
                Hide();
                noiTorPdos.Visible = true;
            }
            else
            {
                noiTorPdos.Visible = false;
                Environment.Exit(0);
            }
        }

        void noiTorPdosClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            noiTorPdos.Visible = false;
        }

        private void Sorry()
        {
            Controls.Clear();
            MessageBox.Show("Sorry that port is unavailable, the program will now close.");
        }
        public void FirstStartUp()
        {
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

            //BTN: Confirm path
            btnConfirmPath.TabIndex = tabNumber++;
            Controls.Add(btnConfirmPath);

            //Labels
            Controls.Add(lblBrowse);

            AcceptButton = btnConfirmPath;

            if (DiskHelper.GetRegistryValue("Path") != null)
            {
                txtPath.Text = DiskHelper.GetRegistryValue("Path");
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

            //BTN: Change path
            btnChangePath.TabIndex = tabNumber++;
            Controls.Add(btnChangePath);

            //Labels
            Controls.Add(lblPassword);
            Controls.Add(lblConfirmPassword);


            AcceptButton = btnCreate;
        }
        public void Login()
        {
            Controls.Clear();
            int tabNumber = 0;

            //TXT: Password
            txtPassword.TabIndex = tabNumber++;
            txtPassword.Text = null;
            Controls.Add(txtPassword);

            //BTN: Login
            btnLogin.TabIndex = tabNumber++;
            Controls.Add(btnLogin);

            //BTN: Back
            btnChangePath.TabIndex = tabNumber++;
            Controls.Add(btnChangePath);

            //Labels
            Controls.Add(lblPassword);

            AcceptButton = btnLogin;
        }
        public void LoggedIn()
        {
            Controls.Clear();
            int tabNumber = 0;

            //BTN: Add Peer
            btnAddPeer.TabIndex = tabNumber++;
            Controls.Add(btnAddPeer);

            //BTN: Logout
            btnLogout.TabIndex = tabNumber++;
            Controls.Add(btnLogout);
        }
        public string PathName()
        {
            if (txtPath.Text.EndsWith(@"\"))
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
