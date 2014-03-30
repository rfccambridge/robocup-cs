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
        int max_robots;

        // play behaviors
        NormalBehavior normalBehavior;
        WaitBehavior waitBehavior;
        KickInBehavior kickInBehavior;
        PenaltyKickBehavior penaltyKickBehavior;
        KickOffBehavior kickOffBehavior;


        public PlaySwitcher(Team our_team, int max_robots)
        {
            team = our_team;
            lockObject = new object();
            this.max_robots = max_robots;
            new QueuedMessageHandler<RefboxStateMessage>(handle_refbox, lockObject);
            new QueuedMessageHandler<FieldVisionMessage>(handle_vision, lockObject);

            // initializing behavior components
            normalBehavior = new NormalBehavior(team);
            waitBehavior = new WaitBehavior(team,max_robots);
            kickInBehavior = new KickInBehavior(team);
            penaltyKickBehavior = new PenaltyKickBehavior(team);
            kickOffBehavior = new KickOffBehavior(team);

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
                    normalBehavior.Play(msg);
                    break;
                case PlayType.Halt:
                    waitBehavior.Halt(msg);
                    break;
                case PlayType.Stopped:
                    waitBehavior.Stop(msg);
                    break;
                case PlayType.Direct_Ours:
                    kickInBehavior.DirectOurs(msg);
                    break;
                case PlayType.Direct_Theirs:
                    kickInBehavior.DirectTheirs(msg);
                    break;
                case PlayType.Indirect_Ours:
                    kickInBehavior.IndirectOurs(msg);
                    break;
                case PlayType.Indirect_Theirs:
                    kickInBehavior.IndirectTheirs(msg);
                    break;
                case PlayType.PenaltyKick_Ours:
                    penaltyKickBehavior.Ours(msg);
                    break;
                case PlayType.PenaltyKick_Ours_Setup:
                    penaltyKickBehavior.OursSetup(msg);
                    break;
                case PlayType.PenaltyKick_Theirs:
                    penaltyKickBehavior.Theirs(msg);
                    break;
                case PlayType.KickOff_Ours:
                    kickOffBehavior.Ours(msg);
                    break;
                case PlayType.KickOff_Ours_Setup:
                    kickOffBehavior.OursSetup(msg);
                    break;
                case PlayType.Kickoff_Theirs_Setup:
                    kickOffBehavior.TheirsSetup(msg);
                    break;
                case PlayType.KickOff_Theirs:
                    kickOffBehavior.Theirs(msg);
                    break;
            }
        }
    }
}
