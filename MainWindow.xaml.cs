using System;
using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace SaveReminderWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        private Forms.NotifyIcon notifyIcon;
        private int interval;
        public MainWindow()
        {
            InitializeComponent();
            CreateIniIfNotExist();
            this.interval = ReadInterval();
            ShowTuoPan();
            ScheduledNotification();
        }

        private void CreateIniIfNotExist()
        {
            if(File.Exists(@"./SaveReminder.ini"))
            {
                MessageBox.Show("程序已在后台运行，人生苦短，及时保存◍'ㅅ'◍");
                return;
            }
            else
            {
                MessageBox.Show("程序默认最小化至程序托盘，双击托盘或右击设置时间间隔。");
                CreateIni();
            }
        }

        private void CreateIni()
        {
            string iniPath = @"./SaveReminder.ini";
            string initConfig = "[Time]\ninterval=5";
            System.IO.File.WriteAllText(iniPath, initConfig, Encoding.UTF8);
            FileInfo fileInfo = new FileInfo(iniPath);
            fileInfo.Attributes = FileAttributes.Hidden;
        }

        private int ReadInterval()
        {
            StringBuilder temp = new StringBuilder(1024);
            string iniPath = @"./SaveReminder.ini";
            string section = "Time";
            string key = "interval";
            GetPrivateProfileString(section, key, "5", temp, 1024, iniPath);
            return int.Parse(temp.ToString());
        }

        private void ShowTuoPan()
        {
            this.Visibility = Visibility.Hidden;
            this.notifyIcon = new Forms.NotifyIcon();
            this.notifyIcon.Text = "你保存了吗？";
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("设置提醒间隔");
            open.Click += new EventHandler(SetInterval);
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            exit.Click += new EventHandler(Close);
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == Forms.MouseButtons.Left) this.SetInterval(o, e);
            });
        }

        private void SetInterval(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void Close(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ScheduledNotification()
        {
            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += new System.Timers.ElapsedEventHandler(ShowNotification);
            t.Interval = interval * 60000;
            t.Enabled = true;
        }

        private void ShowNotification(object sender, System.Timers.ElapsedEventArgs e)
        {
            MessageBox.Show(interval + "分钟过去了，你按Ctrl + S了吗？");
        }

        private void IntervalSetButton_Click(object sender, RoutedEventArgs e)
        {
            int interval = -1;
            string iniPath = @"./SaveReminder.ini";
            string section = "Time";
            string key = "interval";
            try
            {
                interval = int.Parse(IntervalText.Text);
            }
            catch(System.FormatException)
            {
                MessageBox.Show("输入必须是数字！");
                return;
            }
            if(interval <= 0)
            {
                MessageBox.Show("请输入大于0的数值！");
                return;
            }
            this.interval = interval;
            WritePrivateProfileString(section, key, interval.ToString(), iniPath);
            MessageBox.Show("设置保存成功！");
            Forms.Application.Restart();
            Application.Current.Shutdown();
        }
    }
}
