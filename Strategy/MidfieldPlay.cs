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
    class MidfieldPlay
    {
        Team team;
        int goalie_id;
        ServiceManager msngr;
        Goalie goalieBehave;
        DefenseStrategy positionHelper;

        public MidfieldPlay(Team t, int g)
        {
            team = t;
            goalie_id = g;
            positionHelper = new DefenseStrategy(t,g, DefenseStrategy.PlayType.MidField);
            msngr = ServiceManager.getServiceManager();
        }

        public void doMidfield(FieldVisionMessage message)
        {
            int count = message.GetRobots(team).Count();
            
            if (message.Ball.Position.X>0)
            {
                Console.WriteLine("Midfield: wall defense");
                if (count - 1 >= 3)
                {
                    positionHelper.DefenseCommand(message, 3, true);
                }
                else
                {
                    positionHelper.DefenseCommand(message, count - 1, true);
                }

            }
            else
            {
                Console.WriteLine("Midfield: clearing");
                positionHelper.DefenseCommand(message, 1, true);
            }    
        }
    }
}

