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

namespace RFC.Strategy
{
    public class TesterFactory {
        public static Tester newTester(string description, Team team, int goalieNumber) {
            if (description == "Offense")
                return new OffenseTester(team, goalieNumber);
            //if (description == "Goalie")
                //return new GoalieTest(team, goalieNumber);
            //error handling
            throw new ArgumentException("No tester called %s!\n", description);
            return null;
        }
     };
}
