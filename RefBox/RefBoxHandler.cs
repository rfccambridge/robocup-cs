using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Core;

namespace RefBox
{
    abstract public class RefBoxHandler
    {
        public struct RefBoxPacket
        {
            public char cmd;
            public byte cmd_counter;    // counter for current command
            public byte goals_blue;      // current score for blue team
            public byte goals_yellow;    // current score for yellow team
            public short time_remaining; // seconds remaining for current game stage (network byte order)

            public byte[] toByte()
            {
                int len = Marshal.SizeOf(this);
                byte[] arr = new byte[len];
                IntPtr ptr = Marshal.AllocHGlobal(len);
                Marshal.StructureToPtr(this, ptr, true);
                Marshal.Copy(ptr, arr, 0, len);
                Marshal.FreeHGlobal(ptr);
                return arr;
            }

            public void setVals(byte[] dataBytes)
            {
                RefBoxPacket obj = new RefBoxPacket();
                int len = Marshal.SizeOf(obj);
                IntPtr i = Marshal.AllocHGlobal(len);
                Marshal.Copy(dataBytes, 0, i, len);
                obj = (RefBoxPacket)Marshal.PtrToStructure(i, obj.GetType());
                Marshal.FreeHGlobal(i);
                this = obj;
            }

            public int getSize()
            {
                return Marshal.SizeOf(this);
            }
        };

        // Note: If you change these, don't forget to change the names in the map
        public const char HALT = 'H';
        public const char STOP = 'S';
        public const char READY = ' ';
        public const char START = 's';
        public const char KICKOFF_YELLOW = 'k';
        public const char PENALTY_YELLOW = 'p';
        public const char DIRECT_YELLOW = 'f';
        public const char INDIRECT_YELLOW = 'i';
        public const char TIMEOUT_YELLOW = 't';
        public const char TIMEOUT_END_YELLOW = 'z';

        public const char KICKOFF_BLUE = 'K';
        public const char PENALTY_BLUE = 'P';
        public const char DIRECT_BLUE = 'F';
        public const char INDIRECT_BLUE = 'I';
        public const char TIMEOUT_BLUE = 'T';
        public const char TIMEOUT_END_BLUE = 'Z';

        public const char CANCEL = 'c';

        private static Dictionary<char, string> COMMAND_CHAR_TO_NAME;

        protected RefBoxHandler()
        {
            COMMAND_CHAR_TO_NAME = new Dictionary<char, string>();

            COMMAND_CHAR_TO_NAME.Add('H', "HALT");
            COMMAND_CHAR_TO_NAME.Add('S', "STOP");
            COMMAND_CHAR_TO_NAME.Add(' ', "READY");
            COMMAND_CHAR_TO_NAME.Add('s', "START");
            COMMAND_CHAR_TO_NAME.Add('k', "KICKOFF_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('p', "PENALTY_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('f', "DIRECT_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('i', "INDIRECT_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('t', "TIMEOUT_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('z', "TIMEOUT_END_YELLOW");

            COMMAND_CHAR_TO_NAME.Add('K', "KICKOFF_BLUE");
            COMMAND_CHAR_TO_NAME.Add('P', "PENALTY_BLUE");
            COMMAND_CHAR_TO_NAME.Add('F', "DIRECT_BLUE");
            COMMAND_CHAR_TO_NAME.Add('I', "INDIRECT_BLUE");
            COMMAND_CHAR_TO_NAME.Add('T', "TIMEOUT_BLUE");
            COMMAND_CHAR_TO_NAME.Add('Z', "TIMEOUT_END_BLUE");

            COMMAND_CHAR_TO_NAME.Add('c', "CANCEL");

            COMMAND_CHAR_TO_NAME.Add('g', "GOAL_SCORED_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('d', "DECREASE_GOAL_SCORE_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('y', "YELLOW_CARD_YELLOW");
            COMMAND_CHAR_TO_NAME.Add('r', "RED_CARD_YELLOW");

            COMMAND_CHAR_TO_NAME.Add('G', "GOAL_SCORED_BLUE");
            COMMAND_CHAR_TO_NAME.Add('D', "DECREASE_GOAL_SCORE_BLUE");
            COMMAND_CHAR_TO_NAME.Add('Y', "YELLOW_CARD_BLUE");
            COMMAND_CHAR_TO_NAME.Add('R', "RED_CARD_BLUE");
        }

        public static string CommandCharToName(char c)
        {
            string name;
            if (COMMAND_CHAR_TO_NAME.TryGetValue(c, out name))
                return name;
            return "";
        }

        protected Thread _handlerThread;
        protected EndPoint _endPoint;
        protected Socket _socket;

        protected RefBoxPacket _lastPacket;
        protected DateTime _lastReceivedTime;
        protected object lastPacketLock = new object();

        abstract public void Connect(string addr, int port);
        abstract protected void Loop();

        public virtual void Disconnect()
        {
            if (_socket == null)
                throw new ApplicationException("Not connected.");
            if (_handlerThread != null)
                throw new ApplicationException("Must stop before disconnecting.");

            _socket.Close();
            _socket = null;
        }

        public virtual void Start()
        {
            _handlerThread = new Thread(new ThreadStart(Loop));
            _handlerThread.Start();
        }

        public virtual void Stop()
        {
            _handlerThread.Abort();
            _handlerThread = null;
        }

        public int GetCmdCounter()
        {
            lock (lastPacketLock)
            {
                return _lastPacket.cmd_counter;
            }
        }

        protected RefBoxPacket GetLastPacket()
        {
            lock (lastPacketLock)
            {
                return _lastPacket;
            }
        }

        public char GetLastCommand()
        {
            lock (lastPacketLock)
            {
                return _lastPacket.cmd;
            }
        }
        public Score GetScore()
        {
            Score score = new Score();
            RefBoxPacket lastPacket = GetLastPacket();
            score.GoalsYellow = lastPacket.goals_yellow;
            score.GoalsBlue = lastPacket.goals_blue;
            return score;
        }

    }
}
