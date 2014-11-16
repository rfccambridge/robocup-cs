using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.PathPlanning;

/*<summary>
Creates an instance of a play/tester that is equipped with the tester interface.
</summary>*/

namespace RFC.Strategy
{
    public class TesterFactory {
        public static Tester newTester(string play, Team team, int goalieNumber) {
            switch (play) {
                case "Offense":
                    return new OffenseTester(team, goalieNumber);
                case "Normal":
                    return new NormalBehavior(team, goalieNumber);
                case "Goalie":
                    return new GoalieTest(team, goalieNumber);
                case "Movement":
                    return new MovementTest(team);
                case "Setup":
                    return new SetupTest(team, goalieNumber);
                case "Timeout":
                    return new TimeoutBehavior(team, goalieNumber);
            //if (description == "Goalie")
                //return new GoalieTest(team, goalieNumber);
            //error handling
                default: throw new ArgumentException("No play '{0}'!\n", play);
            }
        }
     };
}
