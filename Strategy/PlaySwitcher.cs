using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class PlaySwitcher
    {
        Vector2 position;
        const double thresholdForBallMove = .03;

        bool ballIsMoved(Vector2 ballPosition)
        {
            return this.position.distanceSq(ballPosition) > thresholdForBallMove;
        }

        Team team;
        PlayType play;
        object lockObject;
        int max_robots;
        int goalie_id;

        // play behaviors
        NormalBehavior normalBehavior;
        WaitBehavior waitBehavior;
        KickInBehavior kickInBehavior;
        PenaltyKickBehavior penaltyKickBehavior;
        KickOffBehavior kickOffBehavior;

        public PlaySwitcher(Team our_team, int goalie_id)
        {
            team = our_team;
            lockObject = new object();
            this.max_robots = 12;
            this.goalie_id = goalie_id;
            new QueuedMessageHandler<RefboxStateMessage>(handle_refbox, lockObject);
            new QueuedMessageHandler<FieldVisionMessage>(handle_vision, lockObject);

            // initializing behavior components
            normalBehavior = new NormalBehavior(team, goalie_id);
            waitBehavior = new WaitBehavior(team, goalie_id, max_robots);
            kickInBehavior = new KickInBehavior(team, goalie_id);
            penaltyKickBehavior = new PenaltyKickBehavior(team, goalie_id);
            kickOffBehavior = new KickOffBehavior(team, goalie_id);

        }

        public void handle_refbox(RefboxStateMessage msg)
        {
            play = msg.PlayType;
            position = ServiceManager.getServiceManager().GetLastMessage<BallVisionMessage>().Ball.Position;
            Console.WriteLine("switched play to " + play.ToString());
        }
        
        public void handle_vision(FieldVisionMessage msg)
        {
            System.Threading.Thread.Sleep(50); // this is a complete hack, but things get unstable without it
            if((play == PlayType.Direct_Ours || play == PlayType.Direct_Theirs || play == PlayType.Indirect_Ours || play == PlayType.Indirect_Theirs || play == PlayType.KickOff_Ours || play == PlayType.KickOff_Theirs) && ballIsMoved(msg.Ball.Position))
            {
                play = PlayType.NormalPlay;
                Console.WriteLine("switched to normal play");
            }

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
