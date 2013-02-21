using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AirSoundServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Object LockerDictionary = new Object();
        private Dictionary<ServerThread, ServerThreadData> m_DictionaryServerDatas = new Dictionary<ServerThread, ServerThreadData>();
        private TCPServer m_Server;
        private WinSound.Recorder m_Recorder;
        private uint m_RecorderFactor = 4;
        private WinSound.JitterBuffer m_JitterBufferClient;
        public WinSound.WaveFileHeader m_FileHeader = new WinSound.WaveFileHeader();
        private WinSound.Protocol m_PrototolClient = new WinSound.Protocol(WinSound.ProtocolTypes.LH, Encoding.Default);

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            InitDevs();
        }

        /// <summary>
        /// 初始化录音设备
        /// </summary>
        private void InitDevs()
        {
            comboBoxDevs.Items.Clear();
            List<String> namesClient = WinSound.WinSound.GetRecordingNames();

            foreach (String name in namesClient.Where(x => x != null))
            {
                comboBoxDevs.Items.Add(name);
            }

            if (comboBoxDevs.Items.Count > 0)
            {
                comboBoxDevs.SelectedIndex = 0;
            }
        }

        /// <summary>
        ///IsServerRunning 
        /// </summary>
        private bool IsServerRunning
        {
            get
            {
                if (m_Server != null)
                {
                    return m_Server.State == TCPServer.ListenerState.Started;
                }
                return false;
            }
        }


        /// <summary>
        /// StartServer
        /// </summary>
        private void StartServer()
        {
            try
            {
                if (IsServerRunning == false)
                {
                    m_Server = new TCPServer();
                    m_Server.ClientConnected += new TCPServer.DelegateClientConnected(OnServerClientConnected);
                    m_Server.ClientDisconnected += new TCPServer.DelegateClientDisconnected(OnServerClientDisconnected);
                    m_Server.DataReceived += new TCPServer.DelegateDataReceived(OnServerDataReceiced);
                    m_Server.Start("127.0.0.1", 31832);

                    //Je nach Server Status
                    if (m_Server.State == TCPServer.ListenerState.Started)
                    {
                        ShowServerStarted();
                    }
                    else
                    {
                        ShowServerStopped();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
        /// <summary>
        /// StopServer
        /// </summary>
        private void StopServer()
        {
            try
            {
                if (IsServerRunning == true)
                {

                    //Player beenden
                    DeleteAllServerThreadDatas();

                    //Server beenden
                    m_Server.Stop();
                    m_Server.ClientConnected -= new TCPServer.DelegateClientConnected(OnServerClientConnected);
                    m_Server.ClientDisconnected -= new TCPServer.DelegateClientDisconnected(OnServerClientDisconnected);
                    m_Server.DataReceived -= new TCPServer.DelegateDataReceived(OnServerDataReceiced);
                }

                //Je nach Server Status
                if (m_Server != null)
                {
                    if (m_Server.State == TCPServer.ListenerState.Started)
                    {
                        ShowServerStarted();
                    }
                    else
                    {
                        ShowServerStopped();
                    }
                }

                //Fertig
                m_Server = null;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void DeleteAllServerThreadDatas()
        {
            lock (LockerDictionary)
            {
                try
                {
                    foreach (ServerThreadData info in m_DictionaryServerDatas.Values)
                    {
                        info.Dispose();
                    }
                    m_DictionaryServerDatas.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        /// <summary>
        /// OnServerClientConnected
        /// </summary>
        /// <param name="st"></param>
        private void OnServerClientConnected(ServerThread st)
        {
            try
            {
                //ServerThread Daten erstellen
                ServerThreadData data = new ServerThreadData();
                //Initialisieren
                data.Init(st, m_Config.SoundDeviceNameServer, 8000, 16, 1, 8, 20, 20);
                //Hinzufügen
                m_DictionaryServerDatas[st] = data;
                //Zu FlowLayoutPanels hinzufügen
                AddServerClientToFlowLayoutPanel_ServerClient(st);
                AddServerClientToFlowLayoutPanel_ServerProgressBars(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// AddServerClientToFlowLayoutPanel_ServerClient
        /// </summary>
        /// <param name="st"></param>
        private void AddServerClientToFlowLayoutPanel_ServerClient(ServerThread st)
        {
            try
            {
                //FlowLayoutPanelServerClients.Invoke(new MethodInvoker(delegate()
                //{
                //    //Label erstellen
                //    Label lab = new Label();
                //    lab.AutoSize = false;
                //    lab.BackColor = Color.DimGray;
                //    lab.ForeColor = Color.White;
                //    lab.Font = new Font(lab.Font, FontStyle.Bold);
                //    lab.Margin = new Padding(5, FlowLayoutPanelServerClients.Controls.Count > 0 ? 5 : 10, 0, 5);
                //    lab.TextAlign = ContentAlignment.MiddleCenter;
                //    lab.Width = FlowLayoutPanelServerClients.Width - 10;
                //    lab.Text = String.Format(st.Client.Client.RemoteEndPoint.ToString());
                //    lab.Tag = st;
                //    lab.Name = st.Client.Client.RemoteEndPoint.ToString();

                //    //Hinzufügen
                //    FlowLayoutPanelServerClients.Controls.Add(lab);
                //}));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// AddServerClientToFlowLayoutPanel_ServerProgressBars
        /// </summary>
        /// <param name="st"></param>
        private void AddServerClientToFlowLayoutPanel_ServerProgressBars(ServerThreadData stData)
        {
            try
            {
                //FlowLayoutPanelServerProgressBars.Invoke(new MethodInvoker(delegate()
                //{
                //    //ProgressBar erstellen
                //    ProgressBar prog = new ProgressBar();
                //    prog.AutoSize = false;
                //    prog.Margin = new Padding(5, FlowLayoutPanelServerProgressBars.Controls.Count > 0 ? 5 : 10, 0, 5);
                //    prog.Width = FlowLayoutPanelServerProgressBars.Width - 20;
                //    prog.Tag = stData;
                //    prog.BackColor = Color.White;
                //    prog.Maximum = (int)stData.JitterBuffer.Maximum;
                //    prog.Name = stData.ServerThread.Client.Client.RemoteEndPoint.ToString();

                //    //Hinzufügen
                //    FlowLayoutPanelServerProgressBars.Controls.Add(prog);
                //}));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void RemoveServerClientToFlowLayoutPanel_ServerClient(ServerThread st)
        {
            try
            {
                //FlowLayoutPanelServerClients.Invoke(new MethodInvoker(delegate()
                //{
                //    //Label löschen
                //    FlowLayoutPanelServerClients.Controls.RemoveByKey(st.Client.Client.RemoteEndPoint.ToString());
                //}));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// RemoveServerClientToFlowLayoutPanel_ServerProgressBar
        /// </summary>
        /// <param name="st"></param>
        private void RemoveServerClientToFlowLayoutPanel_ServerProgressBar(ServerThreadData data)
        {
            try
            {
                //FlowLayoutPanelServerProgressBars.Invoke(new MethodInvoker(delegate()
                //{
                //    //Label löschen
                //    FlowLayoutPanelServerProgressBars.Controls.RemoveByKey(data.ServerThread.Client.Client.RemoteEndPoint.ToString());
                //}));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// OnServerClientDisconnected
        /// </summary>
        /// <param name="st"></param>
        /// <param name="info"></param>
        private void OnServerClientDisconnected(ServerThread st, string info)
        {
            try
            {
                //Wenn vorhanden
                if (m_DictionaryServerDatas.ContainsKey(st))
                {
                    //Alle Daten freigeben
                    ServerThreadData data = m_DictionaryServerDatas[st];
                    data.Dispose();
                    lock (LockerDictionary)
                    {
                        //Entfernen
                        m_DictionaryServerDatas.Remove(st);
                    }
                    //Aus FlowLayoutPanels entfernen
                    RemoveServerClientToFlowLayoutPanel_ServerClient(st);
                    RemoveServerClientToFlowLayoutPanel_ServerProgressBar(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// OnServerDataReceiced
        /// </summary>
        /// <param name="st"></param>
        /// <param name="data"></param>
        private void OnServerDataReceiced(ServerThread st, Byte[] data)
        {
            //Wenn vorhanden
            if (m_DictionaryServerDatas.ContainsKey(st))
            {
                //Wenn Protocol
                ServerThreadData stData = m_DictionaryServerDatas[st];
                if (stData.Protocol != null)
                {
                    stData.Protocol.Receive_LH(st, data);
                }
            }
        }

        /// <summary>
        /// OnDataReceivedFromSoundcard
        /// </summary>
        /// <param name="linearData"></param>
        private void OnDataReceivedFromSoundcard(Byte[] data)
        {
            try
            {
                lock (this)
                {
                    if (IsClientConnected)
                    {
                        //Wenn Form noch aktiv
                        if (m_IsFormMain)
                        {
                            //Wenn JitterBuffer
                            if (UseJitterBufferClient)
                            {
                                //Sounddaten in kleinere Einzelteile zerlegen
                                int bytesPerInterval = WinSound.Utils.GetBytesPerInterval((uint)m_Config.SamplesPerSecondClient, m_Config.BitsPerSampleClient, m_Config.ChannelsClient);
                                int count = data.Length / bytesPerInterval;
                                int currentPos = 0;
                                for (int i = 0; i < count; i++)
                                {
                                    //Teilstück in RTP Packet umwandeln
                                    Byte[] partBytes = new Byte[bytesPerInterval];
                                    Array.Copy(data, currentPos, partBytes, 0, bytesPerInterval);
                                    currentPos += bytesPerInterval;
                                    WinSound.RTPPacket rtp = ToRTPPacket(partBytes, m_Config.BitsPerSampleClient, m_Config.ChannelsClient);
                                    //In Buffer legen
                                    m_JitterBufferClient.AddData(rtp);
                                }
                            }
                            else
                            {
                                //Alles in RTP Packet umwandeln
                                Byte[] rtp = ToRTPData(data, m_Config.BitsPerSampleClient, m_Config.ChannelsClient);
                                //Absenden
                                m_Server.Send(m_PrototolClient.ToBytes(rtp));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// OnJitterBufferClientDataAvailable
        /// </summary>
        /// <param name="rtp"></param>
        private void OnJitterBufferClientDataAvailable(Object sender, WinSound.RTPPacket rtp)
        {
            try
            {
                if (IsClientConnected)
                {
                    if (m_IsFormMain)
                    {
                        //RTP Packet in Bytes umwandeln
                        Byte[] rtpBytes = rtp.ToBytes();
                        //Absenden
                        m_Client.Send(m_PrototolClient.ToBytes(rtpBytes));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
        /// <summary>
        /// ToRTPData
        /// </summary>
        /// <param name="linearData"></param>
        /// <param name="bitsPerSample"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        private Byte[] ToRTPData(Byte[] data, int bitsPerSample, int channels)
        {
            //Neues RTP Packet erstellen
            WinSound.RTPPacket rtp = ToRTPPacket(data, bitsPerSample, channels);
            //RTPHeader in Bytes erstellen
            Byte[] rtpBytes = rtp.ToBytes();
            //Fertig
            return rtpBytes;
        }
        /// <summary>
        /// ToRTPPacket
        /// </summary>
        /// <param name="linearData"></param>
        /// <param name="bitsPerSample"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        private WinSound.RTPPacket ToRTPPacket(Byte[] linearData, int bitsPerSample, int channels)
        {
            //Daten Nach MuLaw umwandeln
            Byte[] mulaws = WinSound.Utils.LinearToMulaw(linearData, bitsPerSample, channels);

            //Neues RTP Packet erstellen
            WinSound.RTPPacket rtp = new WinSound.RTPPacket();

            //Werte übernehmen
            rtp.Data = mulaws;
            rtp.CSRCCount = m_CSRCCount;
            rtp.Extension = m_Extension;
            rtp.HeaderLength = WinSound.RTPPacket.MinHeaderLength;
            rtp.Marker = m_Marker;
            rtp.Padding = m_Padding;
            rtp.PayloadType = m_PayloadType;
            rtp.Version = m_Version;
            rtp.SourceId = m_SourceId;

            //RTP Header aktualisieren
            try
            {
                rtp.SequenceNumber = Convert.ToUInt16(m_SequenceNumber);
                m_SequenceNumber++;
            }
            catch (Exception)
            {
                m_SequenceNumber = 0;
            }
            try
            {
                rtp.Timestamp = Convert.ToUInt32(m_TimeStamp);
                m_TimeStamp += mulaws.Length;
            }
            catch (Exception)
            {
                m_TimeStamp = 0;
            }

            //Fertig
            return rtp;
        }


        /// <summary>
        /// StartRecordingFromSounddevice
        /// </summary>
        private void StartRecordingFromSounddevice()
        {
            try
            {
                if (IsRecorderFromSounddeviceStarted == false)
                {
                    //Buffer Grösse berechnen
                    int bufferSize = 0;
                    if (UseJitterBufferClient)
                    {
                        bufferSize = WinSound.Utils.GetBytesPerInterval((uint)m_Config.SamplesPerSecondClient, m_Config.BitsPerSampleClient, m_Config.ChannelsClient) * (int)m_RecorderFactor;
                    }
                    else
                    {
                        bufferSize = WinSound.Utils.GetBytesPerInterval((uint)m_Config.SamplesPerSecondClient, m_Config.BitsPerSampleClient, m_Config.ChannelsClient);
                    }

                    //Wenn Buffer korrekt
                    if (bufferSize > 0)
                    {
                        //Recorder erstellen
                        m_Recorder = new WinSound.Recorder();

                        //Events hinzufügen
                        m_Recorder.DataRecorded += new WinSound.Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard);
                        m_Recorder.RecordingStopped += new WinSound.Recorder.DelegateStopped(OnRecordingStopped);

                        //Recorder starten
                        if (m_Recorder.Start(m_Config.SoundDeviceNameClient, m_Config.SamplesPerSecondClient, m_Config.BitsPerSampleClient, m_Config.ChannelsClient, m_SoundBufferCount, bufferSize))
                        {
                            //Anzeigen
                            ShowStreamingFromSounddeviceStarted();

                            //Wenn JitterBuffer
                            if (UseJitterBufferClient)
                            {
                                m_JitterBufferClient.Start();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ShowError(LabelClient, ex.Message);
            }
        }
        /// <summary>
        /// StopRecordingFromSounddevice
        /// </summary>
        private void StopRecordingFromSounddevice()
        {
            try
            {
                if (IsRecorderFromSounddeviceStarted)
                {
                    //Stoppen
                    m_Recorder.Stop();

                    //Events entfernen
                    m_Recorder.DataRecorded -= new WinSound.Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard);
                    m_Recorder.RecordingStopped -= new WinSound.Recorder.DelegateStopped(OnRecordingStopped);
                    m_Recorder = null;

                    //Wenn JitterBuffer
                    if (UseJitterBufferClient)
                    {
                        m_JitterBufferClient.Stop();
                    }

                    //Anzeigen
                    ShowStreamingFromSounddeviceStopped();
                }
            }
            catch (Exception ex)
            {
                ShowError(LabelClient, ex.Message);
            }
        }
        /// <summary>
        /// OnRecordingStopped
        /// </summary>
        private void OnRecordingStopped()
        {
            try
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    //Anzeigen
                    ShowStreamingFromSounddeviceStopped();

                }));
            }
            catch (Exception ex)
            {
                ShowError(LabelClient, ex.Message);
            }
        }


        /// <summary>
        /// ShowServerStarted 
        /// </summary>
        private void ShowServerStarted()
        {
            comboBoxDevs.IsEnabled = false;
            btnStart.Content = "停止";
        }
        /// <summary>
        /// ShowServerStopped
        /// </summary>
        private void ShowServerStopped()
        {
            comboBoxDevs.IsEnabled = true;
            btnStart.Content = "启动";
        }

        /// <summary>
        /// ShowError
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="text"></param>
        private void ShowError(string text)
        {
            MessageBox.Show(text, "出错啦！", MessageBoxButton.OK, MessageBoxImage.Hand);
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (IsServerRunning)
            {
                StopServer();
            }
            else
            {
                StartServer();
            }
        }
    }
}
