using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    class PlaySwitcher
    {
        Team team;
        PlayType play;
        
        object lockObject;

        public PlaySwitcher(Team our_team)
        {
            team = our_team;
            lockObject = new object();
            new QueuedMessageHandler<RefboxStateMessage>(handle_refbox, lockObject);
            new QueuedMessageHandler<FieldVisionMessage>(handle_vision, lockObject);
        }

        public void handle_refbox(RefboxStateMessage msg)
        {
            play = msg.PlayType;
        }
        
        public void handle_vision(FieldVisionMessage msg)
        {
            switch(play)
            {
                case PlayType.NormalPlay:
                    NormalBehavior.Play(msg, team);
                    break;
                case PlayType.Halt:
                    WaitBehavior.Halt(msg, team);
                    break;
                case PlayType.Stopped:
                    WaitBehavior.Stop(msg, team);
                    break;
                case PlayType.Direct_Ours:
                    KickInBehavior.DirectOurs(msg, team);
                    break;
                case PlayType.Direct_Theirs:
                    KickInBehavior.DirectTheirs(msg, team);
                    break;
                case PlayType.Indirect_Ours:
                    KickInBehavior.IndirectOurs(msg, team);
                    break;
                case PlayType.Indirect_Theirs:
                    KickInBehavior.IndirectTheirs(msg, team);
                    break;
                case PlayType.PenaltyKick_Ours:
                    PenaltyKickBehavior.Ours(msg, team);
                    break;
                case PlayType.PenaltyKick_Ours_Setup:
                    PenaltyKickBehavior.OursSetup(msg, team);
                    break;
                case PlayType.PenaltyKick_Theirs:
                    PenaltyKickBehavior.Theirs(msg, team);
                    break;
                case PlayType.KickOff_Ours:
                    KickOffBehavior.Ours(msg, team);
                    break;
                case PlayType.KickOff_Ours_Setup:
                    KickOffBehavior.OursSetup(msg, team);
                    break;
                case PlayType.Kickoff_Theirs_Setup:
                    KickOffBehavior.TheirsSetup(msg, team);
                    break;
                case PlayType.KickOff_Theirs:
                    KickOffBehavior.Theirs(msg, team);
                    break;
            }
        }
    }
}
