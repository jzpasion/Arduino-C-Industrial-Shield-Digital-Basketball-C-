using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalBasketBall
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ArtNet.ArtNet.Initialize();
        }
        byte[] dmxValues = new byte[512];

        string ArtLynxIPAdd = "192.168.1.100";

        Thread startReading;

        private int readyCounter = 4;
        private int counter = 60;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            
            counter--;
            if (counter == 0)
            {
                timer4.Start();
                timer4.Interval = 1000;
               
                counter = 60;
                lblPoints.Text = "00";

                UdpClient udpClient = new UdpClient();
                udpClient.Connect("192.168.1.169", Convert.ToInt32(8888));
                Byte[] senddata = Encoding.ASCII.GetBytes("END");
                udpClient.Send(senddata, senddata.Length);


            }
            lblTimer.Text = counter.ToString();
            timer2.Enabled = true;

          
            
            
           
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ArtNet.ArtNet.Initialize();
            ArtNet.ArtNet.SendArtPoll(ArtLynxIPAdd);
            ArtNet.ArtNet.SendArtPollReply(ArtLynxIPAdd);
            ArtNet.ArtNet.SendArtDmx(ArtLynxIPAdd, 0, 1, dmxValues);

            Thread thdUdpServer = new Thread(new ThreadStart(serverThread));
            thdUdpServer.Start();

            startReading = new Thread(startRead_InOut);
            startReading.Start();

            timer4.Enabled = true;
            timer4.Interval = 1000;
        }

        public void serverThread()
        {
            UdpClient udpClient = new UdpClient(8888);
            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8888);
                Byte[] receivebytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receivebytes);

                this.Invoke(new MethodInvoker(delegate ()
                {
                    if(returnData.ToString() == "Start")
                    {
                       
                        udpClient.Connect("192.168.1.169", Convert.ToInt32(8888));
                        Byte[] senddata = Encoding.ASCII.GetBytes("END");
                        udpClient.Send(senddata, senddata.Length);

                        rdyTimer.Start();
                        rdyTimer.Interval = 1000;

                     

                    }
                    else if (returnData.ToString() == "Clear")
                    {
                       

                        if(timer1.Enabled == false)
                        {
                            return;
                        }
                        

                        int addpoints = Convert.ToInt32(lblPoints.Text);

                        int latestpoints = addpoints += 2;

                        lblPoints.Text = latestpoints.ToString();

                        timer3.Enabled = true;
                        timer3.Interval = 50;
                        lightscore = 2;



                    }

                }));
                    }
        }
        void startRead_InOut()
        {
            while (true)
            {

                try
                {
                    ArtNet.ArtNet.SendArtDmx(ArtLynxIPAdd, 0, 1, dmxValues);
                    //ArtNet.ArtNet.SendArtDmx(ArtLynxIPAdd, 0, 2, dmxValues);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }
        private int x = 1;
       
        private void Timer2_Tick(object sender, EventArgs e)
        {
        
          int y = x -3;
            if(x < 26)
            {

           
            if(x == 25)
            {
                x = 1;
                timer2.Enabled = false;
                    runLights();
                }
            if(y < 0)
            {
                    y += 3;
                }
            dmxValues[y] = Convert.ToByte(10);
            dmxValues[x] = Convert.ToByte(10);
                x += 3;
            }
        }
        void ScoreLights()
        {
            lightsOff();

            int[] rlights = { 1, 7, 13, 19 };
            int[] blights = { 3, 9, 15, 21 };

            for (int i = 0; i < rlights.Length; i++)
            {
                dmxValues[rlights[i]] = Convert.ToByte(10);
                dmxValues[blights[i]] = Convert.ToByte(10);
            }
    

        }
        void runLights()
        {
            
            for (int a = 0; a < 24; a++)
            {
                dmxValues[a] = Convert.ToByte(100);
            }
        }
        void lightsOff()
        {
            for (int i = 0; i < 24; i++)
            {
                dmxValues[i] = Convert.ToByte(0);
            }
            timer2.Enabled = false;
        }

        private int lightscore = 2;
        private void Timer3_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            lightsOff();
           
            lightscore--;
            if (lightscore == 0)
            {
                timer3.Stop();
                runLights();
                timer2.Start();

            }

            lightsOff();
            ScoreLights();
    


        }

        private int Endlight = 2;
        private void Timer4_Tick(object sender, EventArgs e)
        {


            lightsOff();
            Endlight--;
            timer1.Stop();
            timer2.Stop();

            if(Endlight == 1)
            {
                dmxValues[3] = Convert.ToByte(10);
                dmxValues[9] = Convert.ToByte(10);
                dmxValues[15] = Convert.ToByte(10);
                dmxValues[21] = Convert.ToByte(10);

                dmxValues[1] = Convert.ToByte(10);
                dmxValues[7] = Convert.ToByte(10);
                dmxValues[13] = Convert.ToByte(10);
                dmxValues[19] = Convert.ToByte(10);
                
                timer4.Start();
            }
            else
            {
                dmxValues[2] = Convert.ToByte(10);
                dmxValues[8] = Convert.ToByte(10);
                dmxValues[14] = Convert.ToByte(10);
                dmxValues[20] = Convert.ToByte(10);

                dmxValues[4] = Convert.ToByte(10);
                dmxValues[10] = Convert.ToByte(10);
                dmxValues[16] = Convert.ToByte(10);
                dmxValues[22] = Convert.ToByte(10);
               
                Endlight = 2;
                    timer4.Start();
                }
            




        }

        private void Timer5_Tick(object sender, EventArgs e)
        {
            readyCounter--;
            if (readyCounter == 0)
            {
                rdyTimer.Stop();
                timer1 = new System.Windows.Forms.Timer();
                timer1.Tick += new EventHandler(Timer1_Tick);

                timer1.Interval = 1000; // 1 second
                timer1.Start();
                lblTimer.Text = readyCounter.ToString();
                runLights();
                timer2.Enabled = true;
                timer2.Interval = 50;
               
                lblPoints.Text = "00";
                readyCounter = 4;
                timer4.Stop();
            }
            else
            {
                timer4.Stop();
                lightsOff();
                lblTimer.Text = readyCounter.ToString();
            }
          
        }
    }
}
