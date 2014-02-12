using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;

namespace RFC.Messaging
{
    public class RefboxStateMessage : Message
    {
        public Score Score
        {
            get;
            private set;
        }
        public PlayType PlayType { get; private set; }

        public RefboxStateMessage(Score score, PlayType playType)
        {
            this.Score = score;
            this.PlayType = playType;
        }

        public void LoadConstants()
        {
            throw new Exception("The method or operation is not implemented.");
        }

    }
}
