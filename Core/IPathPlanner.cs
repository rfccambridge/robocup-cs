using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Core
{
    /// <summary>
    /// Interface for algorithms that, given a path, return wheel speeds to
    /// drive around that path
    /// </summary>
    public interface IPathDriver
    {
        WheelSpeeds followPath(RobotPath path, IPredictor predictor);
        void ReloadConstants();
    }
}
