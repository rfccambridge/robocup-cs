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
		AssessThreats assessThreats;
		
		public AlexTest()
		{
			object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
			// Insert code required on object creation below this point.
			assessThreats = new AssessThreats(Team.Yellow,1);
			
		}
		public void Handle(FieldVisionMessage msg)
        {
			assessThreats.getThreats(msg);
		}
		
	}
}