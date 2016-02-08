using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Group3_metro_testing
{
    public partial class Form2 : MetroForm
    {
        public delegate void IdentificationFormSendRegisterHandler(string str); //학번 확인 창에서 등록을 누를 경우 발생할 이벤트의 델리게이트 (파라미터는 학번 스트링입니다)
        public event IdentificationFormSendRegisterHandler IDRegisterEvent; //[등록] 이벤트

        private string ID = ""; //학번 스트링
        public Form2()
        {
            InitializeComponent();
        }

        private void IdentificationRegisterButton_Click(object sender, EventArgs e) //등록 버튼
        {
            ID = IdentificationTextBox.Text; //클릭하면 텍스트 박스의 텍스트를 학번 스트링에 저장합니다.
            this.IDRegisterEvent(ID); //해당 스트링을 인자로서 이벤트를 발생시킵니다.
            this.Close(); //이 폼을 닫아 메인 폼을 보여줍니다.
        }

        private void IdentificationCancleButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0); //취소 버튼을 누르면 프로그램을 종료시킵니다.
        }
    }
}