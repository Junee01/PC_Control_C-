using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;//stream
using System.Net;//http
using System.Windows.Forms;//form
using System.Diagnostics;//process

namespace Group3_metro_testing
{
    class ConnectServer
    {
        private bool ismonitoroff;
        private string tbx1;
        private string tbx2;

        public bool IsMonOff
        {
            set
            {
                ismonitoroff = value;
            }
            get
            {
                return ismonitoroff;
            }
        }
        
        public string LogText
        {
            get
            {
                return tbx1;
            }
        }

        public ConnectServer()
        {
            ismonitoroff = false;
            tbx1 = "";
            tbx2 = "";
        }

        public ConnectServer(string identification)
        {
            ismonitoroff = false;
            tbx1 = "";
            tbx2 = identification;
        }

        private string connection(string asp, string mes)
        {
            string URL = "http://210.94.194.100:20151/";
            URL = string.Concat(URL, asp);
            string message = mes;
            string resultStr;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(message);
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = sendData.Length;
            request.Method = "POST";

            StreamWriter sw = new StreamWriter(request.GetRequestStream());
            sw.Write(message);
            sw.Close();

            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8")); //UTF-8 버전
            //StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.Default); //시스템기본 버전
            resultStr = streamReader.ReadToEnd();
            streamReader.Close();
            httpWebResponse.Close();

            return resultStr;
        }

        private void ReadLogFromServ() //서버에서 log를 read하는 메소드. 공통으로 사용. ID는 학번.
        {
            //만약 학번이 공백이면 다시 입력
            if (tbx2 == "")
            {
                MessageBox.Show("학번을 입력해주십시오.");
                return;
            }

            //서버에서 받는 로그 포맷 //11437|2011112311|SLEEP|2015-04-17 오후 9:31:43<BR>
            //변경하고자 하는 포맷    //[15-04-17] 21:31:43 :: '2011112311'이 Monitor Off를 수행하였습니다.
            string id = string.Concat("id=", tbx2, "&cmd=read"); //전달받은 identification을 concat하여 id 파라미터로 만듦.
            string msgbxcode = connection("log.asp", id); //사용될 asp파일명과 id 파라미터 전달하고 msgbxcode로 그 결과를 수신.
            if (msgbxcode == "ERROR: Not Existed ID.") //존재하지 않는 ID(학번)이면 return됩니다.
            {
                MessageBox.Show("존재하지 않는 학번입니다.");
                return;
            }

            StreamWriter myStream = new StreamWriter(Application.StartupPath + "\\logging.txt", true, Encoding.Default);
            myStream.Write(msgbxcode);
            myStream.Close();

            
            
            string[] log_br_separater = new string[] { "<BR>" }; //<BR>을 구분자로 만들기 위한 선언입니다.

            //서버에서 받아오는 log에서 <BR>을 구분자로 삼아 한줄을 배열로 저장합니다.
            //구분하여 저장시 배열의 맨 뒤에 공백 원소가 저장되는 것을 막기위해 stringSplitOptions.RemoveEmptyEntries이 지정되었습니다.
            string[] log_line_separated = msgbxcode.Split(log_br_separater, StringSplitOptions.RemoveEmptyEntries);
            string[] log_phrase_separated = new string[4]; //한줄씩 저장된 배열을 다시 '|'을 기준으로 구분하여 저장하기 위한 배열입니다.

            //부팅 시 종료후로부터의 시간 계산의 편의를 위해 2차원 배열을 설정하였습니다.
            //length가 하나 적은 이유는 로그의 시간 부분을 아래의 datetime 배열로 분리시켰습니다.
            string[,] log_arr_of_arr = new string[log_line_separated.Length, 3];
            DateTime[] log_date_separated = new DateTime[log_line_separated.Length];

            for (int i = 0; i < log_line_separated.Length; i++)
            {
                //한줄씩 분리된 로그만큼 수행되는 루프입니다.
                log_phrase_separated = log_line_separated[i].Split('|'); //각 한줄을 '|'을 구분자로 삼아 분리합니다.
                for (int j = 0; j < log_phrase_separated.Length - 1; j++)
                {
                    //2차원 배열로 분리된 배열을 저장합니다.
                    log_arr_of_arr[i, j] = log_phrase_separated[j];
                }

                switch (log_arr_of_arr[i, 2])
                {
                    //저장된 FLAG를 기준으로 표현의 용이함을 위해 보기 쉽게 변경하였습니다.
                    case "SLEEP":
                        log_arr_of_arr[i, 2] = "Monitor Off";
                        break;
                    case "SHUTDOWN":
                        log_arr_of_arr[i, 2] = "ExitWin PowerOff";
                        break;
                    case "WAKEUP":
                        log_arr_of_arr[i, 2] = "Wake Up";
                        break;
                    case "SUSPEND":
                        log_arr_of_arr[i, 2] = "Standby Force";
                        break;
                    case "HIBERNATE":
                        log_arr_of_arr[i, 2] = "SetSuspendState";
                        break;
                    default:
                        break;
                }

                log_date_separated[i] = DateTime.Parse(log_phrase_separated[3]); //분리된 로그에서 시간 부분을 parse합니다.
                log_line_separated[i] = log_date_separated[i].ToString("[yy-MM-dd] HH:mm:ss") + " :: \'" + log_arr_of_arr[i, 1] + "\'이 <" + log_arr_of_arr[i, 2] + ">를 수행하였습니다."; //한줄로 구분된 로그 부분에 서버에서 수신한 로그를 정형화하여 저장합니다.
                System.Array.Clear(log_phrase_separated, 0, log_phrase_separated.Length);
            }

            if (log_arr_of_arr[log_line_separated.Length - 2, 2] == "ExitWin PowerOff" && log_arr_of_arr[log_line_separated.Length - 1, 2] == "Wake Up")
            {
                //만약 최종로그 및 그 이전 로그가 각각 PowerOff명령과 Wake Up명령이었다면 최종 종료시간으로부터 부팅까지 사이의 시간을 표현하는 if문입니다.
                TimeSpan log_time_span = new TimeSpan(log_date_separated[log_line_separated.Length - 1].Ticks - log_date_separated[log_line_separated.Length - 2].Ticks);
                //TimeSpan은 시간의 차이를 tick으로 구해주는 객체입니다.
                //MessageBox.Show(log_time_span.ToString());
                //MessageBox.Show("종료시간으로부터 " + log_time_span.Days.ToString() + "일 " + log_time_span.Hours.ToString() + "시간 " + log_time_span.Minutes.ToString() + "분 " + log_time_span.Seconds.ToString() + "초 경과하였습니다.");
            }
            tbx1 = string.Join("\r\n", log_line_separated); //각 로그를 종합하여 텍스트박스에 보여줍니다.
        }

        public void RegisterID() //학번 등록 메소드
        {
            string id = string.Concat("id=", tbx2);
            string msgbxcode = connection("registerUser.asp", id);
            if (msgbxcode.Equals("ERROR: Exist ID"))
            {
                //MessageBox.Show("이미 등록된 학번입니다.", "사용자 등록 실패!", MessageBoxButtons.OK);
            }
            else if (msgbxcode.Equals("OK"))
            {
                //MessageBox.Show("학번 등록에 성공하셨습니다.", "사용자 등록 성공!", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("알 수 없는 오류가 발생하였습니다.", "오류 발생!", MessageBoxButtons.OK);
            }
            ReadLogFromServ();
        }

        public void WakingUp() //Monitor off에서 wakeup을 하는 경우 이용하는 메소드.
        {
            ismonitoroff = false;
            //------------------------------CommandLine-----------------------------------
            //명령줄 인수를 받아옵니다. 시작프로그램에 등록하여 사용할 경우를 위해 사용됩니다.
            //만약 명령줄 인수가 2개이상 존재하고 첫번째 인수가 /wakeup이라면 수행합니다.
            //인수의 0번째는 실행 파일의 경로입니다.
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 2 && arguments[1] == "/wakeup") tbx2 = arguments[2]; //인수로서 입력된 학번을 학번란에 표시합니다.
            //----------------------------------------------------------------------------
            string id = string.Concat("id=", tbx2, "&cmd=write&action=wakeup");
            string msgbxcode = connection("log.asp", id);
            if (msgbxcode == "ERROR: Not Existed ID.")
            {
                //MessageBox.Show("존재하지 않는 학번입니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            else if (msgbxcode == "OK")
            {
                Process.Start(Application.StartupPath + "\\nircmd\\nircmd.exe", "monitor on");
                //MessageBox.Show("모니터 끄기에서 복귀하였습니다.", "로그 전송 성공!", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("알 수 없는 오류가 발생하였습니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            ReadLogFromServ();
        }

        public void Sleeping() //Monitor Off 명령 메소드
        {
            if (ismonitoroff == true) return;
            string id = string.Concat("id=", tbx2, "&cmd=write&action=sleep");
            string msgbxcode = connection("log.asp", id);
            if (msgbxcode == "ERROR: Not Existed ID.")
            {
                //MessageBox.Show("존재하지 않는 학번입니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            else if (msgbxcode == "OK")
            {
                //MessageBox.Show("모니터가 꺼집니다.", "로그 전송 성공!", MessageBoxButtons.OK);
                ismonitoroff = true; //Monitor Off가 되는 경우 상태를 나타내는 flag
                Process.Start(Application.StartupPath + "\\nircmd\\nircmd.exe", "monitor off");
            }
            else
            {
                MessageBox.Show("알 수 없는 오류가 발생하였습니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            ReadLogFromServ();
        }

        public void ShuttingDown() //ExitWin PowerOff 명령 메소드
        {
            string id = string.Concat("id=", tbx2, "&cmd=write&action=shutdown");
            string msgbxcode = connection("log.asp", id);
            if (msgbxcode == "ERROR: Not Existed ID.")
            {
                //MessageBox.Show("존재하지 않는 학번입니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            else if (msgbxcode == "OK")
            {
                //MessageBox.Show("컴퓨터가 꺼집니다.", "로그 전송 성공!", MessageBoxButtons.OK);
                ismonitoroff = true;
                Process.Start(Application.StartupPath + "\\nircmd\\nircmd.exe", "exitwin poweroff");
            }
            else
            {
                MessageBox.Show("알 수 없는 오류가 발생하였습니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            ReadLogFromServ();
        }

        public void Suspending() //Standby [Force] 명령 메소드
        {
            string id = string.Concat("id=", tbx2, "&cmd=write&action=suspend");
            string msgbxcode = connection("log.asp", id);
            if (msgbxcode == "ERROR: Not Existed ID.")
            {
                //MessageBox.Show("존재하지 않는 학번입니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            else if (msgbxcode == "OK")
            {
                //MessageBox.Show("대기 모드가 됩니다.", "로그 전송 성공!", MessageBoxButtons.OK);
                ismonitoroff = true;
                Process.Start(Application.StartupPath + "\\nircmd\\nircmd.exe", "standby force");
            }
            else
            {
                MessageBox.Show("알 수 없는 오류가 발생하였습니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            ReadLogFromServ();
        }

        public void Hibernating() //SetSuspendState 명령 메소드
        {
            string id = string.Concat("id=", tbx2, "&cmd=write&action=hibernate");
            string msgbxcode = connection("log.asp", id);
            if (msgbxcode == "ERROR: Not Existed ID.")
            {
                //MessageBox.Show("존재하지 않는 학번입니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            else if (msgbxcode == "OK")
            {
                //MessageBox.Show("최대 절전 모드가 됩니다.", "로그 전송 성공!", MessageBoxButtons.OK);
                ismonitoroff = true;
                Process.Start(fileName: "rundll32", arguments: "powrprof.dll, SetSuspendState");
            }
            else
            {
                MessageBox.Show("알 수 없는 오류가 발생하였습니다.", "로그 전송 실패!", MessageBoxButtons.OK);
            }
            ReadLogFromServ();
        }
    }
}
