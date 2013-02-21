using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirSoundServer
{
    public class ServerThreadData
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public ServerThreadData()
        {

        }

        //Attribute
        public ServerThread ServerThread;
        public WinSound.Player Player;
        public WinSound.JitterBuffer JitterBuffer;
        public WinSound.Protocol Protocol;
        public int SamplesPerSecond = 8000;
        public int BitsPerSample = 16;
        public int SoundBufferCount = 8;
        public uint JitterBufferCount = 20;
        public uint JitterBufferMilliseconds = 20;
        public int Channels = 1;
        private bool IsInitialized = false;

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="bitsPerSample"></param>
        /// <param name="channels"></param>
        public void Init(ServerThread st, string soundDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int soundBufferCount, uint jitterBufferCount, uint jitterBufferMilliseconds)
        {
            //Werte übernehmen
            this.ServerThread = st;
            this.SamplesPerSecond = samplesPerSecond;
            this.BitsPerSample = bitsPerSample;
            this.Channels = channels;
            this.SoundBufferCount = soundBufferCount;
            this.JitterBufferCount = jitterBufferCount;
            this.JitterBufferMilliseconds = jitterBufferMilliseconds;

            //Player
            this.Player = new WinSound.Player();
            this.Player.Open(soundDeviceName, samplesPerSecond, bitsPerSample, channels, soundBufferCount);

            //Wenn ein JitterBuffer verwendet werden soll
            if (jitterBufferCount >= 2)
            {
                //Neuen JitterBuffer erstellen
                this.JitterBuffer = new WinSound.JitterBuffer(this.Player, jitterBufferCount, jitterBufferMilliseconds);
                this.JitterBuffer.DataAvailable += new WinSound.JitterBuffer.DelegateDataAvailable(OnJitterBufferDataAvailable);
                this.JitterBuffer.Start();
            }

            //Protocol
            this.Protocol = new WinSound.Protocol(WinSound.ProtocolTypes.LH, Encoding.Default);
            this.Protocol.DataComplete += new WinSound.Protocol.DelegateDataComplete(OnProtocolDataComplete);


            //Initialisiert
            IsInitialized = true;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            //Protocol
            if (Protocol != null)
            {
                this.Protocol.DataComplete -= new WinSound.Protocol.DelegateDataComplete(OnProtocolDataComplete);
                this.Protocol = null;
            }

            //JitterBuffer
            if (JitterBuffer != null)
            {
                JitterBuffer.Stop();
                JitterBuffer.DataAvailable -= new WinSound.JitterBuffer.DelegateDataAvailable(OnJitterBufferDataAvailable);
                this.JitterBuffer = null;
            }

            //Player
            if (Player != null)
            {
                Player.Close();
                this.Player = null;
            }

            //Nicht initialisiert
            IsInitialized = false;
        }
        /// <summary>
        /// OnProtocolDataComplete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void OnProtocolDataComplete(Object sender, Byte[] bytes)
        {
            //Wenn initialisiert
            if (IsInitialized)
            {
                if (ServerThread != null && Player != null)
                {
                    try
                    {
                        //Wenn der Player gestartet wurde
                        if (Player.Opened)
                        {
                            //RTP Header auslesen
                            WinSound.RTPPacket rtp = new WinSound.RTPPacket(bytes);

                            ////Wenn Anzeige
                            //if (IsDrawCurve)
                            //{
                            //    TimeMeasurement();
                            //    m_BytesToDraw = rtp.Data;
                            //}

                            //Wenn Header korrekt
                            if (rtp.Data != null)
                            {
                                //Wenn JitterBuffer verwendet werden soll
                                if (JitterBuffer != null && JitterBuffer.Maximum >= 2)
                                {
                                    JitterBuffer.AddData(rtp);
                                }
                                else
                                {
                                    //Nach Linear umwandeln
                                    Byte[] linearBytes = WinSound.Utils.MuLawToLinear(rtp.Data, this.BitsPerSample, this.Channels);
                                    //Abspielen
                                    Player.PlayData(linearBytes, false);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        IsInitialized = false;
                    }
                }
            }
        }
        /// <summary>
        /// OnJitterBufferDataAvailable
        /// </summary>
        /// <param name="packet"></param>
        private void OnJitterBufferDataAvailable(Object sender, WinSound.RTPPacket rtp)
        {
            if (Player != null)
            {
                //Nach Linear umwandeln
                Byte[] linearBytes = WinSound.Utils.MuLawToLinear(rtp.Data, BitsPerSample, Channels);
                //Abspielen
                Player.PlayData(linearBytes, false);
            }
        }
    }

}
