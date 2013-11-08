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

    public enum RefBoxMessageType
    {
        HALT = 'H',
        STOP = 'S',
        READY = ' ',
        START = 's',
        KICKOFF_YELLOW = 'k',
        PENALTY_YELLOW = 'p',
        DIRECT_YELLOW = 'f',
        INDIRECT_YELLOW = 'i',
        TIMEOUT_YELLOW = 't',
        TIMEOUT_END_YELLOW = 'z',

        KICKOFF_BLUE = 'K',
        PENALTY_BLUE = 'P',
        DIRECT_BLUE = 'F',
        INDIRECT_BLUE = 'I',
        TIMEOUT_BLUE = 'T',
        TIMEOUT_END_BLUE = 'Z',

        CANCEL = 'c'
    }
}
