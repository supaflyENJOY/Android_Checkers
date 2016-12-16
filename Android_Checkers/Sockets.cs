using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using System.Threading;

namespace Android_Checkers.Assets {
    enum MessageType {
        Move,
        Remove,
        Clear,
        StartGame,
        EndGame,
        ChangeTeam,
        ChangeState
    };
    class Sockets {
        ushort PORT;
        string IPADDRESS;
        TcpClient tcpclnt;
        NetworkStream NWS;
        BinaryReader R;
        BinaryWriter W;
        GameBoard gb;
        bool connected = false;
        public Sockets(string ip, ushort port) {
            IPADDRESS = ip;
	        PORT = port;
            tcpclnt = new TcpClient();
        }

        public Sockets() {}

        public void sendMessage(MessageType type, int param1=0, int param2=0, int param3=0, int param4=0) {
            if (!connected) return;
            char[] msg = new char[5];
            msg[0] = (char)type;
            msg[1] = (char)param1;
            msg[2] = (char)param2;
            msg[3] = (char)param3;
            msg[4] = (char)param4;
            W.Write(msg);
        }
        public void Connect() {
            tcpclnt.Connect(IPADDRESS, PORT);
            if(!tcpclnt.Connected) {
                //cout << "Couldn't connect to server!" << endl;
                return;
            }
            NWS = tcpclnt.GetStream();
            R = new BinaryReader(NWS);
            W = new BinaryWriter(NWS);
            ThreadStart receiverDelegate = new ThreadStart(completeConnection);
            Thread receiverThread = new Thread(receiverDelegate);
            receiverThread.Start();
            connected = true;
        }

        public void SetGameBoard(GameBoard _gb) {
            gb = _gb;
        }

        public void completeConnection() {
            while (true) {
                if(tcpclnt.Available > 0) {
                    var data = new char[10];
                    R.Read(data,0, tcpclnt.Available);
                    System.Diagnostics.Debug.WriteLine(Enum.GetName(typeof(MessageType), data[0]));

                    if (data[0] == (int)MessageType.Clear) {
                        gb.Clear();
                    } else if (data[0] == (int)MessageType.StartGame) {
                        gb.StartGame();
                    } else if (data[0] == (int)MessageType.Move) {
                        gb.MoveByXY(data[1], data[2], data[3], data[4]);
                    } else if (data[0] == (int)MessageType.Remove) {
                        gb.RemoveByXY(data[1], data[2]);
                    } else if (data[0] == (int)MessageType.ChangeTeam) {
                        gb.SetMyTeam((CheckerTeam)data[1]);
                    } else if (data[0] == (int)MessageType.ChangeState) {
                        gb.ChangeState(data[1]);
                    }
                }
            }
        }
    }
}