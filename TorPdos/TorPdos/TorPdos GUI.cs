using System.Drawing;
using System.Windows.Forms;

namespace TorPdos{
    public class AppForm : Form{
        public AppForm(){
            initializeComponent();
        }

        private void initializeComponent(){
            SuspendLayout();
            // 
            // AppForm
            // 
            BackColor = SystemColors.ControlDarkDark;
            ClientSize = new Size(893, 463);
            ForeColor = SystemColors.Window;
            Name = "AppForm";
            ResumeLayout(false);
        }
    }
}