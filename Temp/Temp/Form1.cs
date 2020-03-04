using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;

namespace Temp
{
    public partial class Form1 : Form
    {
        SerialPort port = new SerialPort("COM3");
        Timer dataUpdate;
        Timer dataSave;
        bool led;
        string data;
        int databaseUpdateInterval = 5000;
        DataTable dt = new DataTable();
        public Form1()
        {
            InitializeComponent();
        }

        public void SetTimerDataUpdate()
        {
            dataUpdate = new Timer();
            dataUpdate.Tick += new EventHandler(DataUpdateTick);
            dataUpdate.Interval = 1000;
            dataUpdate.Start();
        }
        public void SetTimerDataSave()
        {
            dataSave = new Timer();
            dataSave.Tick += new EventHandler(DataSaveTick);
            dataSave.Interval = databaseUpdateInterval;
            dataSave.Start();
        }

        void DataUpdateTick(object sender, EventArgs e)
        {
            try
            {
                LoadData();
            }
            catch
            {
                //desi se nekad da se ne usklade ovaj tajmer iz c# i delay iz arduina, ovo je samo da se ne crashuje program onda
            }
        }

        void DataSaveTick(object sender, EventArgs e)
        {
            try
            {
                SaveData();
            }
            catch
            {
                //desi se nekad da se ne usklade ovaj tajmer iz c# i delay iz arduina, ovo je samo da se ne crashuje program onda
            }
        }

        void SaveData()
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dateTime = DateTime.Now.ToString("HH:mm:ss");
            string temp = data.Substring(0, data.IndexOf(" "));
            string vlaznost = data.Substring(data.IndexOf(" ") + 1);
            using (StreamWriter sw = File.AppendText("data.txt"))
            {
                sw.WriteLine(dateTime + " " + temp + " " + vlaznost);
            }
            // dodati u datasource
            dt.Rows.Add(dateTime, temp, vlaznost);
        }

        public void LoadData()
        {
            data = port.ReadLine();
            label3.Text = data.Substring(0, data.IndexOf(" ")) + "°C";
            int procenat = Convert.ToInt32(label3.Text.Substring(0, 2));
            progressBar1.Value = procenat;
            label4.Text = data.Substring(data.IndexOf(" ") + 1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            port.BaudRate = 9600;
            port.Open();
            SetTimerDataUpdate();
            SetTimerDataSave();

            dt.Columns.Add("Vreme Merenja");
            dt.Columns.Add("Temperatura");
            dt.Columns.Add("Vlaznost Vazduha");
            dataGridView1.DataSource = dt;
            label8.Text = (databaseUpdateInterval / 1000).ToString() + "s";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (led)
            {
                port.WriteLine("0");
                led = false;
                label9.Text = "LED = off";
            }
            else
            {
                port.WriteLine("1");
                led = true;
                label9.Text = "LED = on";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.TextLength > 0)
            {
                try
                {
                    databaseUpdateInterval = Convert.ToInt32(textBox1.Text) * 1000;
                    SetTimerDataSave();
                }
                catch
                {
                    databaseUpdateInterval = 5000;
                }
            }
            label8.Text = (databaseUpdateInterval / 1000).ToString() + "s";
        }
    }
}
