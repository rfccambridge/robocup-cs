﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RFC.Core;
using RFC.Messaging;

namespace RFC.RefBox
{
    public class MulticastRefBoxListener : RefBoxHandler
    {
        protected MultiCastRefBoxRouter router = new MultiCastRefBoxRouter();

        Team team;

        public MulticastRefBoxListener(Team team)
        {
            this.team = team;
        }

        /// <summary>
        /// Connects to refobx through a router. First, tries to setup a router listening
        /// to muticast addr:port.If it can't, a router is already running, so we can just try and connect to it.
        /// </summary>
        /// <param name="addr">Multicast address of refbox</param>
        /// <param name="port">Refbox port</param>
        override public void Connect(string addr, int port)
        {
            if (_socket != null)
                throw new ApplicationException("Already connected.");

            router.Connect(addr, port);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bool bound = false;

            for (int i = 0; i < Parameters.routerPorts.Count; i++)
            {
                _endPoint = new IPEndPoint(Parameters.routerIP, Parameters.routerPorts[i]);
                try
                {
                    _socket.Bind(_endPoint);
                    bound = true;
                }
                catch (SocketException)
                {
                    Console.WriteLine("RefBoxListener failed to connect to port {0}!", Parameters.routerPorts[i]);
                }

                if (bound)
                    break;
            }
            if (!_socket.IsBound)
                throw new ApplicationException("RefBoxListener failed to connect to all router ports!");
        }

        public override void Disconnect()
        {
            router.Disconnect();
            base.Disconnect();
        }

        public override void Start()
        {
            router.Start();
            base.Start();
        }

        public override void Stop()
        {
            router.Stop();
            base.Stop();
        }

        public bool IsReceiving()
        {
            const int MAX_ELAPSED = 3; // seconds
            lock (lastPacketLock)
            {
                TimeSpan elapsed = DateTime.Now - _lastReceivedTime;
                return elapsed.TotalSeconds <= MAX_ELAPSED;
            }
        }

        override protected void Loop()
        {
            RefBoxPacket packet = new RefBoxPacket();
            byte[] buffer = new byte[packet.getSize()];

            while (true)
            {
                int rcv = _socket.ReceiveFrom(buffer, 0, packet.getSize(), SocketFlags.None, ref _endPoint);

                packet = new RefBoxPacket();
                if (rcv == packet.getSize())
                {
                    packet.setVals(buffer);

                    char lastCommand;
                    lock (lastPacketLock)
                    {
                        lastCommand = _lastPacket.cmd;
                        _lastReceivedTime = DateTime.Now;
                        _lastPacket = packet;
                    }

                    /*Console.WriteLine("command: " + packet.cmd + " counter: " + packet.cmd_counter
                        + " blue: " + packet.goals_blue + " yellow: " + packet.goals_yellow+
                        " time left: " + packet.time_remaining);*/

                    Score score = new Score();
                    score.GoalsBlue = packet.goals_blue;
                    score.GoalsYellow = packet.goals_yellow;

                    RefboxStateMessage message = new RefboxStateMessage(score, GetPlayType(packet.cmd, lastCommand));
                    ServiceManager.getServiceManager().SendMessage(message);
                }
                else
                {
                    Console.WriteLine("MulticastRefBoxListener: received a packet of wrong size:" + rcv +
                                      " (expecting " + packet.getSize() + ")");
                }
            }
        }

        PlayType GetPlayType(char command, char lastCommand)
        {
            switch ((RefBoxMessageType)command)
            {
                case RefBoxMessageType.HALT:
                    // stop bots completely
                    return PlayType.Halt;
                case RefBoxMessageType.START:
                    return PlayType.NormalPlay;
                case RefBoxMessageType.CANCEL:
                case RefBoxMessageType.STOP:
                case RefBoxMessageType.TIMEOUT_BLUE:
                case RefBoxMessageType.TIMEOUT_YELLOW:
                    //go to stopped/waiting state
                    return PlayType.Stopped;
                case RefBoxMessageType.TIMEOUT_END_BLUE:
                case RefBoxMessageType.TIMEOUT_END_YELLOW:
                case RefBoxMessageType.READY:
                    if (((RefBoxMessageType)lastCommand == RefBoxMessageType.PENALTY_BLUE && team == Team.Blue) ||
                        ((RefBoxMessageType)lastCommand == RefBoxMessageType.PENALTY_YELLOW && team == Team.Yellow))
                        return PlayType.PenaltyKick_Ours;
                    else if (((RefBoxMessageType)lastCommand == RefBoxMessageType.KICKOFF_BLUE && team == Team.Blue) ||
                        ((RefBoxMessageType)lastCommand == RefBoxMessageType.KICKOFF_YELLOW && team == Team.Yellow))
                        return PlayType.KickOff_Ours;
                    else
                        Console.WriteLine("ready state not expected, previous command: " + lastCommand);
                    break;
                case RefBoxMessageType.KICKOFF_BLUE:
                    if (team == Team.Yellow)
                        return PlayType.KickOff_Theirs;
                    else
                        return PlayType.KickOff_Ours_Setup;
                case RefBoxMessageType.INDIRECT_BLUE:
                    if (team == Team.Yellow)
                        return PlayType.Indirect_Theirs;
                    else
                        return PlayType.Indirect_Ours;
                case RefBoxMessageType.DIRECT_BLUE:
                    if (team == Team.Yellow)
                        return PlayType.Direct_Theirs;
                    else
                        return PlayType.Direct_Ours;
                case RefBoxMessageType.KICKOFF_YELLOW:
                    if (team == Team.Blue)
                        return PlayType.KickOff_Theirs;
                    else
                        return PlayType.KickOff_Ours_Setup;
                case RefBoxMessageType.INDIRECT_YELLOW:
                    if (team == Team.Blue)
                        return PlayType.Indirect_Theirs;
                    else
                        return PlayType.Indirect_Ours;
                case RefBoxMessageType.DIRECT_YELLOW:
                    if (team == Team.Blue)
                        return PlayType.Direct_Theirs;
                    else
                        return PlayType.Direct_Ours;
                case RefBoxMessageType.PENALTY_BLUE:
                    // handle penalty
                    if (team == Team.Yellow)
                        return PlayType.PenaltyKick_Theirs;
                    else
                        return PlayType.PenaltyKick_Ours_Setup;
                case RefBoxMessageType.PENALTY_YELLOW:
                    // penalty kick
                    // handle penalty
                    if (team == Team.Blue)
                        return PlayType.PenaltyKick_Theirs;
                    else
                        return PlayType.PenaltyKick_Ours_Setup;
            }
            return PlayType.Halt;
        }

    }
}
