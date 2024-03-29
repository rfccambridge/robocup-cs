﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;
using RFC.Logging;

namespace RFC.Strategy
{
    public class PlaySwitcher : IMessageHandler<RefboxStateMessage>, IMessageHandler<FieldVisionMessage>
    {
        Point2 position;
        const double thresholdForBallMove = .03;

        bool ballIsMoved(Point2 ballPosition)
        {
            return this.position.distanceSq(ballPosition) > thresholdForBallMove;
        }

        Team team;
        FieldDrawer.FieldDrawer fd;
        PlayType play;
        object lockObject;
        int max_robots;
        int goalie_id;
        int kick_id;

        // play behaviors
        NormalBehavior normalBehavior;
        WaitBehavior waitBehavior;
        KickInBehavior kickInBehavior;
        PenaltyKickBehavior penaltyKickBehavior;
        KickOffBehavior kickOffBehavior;
        ServiceManager msngr;
        MatchRecorder recorder;

        public PlaySwitcher(Team our_team, int goalie_id, FieldDrawer.FieldDrawer fd)
        {
            team = our_team;
            this.fd = fd;
            lockObject = new object();
            this.max_robots = 12;
            this.goalie_id = goalie_id;

            this.msngr = ServiceManager.getServiceManager();
            this.recorder = new MatchRecorder();

            // initializing behavior components
            normalBehavior = new NormalBehavior(team, goalie_id);
            waitBehavior = new WaitBehavior(team, goalie_id, max_robots);
            kickInBehavior = new KickInBehavior(team, goalie_id);
            penaltyKickBehavior = new PenaltyKickBehavior(team, goalie_id);
            kickOffBehavior = new KickOffBehavior(team, goalie_id);
            this.play = PlayType.Stopped;
            this.kick_id = 2;

            msngr.RegisterListener(this.Queued<RefboxStateMessage>(lockObject));
            msngr.RegisterListener(this.Queued<FieldVisionMessage>(lockObject));
        }

        public void HandleMessage(RefboxStateMessage msg)
        {
            play = msg.PlayType;
            position = ServiceManager.getServiceManager().GetLastMessage<BallVisionMessage>().Ball.Position;
        }
        
        public void HandleMessage(FieldVisionMessage msg)
        {
            //System.Threading.Thread.Sleep(100); // this is a complete hack, but things get unstable without it
            if((play == PlayType.Direct_Ours || play == PlayType.Direct_Theirs || play == PlayType.Indirect_Ours || play == PlayType.Indirect_Theirs || play == PlayType.KickOff_Ours || play == PlayType.KickOff_Theirs) && ballIsMoved(msg.Ball.Position))
            {
                play = PlayType.NormalPlay;
                Console.WriteLine("switched to normal");
            }
            fd.Team = team;
            fd.PlayType = play;
            NormalBehavior.State currentState = NormalBehavior.State.Unknown;
            switch (play)
            {
                case PlayType.NormalPlay:
                    currentState = normalBehavior.Play(msg);
                    break;
                case PlayType.Halt:
                    waitBehavior.Halt(msg);
                    break;
                case PlayType.Stopped:
                    waitBehavior.Stop(msg);
                    break;
                case PlayType.Direct_Ours:
                    KickMessage kick = new KickMessage(msg.GetRobot(team,kick_id), Constants.FieldPts.THEIR_GOAL);
                    msngr.SendMessage(kick);
                    break;
                case PlayType.Direct_Theirs:
                    kickInBehavior.DirectTheirs(msg);
                    break;
                case PlayType.Indirect_Ours:
                    KickMessage kick2 = new KickMessage(msg.GetRobot(team,kick_id), Constants.FieldPts.THEIR_GOAL);
                    msngr.SendMessage(kick2);
                    break;
                case PlayType.Indirect_Theirs:
                    kickInBehavior.IndirectTheirs(msg);
                    break;
                case PlayType.PenaltyKick_Ours:
                    KickMessage kick3 = new KickMessage(msg.GetRobot(team,kick_id), Constants.FieldPts.THEIR_GOAL);
                    msngr.SendMessage(kick3);
                    break;
                case PlayType.PenaltyKick_Ours_Setup:
                    penaltyKickBehavior.OursSetup(msg);
                    break;
                case PlayType.PenaltyKick_Theirs:
                    penaltyKickBehavior.Theirs(msg);
                    break;
                case PlayType.KickOff_Ours:
                    KickMessage kick4 = new KickMessage(msg.GetRobot(team,kick_id), Constants.FieldPts.THEIR_GOAL);
                    msngr.SendMessage(kick4);
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
            string playName;
            switch (currentState)
            {
                case NormalBehavior.State.Offense:
                    playName = "Offense";
                    break;
                case NormalBehavior.State.Midfield:
                    playName = "Midfield";
                    break;
                case NormalBehavior.State.Defense:
                    playName = "Defense";
                    break;
                default:
                    playName = "Special";
                    break;
            }
            fd.UpdatePlayName(team, 0, playName);
            recorder.Handle(msg);
        }
    }
}
