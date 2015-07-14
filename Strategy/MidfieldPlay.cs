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
        DefenseStrategy defense;
        OffenseStrategy offense;

        public MidfieldPlay(Team t, int g)
        {
            team = t;
            goalie_id = g;
            defense = new DefenseStrategy(t,g, DefenseStrategy.PlayType.MidField);
            offense = new OffenseStrategy(t, g);
            msngr = ServiceManager.getServiceManager();
        }

        public void doMidfield(FieldVisionMessage message)
        {            
            if (message.Ball.Velocity.X >= 2.0)
            {
                Console.WriteLine("Midfield: offensive");
                offense.Handle(message);
            }
            else
            {
                Console.WriteLine("Midfield: defensive");
                defense.DefenseCommand(message, 1, true);
            }    
        }
    }
}

