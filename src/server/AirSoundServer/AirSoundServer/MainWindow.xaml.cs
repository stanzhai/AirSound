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
        private Configuration m_Config = new Configuration();

        private int m_SoundBufferCount = 8;
        private bool m_IsFormMain = true;
        private long m_SequenceNumber = 4596;
        private long m_TimeStamp = 0;
        private int m_Version = 2;
        private bool m_Padding = false;
        private bool m_Extension = false;
        private int m_CSRCCount = 0;
        private bool m_Marker = false;
        private int m_PayloadType = 0;
        private uint m_SourceId = 0;
        private bool m_IsTimerStreamRunning = false;


        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            InitDevs();
            InitJitterBuffer();
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
        /// InitJitterBuffer
        /// </summary>
        private void InitJitterBuffer()
        {
            //Wenn vorhanden
            if (m_JitterBufferClient != null)
            {
                m_JitterBufferClient.DataAvailable -= new WinSound.JitterBuffer.DelegateDataAvailable(OnJitterBufferClientDataAvailable);
            }

            //Neu erstellen
            m_JitterBufferClient = new WinSound.JitterBuffer(null, m_Config.JitterBufferCount, 20);
            m_JitterBufferClient.DataAvailable += new WinSound.JitterBuffer.DelegateDataAvailable(OnJitterBufferClientDataAvailable);
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
                    m_Server.Start(31832);

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
                //data.Init(st, m_Config.SoundDeviceNameServer, 8000, 16, 1, 8, 20, 20);
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
                listBoxClients.Dispatcher.Invoke(new Action(() =>
                {
                    listBoxClients.Items.Add(st.Client.Client.RemoteEndPoint.ToString());
                }));
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
                listBoxClients.Dispatcher.Invoke(new Action(() =>
                {
                    listBoxClients.Items.Remove(st.Client.Client.RemoteEndPoint.ToString());
                }));
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
                    if (IsServerRunning)
                    {
                        //Wenn Form noch aktiv
                        if (m_IsFormMain)
                        {
                            Byte[] rtp = ToRTPData(data, m_Config.BitsPerSampleClient, m_Config.ChannelsClient);
                            //Absenden
                            m_Server.Send(m_PrototolClient.ToBytes(rtp));

                            //Wenn JitterBuffer
                            //if (m_Config.IsTimeSyncClient)
                            //{
                            //    //Sounddaten in kleinere Einzelteile zerlegen
                            //    int bytesPerInterval = WinSound.Utils.GetBytesPerInterval((uint)m_Config.SamplesPerSecondClient, m_Config.BitsPerSampleClient, m_Config.ChannelsClient);
                            //    int count = data.Length / bytesPerInterval;
                            //    int currentPos = 0;
                            //    for (int i = 0; i < count; i++)
                            //    {
                            //        //Teilstück in RTP Packet umwandeln
                            //        Byte[] partBytes = new Byte[bytesPerInterval];
                            //        Array.Copy(data, currentPos, partBytes, 0, bytesPerInterval);
                            //        currentPos += bytesPerInterval;
                            //        WinSound.RTPPacket rtp = ToRTPPacket(partBytes, m_Config.BitsPerSampleClient, m_Config.ChannelsClient);
                            //        //In Buffer legen
                            //        m_JitterBufferClient.AddData(rtp);
                            //    }
                            //}
                            //else
                            //{
                            //    //Alles in RTP Packet umwandeln
                            //}
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
                if (IsServerRunning)
                {
                    if (m_IsFormMain)
                    {
                        //RTP Packet in Bytes umwandeln
                        Byte[] rtpBytes = rtp.ToBytes();
                        //Absenden
                        m_Server.Send(m_PrototolClient.ToBytes(rtpBytes));
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
                if (IsServerRunning == false)
                {
                    //Buffer Grösse berechnen
                    int bufferSize = 0;
                    if (m_Config.IsTimeSyncClient)
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
                        if (m_Recorder.Start(comboBoxDevs.SelectedItem.ToString(), m_Config.SamplesPerSecondClient, m_Config.BitsPerSampleClient, m_Config.ChannelsClient, m_SoundBufferCount, bufferSize))
                        {
                            //Wenn JitterBuffer
                            if (m_Config.IsTimeSyncClient)
                            {
                                m_JitterBufferClient.Start();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
        /// <summary>
        /// StopRecordingFromSounddevice
        /// </summary>
        private void StopRecordingFromSounddevice()
        {
            try
            {
                if (IsServerRunning)
                {
                    //Stoppen
                    m_Recorder.Stop();

                    //Events entfernen
                    m_Recorder.DataRecorded -= new WinSound.Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard);
                    m_Recorder.RecordingStopped -= new WinSound.Recorder.DelegateStopped(OnRecordingStopped);
                    m_Recorder = null;

                    //Wenn JitterBuffer
                    if (m_Config.IsTimeSyncClient)
                    {
                        m_JitterBufferClient.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
        /// <summary>
        /// OnRecordingStopped
        /// </summary>
        private void OnRecordingStopped()
        {
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
                StopRecordingFromSounddevice();
                StopServer();
            }
            else
            {
                StartRecordingFromSounddevice();
                StartServer();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_IsFormMain = false;
            StopRecordingFromSounddevice();
            StopServer();
        }
    }
}
