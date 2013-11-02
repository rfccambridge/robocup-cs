using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core;
using Messaging;

namespace RefBox
{
    class RefboxStateMessage : Message
    {
        public Team Team
        {
            get;
            private set;
        }
        public Score Score
        {
            get;
            private set;
        }
        PlayType PlayType
        {
            get;
            private set;
        }

        public RefboxStateMessage(Team team, Score score, char command)
        {
            PlayType = PlayType.Halt;

            this.Team = team;
            this.Score = score;

            switch (command)
            {
                case MulticastRefBoxListener.HALT:
                    // stop bots completely
                    PlayType = PlayType.Halt;
                    break;
                case MulticastRefBoxListener.START:
                    PlayType = PlayType.NormalPlay;
                    break;
                case MulticastRefBoxListener.CANCEL:
                case MulticastRefBoxListener.STOP:
                case MulticastRefBoxListener.TIMEOUT_BLUE:
                case MulticastRefBoxListener.TIMEOUT_YELLOW:
                    //go to stopped/waiting state
                    PlayType = PlayType.Stopped;
                    break;
                case MulticastRefBoxListener.TIMEOUT_END_BLUE:
                case MulticastRefBoxListener.TIMEOUT_END_YELLOW:
                case MulticastRefBoxListener.READY:
                    if (PlayType == PlayType.PenaltyKick_Ours_Setup)
                        PlayType = PlayType.PenaltyKick_Ours;
                    else if (PlayType == PlayType.KickOff_Ours_Setup)
                        PlayType = PlayType.KickOff_Ours;
                    else
                        Console.WriteLine("ready state not expected, previous command: " + PlayType);
                    break;
                case MulticastRefBoxListener.KICKOFF_BLUE:
                    if (ourTeam == Team.Yellow)
                        PlayType = PlayType.KickOff_Theirs;
                    else
                        PlayType = PlayType.KickOff_Ours_Setup;
                    break;
                case MulticastRefBoxListener.INDIRECT_BLUE:
                case MulticastRefBoxListener.DIRECT_BLUE:
                    if (ourTeam == Team.Yellow)
                        PlayType = PlayType.SetPlay_Theirs;
                    else
                        PlayType = PlayType.SetPlay_Ours;
                    break;
                case MulticastRefBoxListener.KICKOFF_YELLOW:
                    if (ourTeam == Team.Blue)
                        PlayType = PlayType.KickOff_Theirs;
                    else
                        PlayType = PlayType.KickOff_Ours_Setup;
                    break;
                case MulticastRefBoxListener.INDIRECT_YELLOW:
                case MulticastRefBoxListener.DIRECT_YELLOW:
                    if (ourTeam == Team.Blue)
                        PlayType = PlayType.SetPlay_Theirs;
                    else
                        PlayType = PlayType.SetPlay_Ours;
                    break;
                case MulticastRefBoxListener.PENALTY_BLUE:
                    // handle penalty
                    if (ourTeam == Team.Yellow)
                        PlayType = PlayType.PenaltyKick_Theirs;
                    else
                        PlayType = PlayType.PenaltyKick_Ours_Setup;
                    break;
                case MulticastRefBoxListener.PENALTY_YELLOW:
                    // penalty kick
                    // handle penalty
                    if (ourTeam == Team.Blue)
                        PlayType = PlayType.PenaltyKick_Theirs;
                    else
                        PlayType = PlayType.PenaltyKick_Ours_Setup;
                    break;
            }
        }

        public void LoadConstants()
        {
            throw new Exception("The method or operation is not implemented.");
        }

    }
}
