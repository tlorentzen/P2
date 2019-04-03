using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TorPdos{
    public partial class AppForm : Form{
        public AppForm(){
            InitializeComponent();
        }

        private void InitializeComponent(){
            this.SuspendLayout();
            // 
            // AppForm
            // 
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(893, 463);
            this.ForeColor = System.Drawing.SystemColors.Window;
            this.Name = "AppForm";
            this.ResumeLayout(false);
        }
    }
}