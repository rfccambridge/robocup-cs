using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Core
{
    public enum Team
    {
        Yellow,
        Blue
    }

    public enum PlayType
    {
        NormalPlay,
        Halt,
        Stopped,
        SetPlay_Ours,
        SetPlay_Theirs,
        PenaltyKick_Ours,
        PenaltyKick_Ours_Setup,
        PenaltyKick_Theirs,
        KickOff_Ours,
        KickOff_Ours_Setup,
        Kickoff_Theirs_Setup,
        KickOff_Theirs
    }
}
