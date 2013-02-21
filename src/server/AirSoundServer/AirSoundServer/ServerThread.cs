using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AirSoundServer
{
    public class ServerThread
    {
        // Stop-Flag
        private bool m_IsStopped = false;
        // Die Verbindung zum Client
        private TcpClient m_Connection = null;
        //Lesepuffer
        public byte[] ReadBuffer = new byte[1024];

        public delegate void DelegateDataReceived(ServerThread st, Byte[] data);
        public event DelegateDataReceived DataReceived;
        public delegate void DelegateClientDisconnected(ServerThread sv, string info);
        public event DelegateClientDisconnected ClientDisconnected;

        /// <summary>
        /// Inneren Client
        /// </summary>
        public TcpClient Client
        {
            get
            {
                return m_Connection;
            }
        }
        /// <summary>
        /// Verbindung ist beendet
        /// </summary>
        public bool IsStopped
        {
            get
            {
                return m_IsStopped;
            }
        }
        // Speichert die Verbindung zum Client und startet den Thread
        public ServerThread(TcpClient connection)
        {
            // Speichert die Verbindung zu Client,
            // um sie später schließen zu können
            this.m_Connection = connection;
        }
        /// <summary>
        /// Nachrichten lesen
        /// </summary>
        /// <param name="ar"></param>
        public void Receive(IAsyncResult ar)
        {
            try
            {
                //Wenn nicht mehr verbunden
                if (this.m_Connection.Client.Connected == false)
                {
                    return;
                }

                if (ar.IsCompleted)
                {
                    //Lesen
                    int bytesRead = m_Connection.Client.EndReceive(ar);

                    //Wenn Daten vorhanden
                    if (bytesRead > 0)
                    {
                        //Nur gelesene Bytes ermitteln
                        Byte[] data = new byte[bytesRead];
                        System.Array.Copy(ReadBuffer, 0, data, 0, bytesRead);

                        //Event abschicken
                        DataReceived(this, data);
                        //Weiter lesen
                        m_Connection.Client.BeginReceive(ReadBuffer, 0, ReadBuffer.Length, SocketFlags.None, Receive, m_Connection.Client);
                    }
                    else
                    {
                        //Verbindung getrennt
                        HandleDisconnection("Verbindung wurde beendet");
                    }
                }
            }
            catch (Exception ex)
            {
                //Verbindung getrennt
                HandleDisconnection(ex.Message);
            }
        }
        /// <summary>
        /// Alles nötige bei einem Verbindungsabbruch unternehmen
        /// </summary>
        private void HandleDisconnection(string reason)
        {
            //Clientverbindung ist beendet
            m_IsStopped = true;

            //Event abschicken
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, reason);
            }
        }
        /// <summary>
        /// Senden von Nachrichten
        /// </summary>
        /// <param name="strMessage"></param>
        public void Send(Byte[] data)
        {
            try
            {
                //Wenn die Verbindung noch besteht
                if (this.m_IsStopped == false)
                {
                    //Hole den Stream für's schreiben
                    NetworkStream ns = this.m_Connection.GetStream();

                    lock (ns)
                    {
                        // Sende den kodierten string an den TCPServer
                        ns.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                //Verbindung schliessen
                this.m_Connection.Close();
                //Verbindung beenden
                this.m_IsStopped = true;
                //Exception weiterschicken
                throw ex;
            }
        }
        /// <summary>
        /// Thread anhalten
        /// </summary>
        public void Stop()
        {
            //Wenn ein Client noch verbunden ist
            if (m_Connection.Client.Connected == true)
            {
                //Verbindung beenden
                m_Connection.Client.Disconnect(false);
            }

            this.m_IsStopped = true;
        }

    }
}
