using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RFC.Core;
using RFC.Utilities;
using RFC.Geometry;
using RFC.Messaging;

namespace Vision
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
     *     if observed position (this robot sighting) near reasonable place:
     *         update the robot with this sighting
     *     else: (handle transient sightings)
     *         if this has happened > 2 times in a row:
     *             stop tracking this official robot
     *         else:
     *             update the official robot with pos/vel position
     *         
     *         if a transient compares favorably with this sighting:
     *             update the transient sighting
     *             if this has happened > 2 times in a row:
     *                 if transient does not match official robot:
     *                     move this transient to the official list
     *         else: (new transient)
     *             add a new transient sighting
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
    class JitterFilter
    {
        // dictionaries associating robot IDs with last observations
        // last observation per robot per team
        private Dictionary<Team, Dictionary<int, RobotInfo>> official;
        private Dictionary<Team, Dictionary<int, int>> officialCountup; // resets to zero each time a valid observation is obtained
        private Dictionary<Team, Dictionary<int, RobotInfo>> transient;
        private Dictionary<Team, Dictionary<int, int>> transientCountup; // counts up to transient promotion to official

        private ServiceManager messenger;

        public JitterFilter()
        {
            // Initialize empty lists of official/transient observations.
            // Both are empty: everything begins as a transient observation 
            // and is transferred to official observations as appropriate/confirmed.
            official = new Dictionary<Team, Dictionary<int, RobotInfo>>();
            transient = new Dictionary<Team, Dictionary<int, RobotInfo>>();

            // obtain message transmission equipment
            messenger = ServiceManager.getServiceManager();

            new QueuedMessageHandler<FieldVisionMessage>(Update, new Object());
        }

        public void Update(FieldVisionMessage msg)
        {
            // TODO: UPDATE THE BALL



            // UPDATE THE ROBOTS
            List<RobotInfo> robots = msg.GetRobots();

            foreach (RobotInfo robot in robots)
            {
                // robot processing!

                // get the last official sighting for this robot by team/ID
                RobotInfo lastOfficial = official[robot.Team][robot.ID];
                // TODO handle key not found cases; currently will kill code

                // produce positions/velocities
                // TODO is there any way for me to get observation times? for more precise calculation
                double dt = (double)1/60;
                Vector2 projPos = lastOfficial.Position + lastOfficial.Velocity * dt; // project position
                double radius = 2; // radius from expected position at which robot can be found?
                                   // is more technically 1/2 maximum acceleration times time difference squared

                if ((robot.Position - projPos).magnitude() < radius) // close enough to official sighting
                {
                    official[robot.Team][robot.ID] = robot; // update official sighting with current robot info
                    officialCountup[robot.Team][robot.ID] = 0;
                }
                else
                {
                    // stop tracking if too many missed sightings; otherwise update linearly
                    if (++officialCountup[robot.Team][robot.ID] > 3)
                    {
                        // stopping tracking (NOTE: will produce effects when key not found handling implemented)
                        official[robot.Team].Remove(robot.ID);
                    }
                    else
                    {
                        // updating "official" sighting with a simple linear projection
                        official[robot.Team][robot.ID] = new RobotInfo(lastOfficial.Position + lastOfficial.Velocity * dt,
                                                                       lastOfficial.Velocity, lastOfficial.AngularVelocity,
                                                                       lastOfficial.Orientation, lastOfficial.ID);
                    }

                    // handle this sighting as a transient sighting
                    if (transient[robot.Team][robot.ID] != null)
                    {
                        // update existing transient

                        // transient promotion if appropriate
                        if (transientCountup[robot.Team][robot.ID] >= 3)
                        {
                            // promote to official
                            // TODO: only if official tracking has already been discarded?
                            official[robot.Team][robot.ID] = robot;
                        }
                        else
                        {
                            // is this sighting close enough to existing transient?
                            // transient sighting consistency check!
                            RobotInfo lastTransient = transient[robot.Team][robot.ID];
                            Vector2 projTransient = lastTransient.Position + lastTransient.Velocity * dt; // project position
                            if ((robot.Position - projTransient).magnitude() < radius) // close enough
                            {
                                
                                transientCountup[robot.Team][robot.ID]++;
                            }
                            else
                            {
                                // reset countup
                                transientCountup[robot.Team][robot.ID] = 0;
                            }
                        }
                    }

                    // update transient sighting with current robot info
                    transient[robot.Team][robot.ID] = robot;
                }
            }
        }
    }
}
