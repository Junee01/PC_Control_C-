using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using MouseKeyboardLibrary;
using System.IO;
using System.Runtime.InteropServices; //ini추가를 위한 라이브러리

namespace Group3_metro_testing
{
    public partial class Form1 : MetroForm
    {
        [DllImport("kernel32")] //ini를 사용하기 위해 API를 받을 DLL입니다
        public static extern bool WritePrivateProfileString(string section, string key, string val, string filePath); //ini 기록하는 API입니다
        [DllImport("kernel32")] //ini를 사용하기 위해 API를 받을 DLL입니다
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath); //ini 가져오는 API입니다
        KeyboardHook KeyboardHookForShortcut = new KeyboardHook(); //단축키 사용을 인식하기 위한 키보드 훅을 생성합니다.


        ConnectServer ConServ = new ConnectServer(); //서버 통신 클래스
        private int SystemUpTime; //시스템 사용 시간 (밀리초?)
        private int HookIdleAfterTimeTick = 0; //움직임 감지를 위해 Hooking하는 동안의 Tick 카운터
        private int HookIdleAtTimeTick = 0; //움직임 감지를 위해 Hooking하는 동안의 Tick 카운터
        private bool IsHookAfter = false; //훅을 사용하는지 여부 플래그
        private bool IsHookAt = false; //훅을 사용하는지 여부 플래그
        private int AfterTypeOf; //종료 조건 플래그
        private int AtTypeOf; //종료 조건 플래그
        private int AfterTime; //종료될 시간
        private DateTime AtTime = DateTime.Now; //종료될 시간
        TCPServer TcpServ = new TCPServer();


        //timer for auto displayoff
        private Timer autoDisplayoffTimer;
        private int autoDisplayoffCount = 0;
        private int autoDisplayoffTime = 0;
        //timer for auto dispalyoff
        private Timer autoSuspendTimer;
        private int autoSuspendCount = 0;
        private int autoSuspendTime = 0;
        //cpu Usage & mousehook_for_easy
        PerformanceCounter cpuCounter;
        MouseHook mouseHook_for_easy = new MouseHook();             //easystyle에 해당하는 마우스/키보드 훅 추가
        KeyboardHook keyboardHook_for_easy = new KeyboardHook();

        MouseHook mouseHook = new MouseHook();
        KeyboardHook keyboardHook = new KeyboardHook();

        public Form1()
        {
            InitializeComponent();

            /*easy style mode에 대한 값들 초기화 작업 및 이벤트 헨들러 추가*/
            autoDisplayoffTimer = new Timer();
            autoDisplayoffTimer.Interval = 1000;
            autoDisplayoffTimer.Tick += new EventHandler(tick_for_displayoff);
            autoSuspendTimer = new Timer();
            autoSuspendTimer.Interval = 1000;
            autoSuspendTimer.Tick += new EventHandler(tick_for_suspend);
            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
        }

        #region INI
        private string GetIniValue(string section, string key, string filePath)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 255, filePath);
            return temp.ToString();
        }

        private void IniValueLoad() //ini에서 값을 불러오는 메소드입니다.
        {
            string iniPath = System.Reflection.Assembly.GetExecutingAssembly().Location.ToLower().Replace(".exe", ".ini"); //ini파일이 저장된 경로입니다.
            
            MonitorOffModLink.Text = GetIniValue(this.Name, MonitorOffModLink.Name, iniPath);
            MonitorOffKeysLink.Text = GetIniValue(this.Name, MonitorOffKeysLink.Name, iniPath);
            SuspendModLink.Text = GetIniValue(this.Name, SuspendModLink.Name, iniPath);
            SuspendKeysLink.Text = GetIniValue(this.Name, SuspendKeysLink.Name, iniPath);
            HibernateModLink.Text = GetIniValue(this.Name, HibernateModLink.Name, iniPath);
            HibernateKeysLink.Text = GetIniValue(this.Name, HibernateKeysLink.Name, iniPath);
            ShutdownModLink.Text = GetIniValue(this.Name, ShutdownModLink.Name, iniPath);
            ShutdownKeysLink.Text = GetIniValue(this.Name, ShutdownKeysLink.Name, iniPath);

            if (MonitorOffModLink.Text != "Shift" || MonitorOffModLink.Text != "Control" || MonitorOffModLink.Text != "Alt")
            {
                MonitorOffModLink.Text = "Shift";
                MonitorOffKeysLink.Text = "m";
                SuspendModLink.Text = "Control";
                SuspendKeysLink.Text = "p";
                HibernateModLink.Text = "Alt";
                HibernateKeysLink.Text = "h";
                ShutdownModLink.Text = "Shift";
                ShutdownKeysLink.Text = "s";
            }
        }

        private void IniValueSave()
        {
            string iniPath = System.Reflection.Assembly.GetExecutingAssembly().Location.ToLower().Replace(".exe", ".ini"); //ini파일이 저장될 경로입니다.

            WritePrivateProfileString(this.Name, MonitorOffModLink.Name, MonitorOffModLink.Text, iniPath);
            WritePrivateProfileString(this.Name, MonitorOffKeysLink.Name, MonitorOffKeysLink.Text, iniPath);
            WritePrivateProfileString(this.Name, SuspendModLink.Name, SuspendModLink.Text, iniPath);
            WritePrivateProfileString(this.Name, SuspendKeysLink.Name, SuspendKeysLink.Text, iniPath);
            WritePrivateProfileString(this.Name, HibernateModLink.Name, HibernateModLink.Text, iniPath);
            WritePrivateProfileString(this.Name, HibernateKeysLink.Name, HibernateKeysLink.Text, iniPath);
            WritePrivateProfileString(this.Name, ShutdownModLink.Name, ShutdownModLink.Text, iniPath);
            WritePrivateProfileString(this.Name, ShutdownKeysLink.Name, ShutdownKeysLink.Text, iniPath);
        }
        #endregion


        //cpu 사용 정도를 % 값으로 받아오는 메소드
        public float getCurrentCpuUsage()
        {
            return cpuCounter.NextValue();
        }
        //timer tick displayoff
        void tick_for_displayoff(object sender, EventArgs e)
        {
            metroProgressBar1.Enabled = true;
            metroProgressBar1.PerformStep();
            //cpu 사용량이 10%이상이라면 컴퓨터를 사용중이라고 판단하여 count 및 bar 초기화
            if (getCurrentCpuUsage() > 10)
            {
                autoDisplayoffCount = 0;
                metroProgressBar1.Value = 0;
            }

            mouseHook_for_easy.Start();
            keyboardHook_for_easy.Start();
            //카운트는 tick으로 증가하면서 미리 정해놓은 time에 다다르면 중지하고 시작
            if (++autoDisplayoffCount == autoDisplayoffTime)
            {
                autoDisplayoffTimer.Stop();
                SleepProcedure();
            }

        }
        //timer tick suspend
        void tick_for_suspend(object sender, EventArgs e)
        {
            metroProgressBar2.Enabled = true;
            metroProgressBar2.PerformStep();

            if (getCurrentCpuUsage() > 10)
            {
                autoSuspendCount = 0;
                metroProgressBar2.Value = 0;
            }

            mouseHook_for_easy.Start();
            keyboardHook_for_easy.Start();
            if (++autoSuspendCount == autoSuspendTime)
            {
                autoSuspendTimer.Stop();
                SuspendProcedure();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AlarmTimer.Start();
            
            //WholeControlsDisable(); //학번을 입력해야만 사용할 수 있도록 비활성화
            ScheduleTabInit();
            HookingInit();

            AlarmMethod();

            Form2 IdentificationForm = new Form2();
            IdentificationForm.IDRegisterEvent += new Form2.IdentificationFormSendRegisterHandler(Identify);
            IdentificationForm.ShowDialog();

            IniValueLoad();
            KeyboardHookForShortcut.KeyDown += new KeyEventHandler(KeyboardHookForShortcut_KeyDown);
            KeyboardHookForShortcut.Start();

            AfterSpinnerDeact();
            AtSpinnerDeact();

            ListenerStart();

            /*easy style에 해당하는 hooking 이벤트 추가*/
            HookingInit_for_easy();
        }

        #region Main Intialize
        private void WholeControlsDisable() //주요 컨트롤 비활성화
        {
            metroTabControl1.Enabled = false;
            SettingsTile.Enabled = false;
            MonitorOffTile.Enabled = false;
            SuspendTile.Enabled = false;
            ShutDownTile.Enabled = false;
            CancelTile.Enabled = false;
            metroToggle1.Enabled = false;
            metroToggle2.Enabled = false;
        }
        private void WholeControlsEnable() //주요 컨트롤 활성화
        {
            metroTabControl1.Enabled = true;
            SettingsTile.Enabled = true;
            MonitorOffTile.Enabled = true;
            SuspendTile.Enabled = true;
            ShutDownTile.Enabled = true;
            CancelTile.Enabled = true;
            metroToggle1.Enabled = true;
            metroToggle2.Enabled = true;
        }
        void Identify(string str)
        {
            ConServ = new ConnectServer(str);
            ConServ.RegisterID();
            LogTextBox.Text = ConServ.LogText;
        }
        void SleepProcedure()
        {
            keyboardHook.Start();
            mouseHook.Start();
            ConServ.Sleeping();
            LogTextBox.Text = ConServ.LogText;
        }
        void SuspendProcedure()
        {
            keyboardHook.Start();
            mouseHook.Start();
            ConServ.Suspending();
            LogTextBox.Text = ConServ.LogText;
        }
        void HibernateProcedure()
        {
            keyboardHook.Start();
            mouseHook.Start();
            ConServ.Hibernating();
            LogTextBox.Text = ConServ.LogText;
        }
        void ShutdownProcedure()
        {
            keyboardHook.Start();
            mouseHook.Start();
            ConServ.ShuttingDown();
            LogTextBox.Text = ConServ.LogText;
        }
        #endregion

        //brightness settings for only 64bits
        private void metroToggle1_CheckedChanged(object sender, EventArgs e)
        {
            if(metroToggle1.Checked == true)
            {
                Process.Start("nircmd\\nircmd.exe", "setbrightness 100");   //밝기 최대
            }
            else
            {
                Process.Start("nircmd\\nircmd.exe", "setbrightness 0");     //밝기 최소
            }
        }
        //volumes settings
        private void metroToggle2_CheckedChanged(object sender, EventArgs e)
        {
            if(metroToggle2.Checked == true)
            {
                Process.Start("nircmd\\nircmd.exe", "mutesysvolume 0");     //음소거 실행
            }
            else
            {
                Process.Start("nircmd\\nircmd.exe", "mutesysvolume 1");     //음소거 제거
            }
        }
        //의미없는 부분
        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        //cancel
        private void CancleTile_Click(object sender, EventArgs e)
        {
            mouseHook.Stop();
            keyboardHook.Stop();
            
            IsHookAfter = false;
            AfterSpinnerDeact();
            AfterTimer.Stop();
            AfterTimeCheckBox.Checked = false;
            IsHookAt = false;
            AtSpinnerDeact();
            AtTimer.Stop();
            AtTimeCheckBox.Checked = false;




            /*easy style part stop*/
            autoDisplayoffTimer.Stop();
            autoSuspendTimer.Stop();
            mouseHook_for_easy.Stop();
            keyboardHook_for_easy.Stop();
            metroProgressBar1.Value = 0;
            metroProgressBar2.Value = 0;
        }

        private void MonitorOffTile_Click(object sender, EventArgs e)
        {
            SleepProcedure();
        }

        private void SuspendTile_Click(object sender, EventArgs e)
        {
            SuspendProcedure();
        }

        private void HibernateTile_Click(object sender, EventArgs e)
        {
            HibernateProcedure();
        }

        private void ShutDownTile_Click(object sender, EventArgs e)
        {
            ShutdownProcedure();
        }

        private void SettingsTile_Click(object sender, EventArgs e)
        {
            if (metroTabControl1.SelectedTab == EasyStyleTabPage) //탭페이지 처음 (Easy Style) 일때의 셋팅 조건
            {

            }

            if (metroTabControl1.SelectedTab == SchedulerTabPage) //스케쥴 탭페이지 셋팅
            {
                //After 체크되었을때
                if (AfterTimeCheckBox.Checked == true)
                {
                    if (AfterExcuteTypeStandardRadioButton.Checked == true) //Standard 라디오 선택시 후킹 시작
                    {
                        IsHookAfter = true;
                        mouseHook.Start();
                        keyboardHook.Start();
                    }
                    else if (AfterExcuteTypeForceRadioButton.Checked == true) //Force 라디오 선택시 후킹 시작 없음
                    {
                        IsHookAfter = false;
                    }
                    //선택한 절전 조건을 TypeOf 변수에 저장
                    if (AfterSaveTypeMonitorOffRadioButton.Checked)
                    {
                        SpinnerAfterSaveTypeLabel.Text = AfterSaveTypeMonitorOffRadioButton.Text;
                        AfterTypeOf = 1;
                    }
                    else if (AfterSaveTypeSuspendRadioButton.Checked)
                    {
                        SpinnerAfterSaveTypeLabel.Text = AfterSaveTypeSuspendRadioButton.Text;
                        AfterTypeOf = 2;
                    }
                    else if (AfterSaveTypeHibernateRadioButton.Checked)
                    {
                        SpinnerAfterSaveTypeLabel.Text = AfterSaveTypeHibernateRadioButton.Text;
                        AfterTypeOf = 3;
                    }
                    else if (AfterSaveTypeShutdownRadioButton.Checked)
                    {
                        SpinnerAfterSaveTypeLabel.Text = AfterSaveTypeShutdownRadioButton.Text;
                        AfterTypeOf = 4;
                    }
                    AfterTime = Convert.ToInt32(AfterTimeComboBox.SelectedItem); //얼마뒤에 종료할 것인지 시간을 저장
                    AfterSpinnerAct();
                    AfterTimer.Start();
                }
                else if (AfterTimeCheckBox.Checked == false) //After에 체크가 풀린 상태로 셋팅하면
                {
                    if (AtTimeCheckBox.Checked == false) //At도 체크가 풀려있으면 후킹 해제
                    {
                        mouseHook.Stop();
                        keyboardHook.Stop();
                    }
                    IsHookAfter = false;
                    AfterSpinnerDeact();
                    AfterTimer.Stop();
                }

                //At시작 After와 반대입니다.
                if (AtTimeCheckBox.Checked == true)
                {
                    if (AtExcuteTypeStandardRadioButton.Checked == true) //if keyboard or mouse, nothing 
                    {
                        IsHookAt = true;
                        mouseHook.Start();
                        keyboardHook.Start();
                    }
                    else if (AtExcuteTypeForceRadioButton.Checked == true) //force shut down
                    {
                        IsHookAt = false;
                    }

                    if (AtSaveTypeMonitorOffRadioButton.Checked)
                    {
                        SpinnerAtSaveTypeLabel.Text = AtSaveTypeMonitorOffRadioButton.Text;
                        AtTypeOf = 1;
                    }
                    else if (AtSaveTypeSuspendRadioButton.Checked)
                    {
                        SpinnerAtSaveTypeLabel.Text = AtSaveTypeSuspendRadioButton.Text;
                        AtTypeOf = 2;
                    }
                    else if (AtSaveTypeHibernateRadioButton.Checked)
                    {
                        SpinnerAtSaveTypeLabel.Text = AtSaveTypeHibernateRadioButton.Text;
                        AtTypeOf = 3;
                    }
                    else if (AtSaveTypeShutdownRadioButton.Checked)
                    {
                        SpinnerAtSaveTypeLabel.Text = AtSaveTypeShutdownRadioButton.Text;
                        AtTypeOf = 4;
                    }
                    AtTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Convert.ToInt32(AtTimeHourComboBox.SelectedItem.ToString()), Convert.ToInt32(AtTimeMinuteComboBox.SelectedItem.ToString()), 0); //예약된 시간은 DateTime 객체를 이용하여 시간과 분을 설정
                    if ((AtTime - DateTime.Now) < TimeSpan.Zero)
                    {
                        AtTime.AddDays(1);
                    }

                    AtSpinnerAct();
                    AtTimer.Start();
                    
                }
                else if (AtTimeCheckBox.Checked == false)
                {
                    if (AfterTimeCheckBox.Checked == false)
                    {
                        mouseHook.Stop();
                        keyboardHook.Stop();
                    }
                    IsHookAt = false;
                    AtSpinnerDeact();
                    AtTimer.Stop();
                }


            }
        }

        #region hooking_Event
        void CommonHookMethod()
        {
            if (ConServ.IsMonOff == true)
            {
                keyboardHook.Stop();
                mouseHook.Stop();
                ConServ.WakingUp();
                LogTextBox.Text = ConServ.LogText;
            }
            if (IsHookAfter == true)
            {
                AfterTimer.Stop();
                HookIdleAfterTimeTick = 0;
                AfterTimer.Start();
            }
            if (IsHookAt == true)
            {
                AtTimer.Stop();
                HookIdleAtTimeTick = 0;
                AtTimer.Start();
            }
        }

        void keyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            //CommonHookMethod();
        }
        void keyboardHook_KeyPress(object sender, KeyPressEventArgs e)
        {
            CommonHookMethod();
        }
        void keyboardHook_KeyUp(object sender, KeyEventArgs e)
        {
            //CommonHookMethod();
        }
        void mouseHook_MouseWheel(object sender, MouseEventArgs e)
        {
            CommonHookMethod();
        }
        void mouseHook_MouseUp(object sender, MouseEventArgs e)
        {
            //CommonHookMethod();
        }
        void mouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            CommonHookMethod();
        }
        void mouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            CommonHookMethod();
        }
        void HookingInit()
        {
            mouseHook.MouseMove += new MouseEventHandler(mouseHook_MouseMove);
            mouseHook.MouseDown += new MouseEventHandler(mouseHook_MouseDown);
            mouseHook.MouseUp += new MouseEventHandler(mouseHook_MouseUp);
            mouseHook.MouseWheel += new MouseEventHandler(mouseHook_MouseWheel);
            keyboardHook.KeyDown += new KeyEventHandler(keyboardHook_KeyDown);
            keyboardHook.KeyUp += new KeyEventHandler(keyboardHook_KeyUp);
            keyboardHook.KeyPress += new KeyPressEventHandler(keyboardHook_KeyPress);
        }
        #endregion

        #region Tray
        //form open & closing & tray icon
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = true;
            //this.Hide();
            
            //CloseReason으로 Apllication.Exit()를 호출했을 경우에만 종료되도록 설정했습니다.
            switch (e.CloseReason)
            {
                case CloseReason.ApplicationExitCall: //CloseReason에는 여러가지가 있습니다. 다만 여기서는 Exit()콜로만 종료됩니다.
                    e.Cancel = false;
                    break;
                default:
                    e.Cancel = true;
                    notifyIcon1.Visible = true;
                    this.Hide();
                    break;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mouseHook.Stop();
            keyboardHook.Stop();
            this.Close();
            //Environment.Exit(0);
            Application.Exit(); //종료를 Environment.Exit(0) 대신 Apllication.Exit()로 대체했습니다. Env~의 경우 강제적으로 프로세스를 종료하기 때문에
            //FormClosed 이벤트가 정상적으로 실행되지 않아 ini를 기록할 수 없게됩니다.
        }
        #endregion

        private void AlarmTimer_Tick(object sender, EventArgs e)
        {
            //최초 실행시 한번, 5분마다 갱신하는 타이머입니다.
            AlarmMethod();
        }
        void AlarmMethod()
        {
            //if ((AlarmHowMuchEnergyConsumeLabel.Text == "-") || (DateTime.Now.Minute % 5 == 0))
            //{
            SystemUpTime = Environment.TickCount & Int32.MaxValue; //시스템 부팅된 총 시간

            //전기량
            AlarmHowMuchEnergyConsumeLabel.Text = ((int)(SystemUpTime * (float)160 / 60 / 60 / 1000)).ToString() + " Wh";

            //탄소
            AlarmHowMuchCO2ConsumeLabel.Text = ((int)(SystemUpTime * 67.84 / 60 / 60 / 1000)).ToString() + " g";

            //돈? 대신 나무
            AlarmHowMuchMoneyConsumeLabel.Text = ((int)(SystemUpTime * (float)2770 / 365 / 24 / 60 / 60 / 1000)).ToString() + " tree(s)";

            UpdateTimeLabel2.Text = DateTime.Now.ToString("HH:mm"); //레이블의 텍스트를 현재 시간으로 출력.
            //}
        }
        private void EasyTrackBar_Scroll(object sender, ScrollEventArgs e)
        {
            int easystyleDisplayOffTime = 0;
            int easystyleSuspendTime = 0;
            metroPanel4.Enabled = true;

            switch (EasyTrackBar.Value)
            {
                case 0:
                    {
                        EasyLabel7.Text = "-";
                        EasyLabel9.Text = "-";
                        autoDisplayoffTimer.Stop();
                        metroProgressBar1.Value = 0;
                        autoSuspendTimer.Stop();
                        metroProgressBar2.Value = 0;
                        break;
                    }
                case 1:
                    {
                        easystyleDisplayOffTime = 10;
                        easystyleSuspendTime = 15;
                        EasyLabel7.Text = easystyleDisplayOffTime.ToString() + " minutes";
                        EasyLabel9.Text = easystyleSuspendTime.ToString() + " minutes";
                        metroProgressBar1.Step = 1;
                        metroProgressBar1.Maximum = easystyleDisplayOffTime*60;
                        autoDisplayoffTimer.Start();
                        metroProgressBar2.Step = 1;
                        metroProgressBar2.Maximum = easystyleSuspendTime*60;
                        autoSuspendTimer.Start();
                        break;
                    }
                case 2:
                    {
                        easystyleDisplayOffTime = 30;
                        easystyleSuspendTime = 45;
                        EasyLabel7.Text = easystyleDisplayOffTime.ToString() + " minutes";
                        EasyLabel9.Text = easystyleSuspendTime.ToString() + " minutes";
                        metroProgressBar1.Step = 1;
                        metroProgressBar1.Maximum = easystyleDisplayOffTime * 60;
                        autoDisplayoffTimer.Start();
                        metroProgressBar2.Step = 1;
                        metroProgressBar2.Maximum = easystyleSuspendTime * 60;
                        autoSuspendTimer.Start();
                        break;
                    }
                case 3:
                    {
                        easystyleDisplayOffTime = 60;
                        easystyleSuspendTime = 90;
                        EasyLabel7.Text = easystyleDisplayOffTime.ToString() + " minutes";
                        EasyLabel9.Text = easystyleSuspendTime.ToString() + " minutes";
                        metroProgressBar1.Step = 1;
                        metroProgressBar1.Maximum = easystyleDisplayOffTime * 60;
                        autoDisplayoffTimer.Start();
                        metroProgressBar2.Step = 1;
                        metroProgressBar2.Maximum = easystyleSuspendTime * 60;
                        autoSuspendTimer.Start();
                        break;
                    }
                case 4:
                    {
                        easystyleDisplayOffTime = 120;
                        easystyleSuspendTime = 180;
                        EasyLabel7.Text = easystyleDisplayOffTime.ToString() + " minutes";
                        EasyLabel9.Text = easystyleSuspendTime.ToString() + " minutes";
                        metroProgressBar1.Step = 1;
                        metroProgressBar1.Maximum = easystyleDisplayOffTime * 60;
                        autoDisplayoffTimer.Start();
                        metroProgressBar2.Step = 1;
                        metroProgressBar2.Maximum = easystyleSuspendTime * 60;
                        autoSuspendTimer.Start();
                        break;
                    }
            }
        }

        #region hook_for_easyStyle
        //hooking 이벤트 추가 메소드
        void HookingInit_for_easy()
        {
            //new
            mouseHook_for_easy.MouseMove += new MouseEventHandler(mouseHook_MouseMove_easy);
            mouseHook_for_easy.MouseDown += new MouseEventHandler(mouseHook_MouseDown_easy);
            mouseHook_for_easy.MouseUp += new MouseEventHandler(mouseHook_MouseUp_easy);
            mouseHook_for_easy.MouseWheel += new MouseEventHandler(mouseHook_MouseWheel_easy);
            keyboardHook_for_easy.KeyDown += new KeyEventHandler(keyboardHook_KeyDown_easy);
            keyboardHook_for_easy.KeyUp += new KeyEventHandler(keyboardHook_KeyUp);
            keyboardHook_for_easy.KeyPress += new KeyPressEventHandler(keyboardHook_KeyPress_easy);
        }
        //easy style mode 용 keyboard & mousehook 정의
        void keyboardHook_KeyDown_easy(object sender, KeyEventArgs e)
        {
            keyboardHook_for_easy.Stop();
            mouseHook_for_easy.Stop();
            autoDisplayoffCount = 0;
            autoSuspendCount = 0;
            metroProgressBar1.Value = 0;
            metroProgressBar2.Value = 0;
        }
        void keyboardHook_KeyPress_easy(object sender, KeyPressEventArgs e)
        {
            keyboardHook_for_easy.Stop();
            mouseHook_for_easy.Stop();
            autoDisplayoffCount = 0;
            autoSuspendCount = 0;
            metroProgressBar1.Value = 0;
            metroProgressBar2.Value = 0;
        }
        void keyboardHook_KeyUp_easy(object sender, KeyEventArgs e)
        {
            keyboardHook_for_easy.Stop();
            mouseHook_for_easy.Stop();
            autoDisplayoffCount = 0;
            autoSuspendCount = 0;
            metroProgressBar1.Value = 0;
            metroProgressBar2.Value = 0;
        }
        void mouseHook_MouseWheel_easy(object sender, MouseEventArgs e)
        {
            keyboardHook_for_easy.Stop();
            mouseHook_for_easy.Stop();
            autoDisplayoffCount = 0;
            autoSuspendCount = 0;
            metroProgressBar1.Value = 0;
            metroProgressBar2.Value = 0;
        }
        void mouseHook_MouseUp_easy(object sender, MouseEventArgs e)
        {
            keyboardHook_for_easy.Stop();
            mouseHook_for_easy.Stop();
            autoDisplayoffCount = 0;
            autoSuspendCount = 0;
            metroProgressBar1.Value = 0;
            metroProgressBar2.Value = 0;
        }
        void mouseHook_MouseDown_easy(object sender, MouseEventArgs e)
        {
            keyboardHook_for_easy.Stop();
            mouseHook_for_easy.Stop();
            autoDisplayoffCount = 0;
            autoSuspendCount = 0;
            metroProgressBar1.Value = 0;
            metroProgressBar2.Value = 0;
        }
        void mouseHook_MouseMove_easy(object sender, MouseEventArgs e)
        {
            keyboardHook_for_easy.Stop();
            mouseHook_for_easy.Stop();
            autoDisplayoffCount = 0;
            autoSuspendCount = 0;
            metroProgressBar1.Value = 0;
            metroProgressBar2.Value = 0;
        }
        #endregion 

        #region ScheduleTab
        private void ScheduleTabInit() //컨트롤 비활성화 및 라디오버튼들은 초기 선택
        {
            metroTabControl1.SelectedIndex = 2;
            AfterTimePanel.Enabled = false;
            AfterSaveTypePanel.Enabled = false;
            AfterExcuteTypePanel.Enabled = false;
            AfterTimeComboBox.SelectedIndex = 0;
            AfterSaveTypeMonitorOffRadioButton.Checked = true;
            AfterExcuteTypeStandardRadioButton.Checked = true;
            AtTimePanel.Enabled = false;
            AtSaveTypePanel.Enabled = false;
            AtExcuteTypePanel.Enabled = false;
            AtTimeHourComboBox.SelectedIndex = 0;
            AtTimeMinuteComboBox.SelectedIndex = 0;
            AtSaveTypeMonitorOffRadioButton.Checked = true;
            AtExcuteTypeStandardRadioButton.Checked = true;
        }

        private void AfterTimeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AfterTimeCheckBox.Checked == true)
            {
                AfterTimePanel.Enabled = true;
                AfterSaveTypePanel.Enabled = true;
                AfterExcuteTypePanel.Enabled = true;
            }
            else if(AfterTimeCheckBox.Checked == false)
            {
                AfterTimePanel.Enabled = false;
                AfterSaveTypePanel.Enabled = false;
                AfterExcuteTypePanel.Enabled = false;
            }
        }

        private void AtTimeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AtTimeCheckBox.Checked == true)
            {
                AtTimePanel.Enabled = true;
                AtSaveTypePanel.Enabled = true;
                AtExcuteTypePanel.Enabled = true;
            }
            else if (AtTimeCheckBox.Checked == false)
            {
                AtTimePanel.Enabled = false;
                AtSaveTypePanel.Enabled = false;
                AtExcuteTypePanel.Enabled = false;
            }
        }

        
        private void AtTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan AtTimeSpan;
            HookIdleAtTimeTick += AtTimer.Interval;
            AtTimeSpan = AtTime - DateTime.Now;
            
            SpinnerAtTimeLabel.Text = AtTimeSpan.ToString(@"hh\:mm\:ss");

            
            if ((AtTime.Hour == DateTime.Now.Hour) && (AtTime.Minute == DateTime.Now.Minute))  //현재 시간이 설정한 시간과 분에 해당된다면 수행
            {
                if (HookIdleAtTimeTick < 10000) //활동중의 기준 10초
                {
                    AtTimer.Stop(); //타이머의 틱이 1초 기준이므로 중복해서 수행될 수 있으므로 타이머 정지
                    MessageBox.Show("사용중이므로 예약이 취소됩니다.");
                }

                AtTimer.Stop(); //타이머의 틱이 1초 기준이므로 중복해서 수행될 수 있으므로 타이머 정지
                MessageBox.Show("자동 절전"); //테스트를 위한 메시지박스. 삭제가능
                switch (AtTypeOf)
                {
                    case 1:
                        SleepProcedure();
                        break;
                    case 2:
                        SuspendProcedure();
                        break;
                    case 3:
                        HibernateProcedure();
                        break;
                    case 4:
                        ShutdownProcedure();
                        break;
                    default:
                        MessageBox.Show("Error!");
                        break;
                }
            }
        }

        private void AfterTimer_Tick(object sender, EventArgs e)
        {
            HookIdleAfterTimeTick += AfterTimer.Interval;
            AfterProgressSpinner.Value = (int)((float)HookIdleAfterTimeTick / AfterTime / 60 / 1000 * 100);
            SpinnerAfterTimeLabel.Text = (((AfterTime * 60) - (HookIdleAfterTimeTick / 1000)) / 60).ToString("00") + ":" + (((AfterTime * 60) - (HookIdleAfterTimeTick / 1000)) % 60).ToString("00");

            if (HookIdleAfterTimeTick > AfterTime*60*1000) //종료될 시간 (기준이 분이므로 *초*밀리)
            {
                AfterTimer.Stop(); //타이머의 틱이 1초 기준이므로 중복해서 수행될 수 있으므로 타이머 정지
                MessageBox.Show("자동 절전"); //테스트를 위한 메시지박스. 삭제가능
                switch (AfterTypeOf)
                {
                    case 1:
                        SleepProcedure();
                        break;
                    case 2:
                        SuspendProcedure();
                        break;
                    case 3:
                        HibernateProcedure();
                        break;
                    case 4:
                        ShutdownProcedure();
                        break;
                    default:
                        MessageBox.Show("Error!");
                        break;
                }
            }
        }
        #endregion

        private void shortCutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //선택하면 해당 탭 페이지를 보여줌
            metroTabControl1.SelectTab(2);
            this.Show();
        }

        #region ShorCut_Setting_Part
        private void MonitorOffModLink_Click(object sender, EventArgs e)
        {
            Keyboard2 tempkeyboard2 = new Keyboard2();
            tempkeyboard2.ShowDialog();
            MonitorOffModLink.Text = tempkeyboard2.keycod;
        }

        private void MonitorOffKeysLink_Click(object sender, EventArgs e)
        {
            keyboard tempkeyboard = new keyboard();
            tempkeyboard.ShowDialog();
            MonitorOffKeysLink.Text = tempkeyboard.keycod;
        }

        private void SuspendModLink_Click(object sender, EventArgs e)
        {
            Keyboard2 tempkeyboard2 = new Keyboard2();
            tempkeyboard2.ShowDialog();
            SuspendModLink.Text = tempkeyboard2.keycod;
        }

        private void SuspendKeysLink_Click(object sender, EventArgs e)
        {
            keyboard tempkeyboard = new keyboard();
            tempkeyboard.ShowDialog();
            SuspendKeysLink.Text = tempkeyboard.keycod;
        }

        private void HibernateModLink_Click(object sender, EventArgs e)
        {
            Keyboard2 tempkeyboard2 = new Keyboard2();
            tempkeyboard2.ShowDialog();
            HibernateModLink.Text = tempkeyboard2.keycod;
        }

        private void HibernateKeysLink_Click(object sender, EventArgs e)
        {
            keyboard tempkeyboard = new keyboard();
            tempkeyboard.ShowDialog();
            HibernateKeysLink.Text = tempkeyboard.keycod;
        }

        private void ShutdownModLink_Click(object sender, EventArgs e)
        {
            Keyboard2 tempkeyboard2 = new Keyboard2();
            tempkeyboard2.ShowDialog();
            ShutdownModLink.Text = tempkeyboard2.keycod;
        }

        private void ShutdownKeysLink_Click(object sender, EventArgs e)
        {
            keyboard tempkeyboard = new keyboard();
            tempkeyboard.ShowDialog();
            ShutdownKeysLink.Text = tempkeyboard.keycod;
        }
        #endregion

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //선택하면 해당 탭 페이지를 보여줌
            metroTabControl1.SelectTab(4);
            this.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Chart cart = new Chart();
            cart.ShowDialog();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) //FormClosing은 닫힐 때, FormClosed는 닫힌 후 입니다.
        {
            KeyboardHookForShortcut.Stop();
            IniValueSave(); //폼이 완전히 닫히면 ini에 값을 저장합니다.
        }

        void KeyboardHookForShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            //키가 정해져있지 않으면 버그

            //콤보박스에 지정된 아이템을 Keys로 변경하기 위한 객체입니다. 단축키로 사용되는 키가 겹치는 경우 이벤트가 발생하지 않을 수 있어 객체를 12개로 선언했습니다.
            KeysConverter[] linkkeysconverter = new KeysConverter[8];
            for (int i = 0; i < 8; i++)  
            {
                linkkeysconverter[i] = new KeysConverter();
            }

            //폼에 나타나는 단축키를 눌렀을 경우 해당 작업을 실행합니다.
            //각각의 콤보박스에서 선택된 item 정보를 토대로 단축키 인식을 합니다.
            if (e.Modifiers == (Keys)linkkeysconverter[0].ConvertFromString(MonitorOffModLink.Text) && e.KeyCode == (Keys)linkkeysconverter[1].ConvertFromString(MonitorOffKeysLink.Text.ToUpper()))
            {
                    SleepProcedure();
            }
            if (e.Modifiers == (Keys)linkkeysconverter[2].ConvertFromString(SuspendModLink.Text) && e.KeyCode == (Keys)linkkeysconverter[3].ConvertFromString(SuspendKeysLink.Text.ToUpper()))
            {
                    SuspendProcedure();
            }
            if (e.Modifiers == (Keys)linkkeysconverter[4].ConvertFromString(HibernateModLink.Text) && e.KeyCode == (Keys)linkkeysconverter[5].ConvertFromString(HibernateKeysLink.Text.ToUpper()))
            {
                    HibernateProcedure();
            }
            if (e.Modifiers == (Keys)linkkeysconverter[6].ConvertFromString(ShutdownModLink.Text) && e.KeyCode == (Keys)linkkeysconverter[7].ConvertFromString(ShutdownKeysLink.Text.ToUpper())) 
            {
                    ShutdownProcedure();
            }
        }
        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData) 키가 상수값이어야 함

        void AfterSpinnerAct()
        {
            AfterProgressSpinner.Spinning = true;
            SpinnerAfterPanel.Enabled = true;
        }

        void AfterSpinnerDeact()
        {
            AfterProgressSpinner.Spinning = false;
            SpinnerAfterPanel.Enabled = false;
        }

        void AtSpinnerAct()
        {
            AtProgressSpinner.Spinning = true;
            SpinnerAtPanel.Enabled = true;
        }

        void AtSpinnerDeact()
        {
            AtProgressSpinner.Spinning = false;
            SpinnerAtPanel.Enabled = false;
        }

        void ListenerStart()
        {
            TcpServ.Start();

            TcpServ.FormSleepEvent += new TCPServer.FormSendSleepHandler(SleepProcedure);
            TcpServ.FormSuspendEvent += new TCPServer.FormSendSuspendHandler(SuspendProcedure);
            TcpServ.FormHibernateEvent += new TCPServer.FormSendHibernateHandler(HibernateProcedure);
            TcpServ.FormShutdownEvent += new TCPServer.FormSendOffHandler(ShutdownProcedure);
        }

        private void metroTile1_Click(object sender, EventArgs e)
        {
            Chart cart = new Chart();
            cart.ShowDialog();
        }

        private void easyModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            metroTabControl1.SelectTab(0);
            this.Show();
        }

        private void schdulerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            metroTabControl1.SelectTab(1);
            this.Show();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            metroTabControl1.SelectTab(5);
            this.Show();
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            metroTabControl1.SelectTab(3);
            this.Show();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Chart cart = new Chart();
            cart.ShowDialog();
        }


    }
}