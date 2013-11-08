using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;

namespace RFC.RefBox
{
    public class RefboxStateMessage : Message
    {
        public Score Score
        {
            get;
            private set;
        }
        readonly char command;
        readonly char lastCommand;
        PlayType GetPlayType(Team team) {
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
                case RefBoxMessageType.DIRECT_BLUE:
                    if (team == Team.Yellow)
                        return PlayType.SetPlay_Theirs;
                    else
                        return PlayType.SetPlay_Ours;
                case RefBoxMessageType.KICKOFF_YELLOW:
                    if (team == Team.Blue)
                        return PlayType.KickOff_Theirs;
                    else
                        return PlayType.KickOff_Ours_Setup;
                case RefBoxMessageType.INDIRECT_YELLOW:
                case RefBoxMessageType.DIRECT_YELLOW:
                    if (team == Team.Blue)
                        return PlayType.SetPlay_Theirs;
                    else
                        return PlayType.SetPlay_Ours;
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

        public RefboxStateMessage(Score score, char command, char previousCommand)
        {
            this.Score = score;
            this.command = command;
            this.lastCommand = previousCommand;
        }

        public void LoadConstants()
        {
            throw new Exception("The method or operation is not implemented.");
        }

    }
}
