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
    public partial class Keyboard2 : MetroForm
    {
        public string keycod = "-";  //여길 ""로 하면 선택 안할 경우 label이 사라져서 선택도 못하게 됩니다.

        public Keyboard2()
        {
            InitializeComponent();
        }

        private void Keyboard2_Load(object sender, EventArgs e)
        {
            metroButton54.Enabled = false;
            metroButton55.Enabled = false;
            metroButton56.Enabled = false;
            metroButton57.Enabled = false;
            metroButton58.Enabled = false;
            metroButton59.Enabled = false;
            metroButton60.Enabled = false;
            metroButton8.Enabled = false;
            metroButton9.Enabled = false;
            metroButton10.Enabled = false;
            metroButton11.Enabled = false;
            metroButton12.Enabled = false;
            metroButton13.Enabled = false;
            metroButton14.Enabled = false;
            metroButton15.Enabled = false;
            metroButton16.Enabled = false;
            metroButton17.Enabled = false;
            metroButton18.Enabled = false;
            metroButton19.Enabled = false;
            metroButton20.Enabled = false;
            metroButton21.Enabled = false;
            metroButton22.Enabled = false;
            metroButton23.Enabled = false;
            metroButton24.Enabled = false;
            metroButton25.Enabled = false;
            metroButton26.Enabled = false;
            metroButton27.Enabled = false;
            metroButton28.Enabled = false;
            metroButton29.Enabled = false;
            metroButton30.Enabled = false;
            metroButton31.Enabled = false;
            metroButton32.Enabled = false;
            metroButton33.Enabled = false;
            metroButton34.Enabled = false;
            metroButton35.Enabled = false;
            metroButton37.Enabled = false;
            metroButton38.Enabled = false;
            metroButton39.Enabled = false;
            metroButton40.Enabled = false;
            metroButton41.Enabled = false;
            metroButton42.Enabled = false;
            metroButton43.Enabled = false;
            metroButton44.Enabled = false;
            metroButton45.Enabled = false;
            metroButton46.Enabled = false;
            metroButton48.Enabled = false;
            metroButton49.Enabled = false;
            metroButton50.Enabled = false;
            metroButton51.Enabled = false;
            metroButton53.Enabled = false;
        }

        private void metroButton47_Click(object sender, EventArgs e)
        {
            keycod = metroButton47.Text;
            this.Close();
        }

        private void metroButton36_Click(object sender, EventArgs e)
        {
            keycod = metroButton36.Text;
            this.Close();
        }

        private void metroButton52_Click(object sender, EventArgs e)
        {
            keycod = metroButton52.Text;
            this.Close();
        }
    }
}
