using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/********************** 차 트 ****************************************/
namespace Group3_metro_testing
{
    public partial class Chart : MetroForm
    {
        public Chart()
        {
            InitializeComponent();
            makeChart(); //차트 그리는데 필요한 데이터 계산
            showWeb(); //웹으로 차트 보여주기
        }

        public void showWeb()
        {
            //POST방식으로 데이터를 보내기위하여 인코딩
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            String requestData = "pieChartData;;{0}::verticalStackChartData;;{1}::horizontalStackChartData;;{2}";
            String header = "";
            requestData = requestData.Replace("{0}", getPieChartData())
                                     .Replace("{1}", getVerticalStackBarData())
                                     .Replace("{2}", getHorizontalStackBarData());
            this.webBrowser1.Navigate("http://dna.dongguk.ac.kr/~freddi/seojung/test.php", "", encoding.GetBytes(requestData), header);
        }

        String[] log; //읽어온 로그기록을 요소별로 잘라서 보관
        double[][] saveEnergy; //날짜별 절약된 에너지 저장 배열([0] = 오늘, [1] = 어제,,,)
        double[] carbon;//탄소량 저장 배열

        public void makeChart()
        {
            //2주치 (14일) 데이터만 분석하기 때문에 배열크기를 14로 할당
            saveEnergy = new double[14][];
            carbon = new double[14];
            for (int i = 0; i < saveEnergy.Length; i++) //배열 초기화
            {
                saveEnergy[i] = new double[5]; //4가지 기능+total값 저장 위해 배열크기 5만큼 할당
                for (int j = 0; j < 5; j++)
                {
                    saveEnergy[i][j] = 0;
                }
                carbon[i] = 0;
            }
            //getLog(); //로그기록 읽어와 배열에 저장( 코드 제출용)
            readText(); //임시 로그기록 읽어와 배열에 저장 (시연용)
            calculateEnergy();
        }

        public void calculateEnergy()
        {
            //절약된 에너지 계산
            //  오늘 / 오늘부터 7일간 / 이번주&저번주 데이터만 제공한다고 가정 -> 14일치 데이터만 필요
            //saveEnergy[0] = 오늘 절약 에너지, saveEnergy[1] = 어제 절약 에너지.. saveEnergy[6]까지 계산
            //log[]의 맨마지막부터 14일치 데이터만 계산하면 됨.

            int index = log.Length - 1;
            int day = 0;
            TimeSpan save_time;
            DateTime date1, date2;
            while (day < 14)
            {
                /* 
                 * 절약 시간:
                 * suspend/hibernate/shutdown 시작시간 ~ wakeup한 시간 또는
                 * sleep 시작시간 ~ suspend/hibernate/shutdown/wakeup 시간
                 * 
                 * wakeup 시작시간~ 다른 동작 시작시간 사이는 사용한 시간이므로 절약시간이 아니다.
                 */

                //날짜와 시간은 "연-월-일 오전/오후 시:분:초" 형태
                date2 = DateTime.ParseExact(log[index - 1], "yyyy-MM-dd tt h:mm:ss", null);
                if (!log[index - 6].Equals("WAKEUP")) //wakeup시간이 아닌 시간(절약 시간)
                {
                    date1 = DateTime.ParseExact(log[index - 5], "yyyy-MM-dd tt h:mm:ss", null);
                }
                else { index -= 4; continue; } //wakeup~sleep시간을 뺀 것과 같은, 사용한 시간을 체크하는 꼴은 계산 안함

                save_time = date2 - date1; //절약된 시간 계산

                switch (log[index - 6])
                { //동작마다 각각의 분당 절약전략을 절약 시간에 곱해 절약전략량을 계산한다
                    case "SLEEP": //모니터 끄기
                        saveEnergy[day][0] += save_time.TotalMinutes * 0.67;
                        carbon[day] += save_time.TotalMinutes * 0.67 * 7.07 / 1000;
                        saveEnergy[day][4] += save_time.TotalMinutes * 0.67; //해당 날짜의 절약전력 총합
                        break;
                    case "HIBERNATE": //최대 절전
                        saveEnergy[day][1] += save_time.TotalMinutes * 0.67;
                        carbon[day] += save_time.TotalMinutes * 2.65 * 7.07 / 1000;
                        saveEnergy[day][4] += save_time.TotalMinutes * 0.67; //해당 날짜의 절약전력 총합
                        break;
                    case "SUSPEND": // 대기
                        saveEnergy[day][2] += save_time.TotalMinutes * 0.67;
                        carbon[day] += save_time.TotalMinutes * 2.63 * 7.07 / 1000;
                        saveEnergy[day][4] += save_time.TotalMinutes * 0.67; //해당 날짜의 절약전력 총합
                        break;
                    case "SHUTDOWN": //컴퓨터 종료
                        saveEnergy[day][3] += save_time.TotalMinutes * 0.67;
                        carbon[day] += save_time.TotalMinutes * 2.67 * 7.07 / 1000;
                        saveEnergy[day][4] += save_time.TotalMinutes * 0.67; //해당 날짜의 절약전력 총합
                        break;
                    default:
                        //동작명령 에러!
                        break;
                }

                if (!log[index - 5].Substring(0, log[index - 5].IndexOf("오")).Equals(log[index - 9].Substring(0, log[index - 9].IndexOf("오"))))
                {
                    //다음 로그의 날짜가 다르면 day++하여 다음날 절약전력에 더하도록 함
                    day++;
                }
                index -= 4; //다음 로그기록으로 이동

            }
        }

        public void getLog() //진짜 로그파일 읽어와서 log[]배열 만들기
        {
            ConnectServer ConServ = new ConnectServer(); //서버 통신 클래스
            string temp = ConServ.LogText.Replace("<BR>", "|");
            log = temp.Split('|');
        }

        //텍스트파일 읽어서 배열에 저장(로그기록 불러오는 대신)
        public void readText() //getlog()대신 가상의 로그기록을 사용. 차트기능 테스트위해
        {
            TextReader streamreader = new StreamReader("log.txt", System.Text.Encoding.Default);
            string resultStr = streamreader.ReadToEnd();
            string temp = resultStr.Replace("<BR>", "|");
            log = temp.Split('|');
            //로그번호 앞에 "\r\n"이 붙긴 하지만 로그번호는 사용되지 않으니까 크게 문제 안됨
        }

        //원형차트 데이터(하루 절약 전력량 비교)
        public String getPieChartData()
        {
            string[] valueX = { "SLEEP", "HIBERNATE", "SUSPEND", "SHUTDOWN" };
            double[] valueY = { saveEnergy[0][0], saveEnergy[0][1], saveEnergy[0][2], saveEnergy[0][3] };

            //서버에 보낼 데이터를 string으로 저장
            String csvData = "";
            for (int i = 0; i < valueX.Length; i++)
            {
                if (csvData.Length == 0)
                {
                    csvData = csvData + valueX[i] + "," + valueY[i];
                }
                else
                {
                    csvData = csvData + "\t" + valueX[i] + "," + valueY[i];
                }
            }
            return csvData;
        }

        //일주일 절약전력량 데이터
        public String getHorizontalStackBarData()
        {
            string[] valueX = { "SLEEP", "HIBERNATE", "SUSPEND", "SHUTDOWN", "CARBON" };
            double[] valueY = { saveEnergy[0][0], saveEnergy[0][1], saveEnergy[0][2], saveEnergy[0][3], saveEnergy[0][3] };

            String rowFormat = "{0}:{1}\t";
            String[] energyValues = { "", "", "", "", "" };

            String horizontalStackData = "";
            for (int i = 0; i < 7; i++)
            {
                if (i == 0)
                {
                    energyValues[0] = String.Concat(energyValues[0], saveEnergy[i][0].ToString());
                    energyValues[1] = String.Concat(energyValues[1], saveEnergy[i][1].ToString());
                    energyValues[2] = String.Concat(energyValues[2], saveEnergy[i][2].ToString());
                    energyValues[3] = String.Concat(energyValues[3], saveEnergy[i][3].ToString());
                    energyValues[4] = String.Concat(energyValues[4], carbon[i].ToString());
                }
                else
                {
                    energyValues[0] = String.Concat(energyValues[0], ", ", saveEnergy[i][0].ToString());
                    energyValues[1] = String.Concat(energyValues[1], ", ", saveEnergy[i][1].ToString());
                    energyValues[2] = String.Concat(energyValues[2], ", ", saveEnergy[i][2].ToString());
                    energyValues[3] = String.Concat(energyValues[3], ", ", saveEnergy[i][3].ToString());
                    energyValues[4] = String.Concat(energyValues[4], ", ", carbon[i].ToString());
                }
            }

            //데이터를 string으로 연결
            for (int i = 0; i < 5; i++)
            {
                horizontalStackData += rowFormat.Replace("{0}", valueX[i]).Replace("{1}", energyValues[i]);
            }
            return horizontalStackData;
        }

        //2주치 절약전력량 데이터
        public String getVerticalStackBarData()
        {
            double[] valueY = { saveEnergy[0][0], saveEnergy[0][1], saveEnergy[0][2], saveEnergy[0][3] };

            double[] thisweek = new double[4];
            double[] lastweek = new double[4];
            for (int i = 0; i < 7; i++)
            {
                thisweek[0] += saveEnergy[i][0];
                thisweek[1] += saveEnergy[i][1];
                thisweek[2] += saveEnergy[i][2];
                thisweek[3] += saveEnergy[i][3];
            }
            for (int i = 7; i < 14; i++)
            {
                lastweek[0] += saveEnergy[i][0];
                lastweek[1] += saveEnergy[i][1];
                lastweek[2] += saveEnergy[i][2];
                lastweek[3] += saveEnergy[i][3];
            }

            //2주치 차트 데이터를 string으로 연결
            String rowFormat = "{0}:{1}\t";
            String sleepData = rowFormat.Replace("{0}", "SLEEP").Replace("{1}", String.Concat(thisweek[0].ToString(), ",", lastweek[0].ToString()));
            String hiberndateData = rowFormat.Replace("{0}", "HIBERNATE").Replace("{1}", String.Concat(thisweek[1].ToString(), ",", lastweek[1].ToString()));
            String suspendData = rowFormat.Replace("{0}", "SUSPEND").Replace("{1}", String.Concat(thisweek[2].ToString(), ",", lastweek[2].ToString()));
            String shutdownData = rowFormat.Replace("\t", "").Replace("{0}", "SHUTDOWN").Replace("{1}", String.Concat(thisweek[3].ToString(), ",", lastweek[3].ToString()));

            return String.Concat(sleepData, hiberndateData, suspendData, shutdownData);
        }


    }
}