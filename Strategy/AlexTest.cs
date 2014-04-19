using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Strategy
{
	
	public class AlexTest
	{
        DefenseStrategy defenseStrategy;
		
		public AlexTest(Team myTeam, int goalie_id)
		{
			object lockObject = new object();
            defenseStrategy = new DefenseStrategy(myTeam, goalie_id);
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
			
			
		}
		public void Handle(FieldVisionMessage msg)
        {
            defenseStrategy.DefenseCommand(msg);

		}
		
	}
}