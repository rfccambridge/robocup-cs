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
                    NormalBehavior.Play(msg);
                    break;
                case PlayType.Halt:
                    WaitBehavior.Halt(msg);
                    break;
                case PlayType.Stopped:
                    WaitBehavior.Stop(msg);
                    break;
                case PlayType.Direct_Ours:
                    KickInBehavior.DirectOurs(msg);
                    break;
                case PlayType.Direct_Theirs:
                    KickInBehavior.DirectTheirs(msg);
                    break;
                case PlayType.Indirect_Ours:
                    KickInBehavior.IndirectOurs(msg);
                    break;
                case PlayType.Indirect_Theirs:
                    KickInBehavior.IndirectTheirs(msg);
                    break;
                case PlayType.PenaltyKick_Ours:
                    PenaltyKickBehavior.Ours(msg);
                    break;
                case PlayType.PenaltyKick_Ours_Setup:
                    PenaltyKickBehavior.OursSetup(msg);
                    break;
                case PlayType.PenaltyKick_Theirs:
                    PenaltyKickBehavior.Theirs(msg);
                    break;
                case PlayType.KickOff_Ours:
                    KickOffBehavior.Ours(msg);
                    break;
                case PlayType.KickOff_Ours_Setup:
                    KickOffBehavior.OursSetup(msg);
                    break;
                case PlayType.Kickoff_Theirs_Setup:
                    KickOffBehavior.TheirsSetup(msg);
                    break;
                case PlayType.KickOff_Theirs:
                    KickOffBehavior.Theirs(msg);
                    break;
            }
        }
    }
}
