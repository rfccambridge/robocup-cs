using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RFC.Core;
using RFC.Utilities;
using RFC.Geometry;
using RFC.Messaging;

namespace RFC.Vision
{
    /**
     * A simple short-duration jitter filter to blur our eyes from seeing 
     * the strange jumps that things make when reality apparently misplaces them
     * (the camera system is unreliable sometimes, okay?)
     * 
     * This filter maintains two lists: one of transient sightings and one of 
     * confirmed tracking items. Steps for maintenance are as follows: 
     * (TODO update pseudocode: consistent IDs / some simplification has since been performed)
     * 
     * Receive next AveragingPredictor state
     * for each robot in the state:
     *     calculate expected region from last official sighting
     *     if robot in expected region:
     *         update the robot with this sighting
     *     else: (handle transient sightings)
     *         if this has happened 3 times in a row:
     *             drop this robot from the official list
     *         else:
     *             update the official robot with extrapolated position
     *         
     *         if there is a transient for this robot: (update transient count)
     *             calculate expected region from last transient
     *             if sighting in expected region:
     *                 if this has triggered 3 times in a row:
     *                     promote this transient to the official list
     *         
     *         update or set transient anew
     * 
     * Do one run of this also to account for the ball. Remember the ball travels much faster.
     * 
     * At the end of this procedure we may go on to calculate the predicted 
     * next positions for each robot so as to have them around next time this is run.
     * 
     * This will serve to quash any jitters shorter than 3 frames, linearly 
     * interpolating positions in the meantime.
     * 
     * TODO: currently listens for AveragingPredictor messages; if possible, 
     * it might be better to attach this or build it onto AveragingPredictor in 
     * such a way that it updates synchronously (reducing message volume and handling)
     */
    public class JitterFilter
    {
        // dictionaries associating robot IDs with last observations
        // last observation per robot per team
        private Dictionary<Team, Dictionary<int, RobotInfo>> official;
        private Dictionary<Team, Dictionary<int, int>> officialCountup; // resets to zero each time a valid observation is obtained
        private Dictionary<Team, Dictionary<int, RobotInfo>> transient;
        private Dictionary<Team, Dictionary<int, int>> transientCountup; // counts up to transient promotion to official

        private ServiceManager messenger;

        private const int FRAMECOUNT = 5;

        public JitterFilter()
        {
            // Initialize empty lists of official/transient observations.
            // Both are empty: everything begins as a transient observation 
            // and is transferred to official observations as appropriate/confirmed.
            official = new Dictionary<Team, Dictionary<int, RobotInfo>>();
            transient = new Dictionary<Team, Dictionary<int, RobotInfo>>();
            officialCountup = new Dictionary<Team, Dictionary<int, int>>();
            transientCountup = new Dictionary<Team, Dictionary<int, int>>();

            // obtain message transmission equipment
            messenger = ServiceManager.getServiceManager();

        }

        private List<RobotInfo> getRobots(Team team)
        {
            if (official.ContainsKey(team))
            {
                return official[team].Values.ToList();
            }
            else
            {
                return new List<RobotInfo>();
            }
        }

        public void Update(FieldVisionMessage msg)
        {
            // TODO: UPDATE THE BALL


            // TODO also update the robots that weren't in this message
            // UPDATE THE ROBOTS
            List<RobotInfo> robots = msg.GetRobots();

            foreach (RobotInfo robot in robots)
            {
                // robot processing!

                // get the last official sighting for this robot by team/ID
                if (!official.ContainsKey(robot.Team))
                {
                    official.Add(robot.Team, new Dictionary<int, RobotInfo>());
                }

                RobotInfo lastOfficial;
                Vector2 projPos = new Vector2();
                double radius = .5; // radius from expected position at which robot can be found?
                double dt = (double)1 / 60;
                if (!official[robot.Team].ContainsKey(robot.ID))
                {
                    lastOfficial = null;
                }
                else
                {
                    lastOfficial = official[robot.Team][robot.ID];
                    // produce positions/velocities
                    // TODO is there any way for me to get observation times? for more precise calculation
                    projPos = lastOfficial.Position + lastOfficial.Velocity * dt; // project position
                    // is more technically 1/2 maximum acceleration times time difference squared
                }

                // check if counts exist
                if (!officialCountup.ContainsKey(robot.Team))
                {
                    officialCountup.Add(robot.Team, new Dictionary<int, int>());
                }
                if (!officialCountup[robot.Team].ContainsKey(robot.ID))
                {
                    officialCountup[robot.Team][robot.ID] = 0;
                }

                if (!transientCountup.ContainsKey(robot.Team))
                {
                    transientCountup.Add(robot.Team, new Dictionary<int, int>());
                }
                if (!transientCountup[robot.Team].ContainsKey(robot.ID))
                {
                    transientCountup[robot.Team][robot.ID] = 0;
                }

                if (lastOfficial != null && (robot.Position - projPos).magnitude() < radius) // robot not null AND close enough to official sighting
                {
                    official[robot.Team][robot.ID] = robot; // update official sighting with current robot info
                    officialCountup[robot.Team][robot.ID] = 0;
                    transientCountup[robot.Team][robot.ID] = 0;
                }
                else // missed sighting
                {
                    // stop tracking if too many missed sightings; otherwise update linearly
                    if (++officialCountup[robot.Team][robot.ID] > FRAMECOUNT)
                    {
                        // stopping tracking (NOTE: will produce effects when key not found handling implemented)
                        official[robot.Team].Remove(robot.ID);
                    }
                    else if (lastOfficial != null)
                    {
                        // updating "official" sighting with a simple linear projection
                        official[robot.Team][robot.ID] = new RobotInfo(lastOfficial.Position + lastOfficial.Velocity * dt,
                                                                       lastOfficial.Velocity, lastOfficial.AngularVelocity,
                                                                       lastOfficial.Orientation, lastOfficial.Team, lastOfficial.ID);
                    }

                    // handle this sighting as a transient sighting
                    if (!transient.ContainsKey(robot.Team))
                    {
                        transient.Add(robot.Team, new Dictionary<int, RobotInfo>());
                    }
                    if (transient[robot.Team].ContainsKey(robot.ID) && transient[robot.Team][robot.ID] != null)
                    {
                        // update existing transient

                        // is this sighting close enough to existing transient?
                        // transient sighting consistency check!
                        RobotInfo lastTransient = transient[robot.Team][robot.ID];
                        Vector2 projTransient = lastTransient.Position + lastTransient.Velocity * dt; // project position
                        if ((robot.Position - projTransient).magnitude() < radius) // close enough
                        {
                            transientCountup[robot.Team][robot.ID]++;

                            // transient promotion if appropriate
                            if (transientCountup[robot.Team][robot.ID] >= FRAMECOUNT)
                            {
                                // promote to official
                                // TODO: only if official tracking has already been discarded?
                                official[robot.Team][robot.ID] = robot;
                                transient[robot.Team].Remove(robot.ID);
                                transientCountup[robot.Team][robot.ID] = 0;
                            }
                        }
                        else
                        {
                            // reset countup
                            transientCountup[robot.Team][robot.ID] = 0;
                        }
                    }

                    // update transient sighting with current robot info
                    transient[robot.Team][robot.ID] = robot;
                }
            }


            BallInfo ball = msg.Ball;

            RobotVisionMessage robots_msg = new RobotVisionMessage(getRobots(Team.Blue), getRobots(Team.Yellow));
            FieldVisionMessage all_msg = new FieldVisionMessage(getRobots(Team.Blue), getRobots(Team.Yellow), ball);

            messenger.SendMessage<RobotVisionMessage>(robots_msg);
            messenger.SendMessage<FieldVisionMessage>(all_msg);
        }
    }
}
