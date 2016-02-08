using MetroFramework.Forms;
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

namespace RemoteController
{
    public partial class Form1 : MetroForm
    {
        private String URL = "http://210.94.194.100:20151/command.asp";
        private string ip;
        private string cmd;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InputIpAddress inputForm = new InputIpAddress();
            inputForm.ShowDialog();
            ip = inputForm._ipaddress;                  //InputIpAddress Form에서 ip 주소를 받아온다.
            metroLabel1.Text = "IP Address : " + ip;    //라벨에 우선 뿌려준다.
        }
        //각각의 URL의 뒷부분에 ip 및 명령어를 추가해주고 기능 실행
        private void metroTile1_Click(object sender, EventArgs e)
        {
            cmd = "ip=" + ip + "&cmd=sleep";
            performance(cmd);
        }

        private void metroTile2_Click(object sender, EventArgs e)
        {
            cmd = "ip=" + ip + "&cmd=suspend";
            performance(cmd);
        }

        private void metroTile3_Click(object sender, EventArgs e)
        {
            cmd = "ip=" + ip + "&cmd=hibernate";
            performance(cmd);
        }

        private void metroTile4_Click(object sender, EventArgs e)
        {
            cmd = "ip=" + ip + "&cmd=shutdown";
            performance(cmd);
        }

        void performance(String cmd)
        {
            //URL에 위에서 정해진 cmd로 request보내는 작업
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(cmd);
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = sendData.Length;
            request.Method = "POST";

            StreamWriter sw = new StreamWriter(request.GetRequestStream());
            sw.Write(cmd);
            sw.Close();

            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();
            httpWebResponse.Close();
        }
    }
}
