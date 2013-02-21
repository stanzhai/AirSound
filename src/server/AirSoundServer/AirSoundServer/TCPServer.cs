//============================================================
//
//    来源：
//    文件名　：TCPServer.cs
//    创建标识：StanZhai 2013/02/21
//    文件版本：1.0.0.0
//
//============================================================
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace AirSoundServer
{
    /// <summary>
    /// TCPServer
    /// </summary>
    public class TCPServer
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public TCPServer()
        {

        }

        //Attribute
        private IPEndPoint m_endpoint;
        private TcpListener m_tcpip;
        private Thread m_ThreadMainServer;
        private ListenerState m_State;


        //Die Liste der laufenden TCPServer-Threads
        private List<ServerThread> m_threads = new List<ServerThread>();

        //Delegates
        public delegate void DelegateClientConnected(ServerThread st);
        public delegate void DelegateClientDisconnected(ServerThread st, string info);
        public delegate void DelegateDataReceived(ServerThread st, Byte[] data);

        //Events
        public event DelegateClientConnected ClientConnected;
        public event DelegateClientDisconnected ClientDisconnected;
        public event DelegateDataReceived DataReceived;

        /// <summary>
        /// TCPServer Stati
        /// </summary>
        public enum ListenerState
        {
            None,
            Started,
            Stopped,
            Error
        };
        /// <summary>
        /// Alle Aktuellen Clients des Servers
        /// </summary>
        public List<ServerThread> Clients
        {
            get
            {
                return m_threads;
            }
        }
        /// <summary>
        /// Connected
        /// </summary>
        public ListenerState State
        {
            get
            {
                return m_State;
            }
        }
        /// <summary>
        /// Gibt den inneren TcpListener des Servers zurück
        /// </summary>
        public TcpListener Listener
        {
            get
            {
                return this.m_tcpip;
            }
        }
        /// <summary>
        /// Starten des Servers
        /// </summary>
        public void Start(string strIPAdress, int Port)
        {
            //Endpoint und Listener bestimmen
            m_endpoint = new IPEndPoint(IPAddress.Parse(strIPAdress), Port);
            m_tcpip = new TcpListener(m_endpoint);

            if (m_tcpip == null) return;

            try
            {
                m_tcpip.Start();

                // Haupt-TCPServer-Thread initialisieren und starten
                m_ThreadMainServer = new Thread(new ThreadStart(Run));
                m_ThreadMainServer.Start();

                //State setzen
                this.m_State = ListenerState.Started;
            }
            catch (Exception ex)
            {
                //Beenden
                m_tcpip.Stop();
                this.m_State = ListenerState.Error;

                //Exception werfen
                throw ex;
            }
        }
        /// <summary>
        /// Run
        /// </summary>
        private void Run()
        {
            while (true)
            {
                //Wartet auf eingehenden Verbindungswunsch
                TcpClient client = m_tcpip.AcceptTcpClient();
                //Initialisiert und startet einen TCPServer-Thread
                //und fügt ihn zur Liste der TCPServer-Threads hinzu
                ServerThread st = new ServerThread(client);

                //Events hinzufügen
                st.DataReceived += new ServerThread.DelegateDataReceived(OnDataReceived);
                st.ClientDisconnected += new ServerThread.DelegateClientDisconnected(OnClientDisconnected);

                //Weitere Arbeiten
                OnClientConnected(st);

                //Beginnen zu lesen
                client.Client.BeginReceive(st.ReadBuffer, 0, st.ReadBuffer.Length, SocketFlags.None, st.Receive, client.Client);
            }
        }
        /// <summary>
        /// Nachricht an alle verbundenen Clients senden. Gibt die Anzahl der vorhandenen Clients zurück
        /// </summary>
        /// <param name="Message"></param>
        public int Send(Byte[] data)
        {
            //Für jede Verbindung
            foreach (ServerThread sv in m_threads)
            {
                try
                {
                    //Senden
                    if (data.Length > 0)
                    {
                        sv.Send(data);
                    }
                }
                catch (Exception)
                {

                }
            }
            //Anzahl zurückgeben
            return m_threads.Count;
        }
        /// <summary>
        /// Wird ausgeführt wenn Daten angekommen sind
        /// </summary>
        /// <param name="Data"></param>
        private void OnDataReceived(ServerThread st, Byte[] data)
        {
            //Event abschicken bzw. weiterleiten
            if (DataReceived != null)
            {
                DataReceived(st, data);
            }
        }
        /// <summary>
        /// Wird aufgerufen wenn sich ein Client beendet
        /// </summary>
        /// <param name="st"></param>
        private void OnClientDisconnected(ServerThread st, string info)
        {
            //Aus Liste entfernen
            m_threads.Remove(st);

            //Event abschicken bzw. weiterleiten
            if (ClientDisconnected != null)
            {
                ClientDisconnected(st, info);
            }
        }
        /// <summary>
        /// Wird aufgerufen wenn sich ein Client verbindet
        /// </summary>
        /// <param name="st"></param>
        private void OnClientConnected(ServerThread st)
        {
            //Wenn nicht vorhanden
            if (!m_threads.Contains(st))
            {
                //Zur Liste der Clients hinzufügen
                m_threads.Add(st);
            }

            //Event abschicken bzw. weiterleiten
            if (ClientConnected != null)
            {
                ClientConnected(st);
            }
        }
        /// <summary>
        /// Beenden des Servers
        /// </summary>
        public void Stop()
        {
            try
            {
                if (m_ThreadMainServer != null)
                {
                    // Haupt-TCPServer-Thread stoppen
                    m_ThreadMainServer.Abort();
                    System.Threading.Thread.Sleep(100);
                }

                // Alle TCPServer-Threads stoppen
                for (IEnumerator en = m_threads.GetEnumerator(); en.MoveNext(); )
                {
                    //Nächsten TCPServer-Thread holen
                    ServerThread st = (ServerThread)en.Current;
                    //und stoppen
                    st.Stop();

                    //Event abschicken
                    if (ClientDisconnected != null)
                    {
                        ClientDisconnected(st, "Verbindung wurde beendet");
                    }
                }

                if (m_tcpip != null)
                {
                    //Listener stoppen
                    m_tcpip.Stop();
                    m_tcpip.Server.Close();
                }

                //Liste leeren
                m_threads.Clear();
                //Status vermerken
                this.m_State = ListenerState.Stopped;

            }
            catch (Exception)
            {
                this.m_State = ListenerState.Error;
            }
        }
    }
}
