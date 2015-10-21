using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Strategy
{
	
	public class AlexTest : IMessageHandler<FieldVisionMessage>
	{
        DefenseStrategy defenseStrategy;
		
		public AlexTest(Team myTeam, int goalie_id)
		{
			object lockObject = new object();
            var msngr = ServiceManager.getServiceManager();
            defenseStrategy = new DefenseStrategy(myTeam, goalie_id, DefenseStrategy.PlayType.Defense);
            msngr.RegisterListener(this.Queued(lockObject));
		}
		public void HandleMessage(FieldVisionMessage msg)
        {
            System.Threading.Thread.Sleep(100);
            defenseStrategy.DefenseCommand(msg, 1, false, .5);

		}
		
	}
}