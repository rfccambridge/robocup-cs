using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Core
{
    public class Score
    {
        public int GoalsYellow;
        public int GoalsBlue;
        public Score()
        {
            GoalsBlue = 0;
            GoalsYellow = 0;
        }
        public void SetScore(Team team, int score)
        {
            if (team == Team.Blue)
                GoalsBlue = score;
            else if (team == Team.Yellow)
                GoalsYellow = score;
        }
    }
}
