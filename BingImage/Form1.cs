using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BingImage
{
    public partial class Form1 : Form
    {

        public JObject imageInfo;
        public Image imageContent;


        public static Form1 form1;

        

        public Form1()
        {
            InitializeComponent();

            if (!Properties.Settings.Default.isUpgraded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.isUpgraded = true;
                Properties.Settings.Default.Save();
            }
            
            form1 = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            this.Left = SystemInformation.WorkingArea.Width - 340;
            this.Top = SystemInformation.WorkingArea.Height - 162;

            this.Opacity = 0;

            
            getAndSetBackgroundImage();


            //首次运行
            if (Properties.Settings.Default.firstRun)
            {
                Settings settings = new Settings();
                settings.Show();
                Properties.Settings.Default.firstRun = false;
                Properties.Settings.Default.lastVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Properties.Settings.Default.Save();

                this.TopMost = false;
            }


            bool isRelease = true;
#if DEBUG
            isRelease = false;
#endif

            //更新后
            if (isRelease && Properties.Settings.Default.lastVersion!= System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                Properties.Settings.Default.lastVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Properties.Settings.Default.Save();
                notifyIcon1.ShowBalloonTip(1000, "提示", "必应每日壁纸已更新至" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), ToolTipIcon.Info);
                
            }

            //定时更新
            if (Properties.Settings.Default.autoUpdateImage)
            {
                timerCheckNewImage.Enabled = true;
            }


        }

        
        //获取并设置壁纸
        public bool getAndSetBackgroundImage()
        {
            imageInfo = getImageInfo();
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.imageVersion != imageInfo["images"][0]["hsh"].ToString())
            {
                Properties.Settings.Default.imageVersion = imageInfo["images"][0]["hsh"].ToString();
                Properties.Settings.Default.Save();
                imageContent = getImageFromURL("https://cn.bing.com" + imageInfo["images"][0]["url"].ToString());
                addWatermarkText(imageContent, imageInfo["images"][0]["copyright"].ToString(), imageContent.Width, imageContent.Height);
                label1.Text = imageInfo["images"][0]["copyright"].ToString();
                this.Opacity = 100;
                this.Visible = true;
                setBackgroundImage(imageContent);

                timer1.Enabled = true;
                return true;
            }
            else
            {
                //无新壁纸
                //Application.Exit();
                timer1.Enabled = true;
                return false;
            }
        }

        public String sendGet(String url)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        public JObject getImageInfo()
        {
            string getUrl = "http://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1";
            string jsonResponse = sendGet(getUrl);
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonResponse);
            return jo;
        }

        public Image getImageFromURL(string url)
        {
            return Image.FromStream(WebRequest.Create(url).GetResponse().GetResponseStream());
        }

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public void setBackgroundImage(Image image)
        {
            string savePath = Application.StartupPath + "/backgroundImage.bmp";
            image.Save(savePath, System.Drawing.Imaging.ImageFormat.Bmp);
            SystemParametersInfo(20, 1, savePath, 1);
        }


        public void downloadFiles(string url,string path)
        {
            //该方法非异步
            WebClient webclient = new WebClient();
            webclient.DownloadFile(url, path);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        public void addWatermarkText(Image _image,string _watermarktext,int _width,int _height, string _watermarkposition="TOP_RIGHT")
        {
            Graphics _picture = Graphics.FromImage(_image);

            Font crFont = new Font("arial", 10, FontStyle.Bold);
            SizeF crSize = _picture.MeasureString(_watermarktext, crFont);

            float xpos = 0;
            float ypos = 0;

            switch (_watermarkposition)
            {
                case "TOP_RIGHT":
                    xpos = ((float)_width * (float).99) - (crSize.Width / 2);
                    ypos = (float)_height * (float).01;
                    break;
                case "BOTTOM_RIGHT":
                    xpos = ((float)_width * (float).99) - (crSize.Width / 2);
                    ypos = ((float)_height * (float).99) - crSize.Height;
                    break;
            }

            StringFormat strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Center;

            SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(153, 0, 0, 0));
            _picture.DrawString(_watermarktext, crFont, semiTransBrush2, xpos + 1, ypos + 1, strFormat);

            SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));
            _picture.DrawString(_watermarktext, crFont, semiTransBrush, xpos, ypos, strFormat);

            semiTransBrush2.Dispose();
            semiTransBrush.Dispose();

        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (!this.Visible)
            {
                timer1.Enabled = false;
                return;
            }
            this.Visible = false;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!getAndSetBackgroundImage())
            {
                notifyIcon1.ShowBalloonTip(1000, "提示", "没有新壁纸", ToolTipIcon.Info);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();

            this.TopMost = false;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void timerCheckNewImage_Tick(object sender, EventArgs e)
        {
            getAndSetBackgroundImage();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Visible = false;
                e.Cancel = true;
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }
    }
}
