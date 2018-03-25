using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using log4net;

namespace TFSMonitoring
{
    public partial class mainForm : Form
    {
        Ping pingSender = new Ping();
        PingOptions options = new PingOptions();
        Thread thr;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ContextMenu contextMenu1;
        private MenuItem menuItem1;
        private MenuItem menuItem2;
        private string currentIP = "";
        int interval = 60000;

        public mainForm()
        {
            InitializeComponent();
            contextMenu1 = new ContextMenu();
            menuItem1 = new MenuItem();
            menuItem1.Index = 0;
            menuItem1.Text = "Exit";
            menuItem1.Click += new EventHandler(menuItem1_Click);
            menuItem2 = new MenuItem();
            menuItem2.Index = 0;
            menuItem2.Text = "Stop";
            menuItem2.Click += new EventHandler(menuItem2_Click);
            contextMenu1.MenuItems.AddRange(
            new MenuItem[] { menuItem2, menuItem1 });
            notifyIcon1.ContextMenu = contextMenu1;
            log.Info("Application opened");
            numericUpDown1.Value = interval / 1000;
        }

        private void mainLoop()
        {
            showBalloon("Start", "Server Monitoring started");
            log.Info("System started");
            log.Info("Interval set to " + (interval/1000) + " sec");

            PingReply rply;
            string tmpIP;

            while (true)
            {
                try
                {
                    rply = runPing();
                    if (rply.Status == IPStatus.Success)
                    {
                        tmpIP = rply.Address.ToString();
                        if (tmpIP != currentIP)
                        {
                            currentIP = tmpIP;
                            showBalloon("Change!", "Server IP Changed to " + currentIP);
                            log.Info("Server IP Changed to " + currentIP);
                            notifyIcon1.Text = "Server Monitoring\nIP address: " + currentIP + "";
                        }
                    }
                    else
                    {
                        tmpIP = "no connection";
                        if (tmpIP != currentIP)
                        {
                            currentIP = tmpIP;
                            showBalloon("NO CONNECTION!", "Connection to server lost");
                            log.Warn("Connection to server lost");
                            notifyIcon1.Text = "Server Monitoring\nNo Connection!!";
                        }
                        Thread.Sleep(3000);
                        continue;
                    }
                }
                catch
                {
                    tmpIP = "no connection";
                    if (tmpIP != currentIP)
                    {
                        currentIP = tmpIP;
                        showBalloon("NO CONNECTION!", "Connection to server lost");
                        log.Warn("Connection to server lost");
                        notifyIcon1.Text = "Server Monitoring\nNo Connection!!";
                    }
                    Thread.Sleep(3000);
                    continue;
                }
                Thread.Sleep(interval);
            }
        }
        private PingReply runPing()
        {
            options.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 4000;
            PingReply reply = pingSender.Send(textBox1.Text, timeout, buffer, options);
            return reply;
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            thr.Abort();
            log.Info("System stopped");
            Application.Exit();
        }

        private void menuItem2_Click(object Sender, EventArgs e)
        {
            thr.Abort();
            log.Info("System stopped");
            Show();
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void showBalloon(string title, string body)
        {
            string titTxt = "", bdyTxt = "";
            if (title != null) titTxt = title;
            if (body != null) bdyTxt = body;
            notifyIcon1.ShowBalloonTip(4000, titTxt, bdyTxt, ToolTipIcon.Info);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            currentIP = "";
            thr = new Thread(mainLoop);
            thr.Start();
            Hide();
            notifyIcon1.Visible = true;
        }

        private void mainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            log.Info("Application closed");
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            interval = (int)numericUpDown1.Value * 1000;
        }
    }
}
