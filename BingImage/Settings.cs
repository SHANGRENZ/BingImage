using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BingImage
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {

            this.Left = SystemInformation.WorkingArea.Width - 320;
            this.Top = SystemInformation.WorkingArea.Height - 200;

            //Form1 form1 = new Form1();
            //form1.TopMost = true;
            Form1.form1.TopMost = true;


            
            checkBox1.Checked = Properties.Settings.Default.autoStart;
            checkBox2.Checked = Properties.Settings.Default.autoUpdateImage;


        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk.SetValue("BingImage", path);
                rk.Close();
                Properties.Settings.Default.autoStart = true;
            }
            else
            {
                RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk.DeleteValue("BingImage", false);
                rk.Close();
                Properties.Settings.Default.autoStart = false;
            }
            Properties.Settings.Default.Save();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Properties.Settings.Default.autoUpdateImage = true;
            }
            else
            {
                Properties.Settings.Default.autoUpdateImage = false;
            }
            Properties.Settings.Default.Save();
        }
    }
}
