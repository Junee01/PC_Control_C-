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

namespace RemoteController
{
    public partial class InputIpAddress : MetroForm
    {
        public string _ipaddress;

        public InputIpAddress()
        {
            InitializeComponent();
        }
        #region placeholder
        //placeholder를 구현해본 부분
        public void TextGotFocus(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            //텍스트가 다음과 같다면, 새로 쓸 수 있도록 빈칸으로 조정
            if(tb.Text == "For example... 123.123.01.1")
            {
                tb.Text = "";
                tb.ForeColor = Color.LightGray;
            }
        }

        public void TextLostFocus(object sender, EventArgs e)
        {
            //텍스트가 비어있다면 다시 원상복귀
            TextBox tb = (TextBox)sender;
            if(tb.Text == "")
            {
                tb.Text = "For example... 123.123.01.1";
                tb.ForeColor = Color.LightGray;
            }
        }
        #endregion

        private void InputIpAddress_Load(object sender, EventArgs e)
        {
            //클릭 버튼으로 해당 폼에 준비된 동작들이 실행됨
            this.ActiveControl = metroButton1;
            textBox1.GotFocus += new EventHandler(this.TextGotFocus);
            textBox1.LostFocus += new EventHandler(this.TextLostFocus);
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            //ip 주소 저장
            _ipaddress = textBox1.Text;
            this.Close();
        }
    }
}
