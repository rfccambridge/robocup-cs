using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;
using RFC.Utilities;
using System.ComponentModel;
using System.Windows.Forms;

namespace RFC.FieldDrawer
{
    public enum ArrowType
    {
        Destination,
        Waypoint
    }

    public class WaypointInfo
    {
        public Object Object;
        public Color Color;
        public WaypointInfo(Object obj, Color color)
        {
            Object = obj;
            Color = color;
        }
    }

    public class WaypointMovedInfo : WaypointInfo
    {
        public Point2 NewLocation;
        public double NewOrientation;
        public WaypointMovedInfo(Object obj, Color color, Point2 newLocation, double newOrientation)
            : base(obj, color)
        {
            NewLocation = newLocation;
            NewOrientation = newOrientation;
        }
    }

    public partial class FieldDrawer : IMessageHandler<RobotVisionMessage>,
                            IMessageHandler<BallVisionMessage>,
                            IMessageHandler<VisualDebugMessage>,
                            IMessageHandler<VisualDebugMessage<Lattice<Color>>>,
                            IMessageHandler<RobotPathMessage>,
                            IMessageHandler<RobotDestinationMessage>,
                            IMessageHandler<RefboxStateMessage>
    {
        public bool debug_motion = true;

        public event EventHandler<EventArgs<WaypointInfo>> WaypointAdded;
        public event EventHandler<EventArgs<WaypointInfo>> WaypointRemoved;
        public event EventHandler<EventArgs<WaypointMovedInfo>> WaypointMoved;

        public event EventHandler<EventArgs> StateUpdated;

        public class RobotDrawingInfo
        {
            public RobotInfo RobotInfo;
            public Dictionary<ArrowType, Arrow> Arrows = new Dictionary<ArrowType, Arrow>();
            public string PlayName;
            public string AdditionalInfo;
            public RobotPath Path;

            public RobotDrawingInfo(RobotInfo robotInfo)
            {
                RobotInfo = robotInfo;
            }
        }

        public class Marker
        {
            public const double SIZE = 0.02;

            public Point2 Location;
            public Color Color;
            public object Object;

            public bool DoDrawOrientation;
            public double Orientation;

            public Marker(Point2 loc, Color col, Object obj)
            {
                Location = loc;
                Color = col;
                Object = obj;

                DoDrawOrientation = false;
                Orientation = 0;
            }

            public Marker(Point2 loc, double orientation, Color col, Object obj)
            {
                Location = loc;
                Color = col;
                Object = obj;

                DoDrawOrientation = true;
                Orientation = orientation;
            }

            public bool contains(Point2 pt)
            {
                return !(pt.X < Location.X - Marker.SIZE || pt.X > Location.X + Marker.SIZE
                    || pt.Y < Location.Y - Marker.SIZE || pt.Y > Location.Y + Marker.SIZE);
            }
        }

        public class Arrow
        {
            public Point2 ToPoint;
            public Color Color;

            public Arrow(Point2 toPoint, Color color)
            {
                ToPoint = toPoint;
                Color = color;
            }
        }

        public class State
        {
            public Dictionary<Team, Dictionary<int, RobotDrawingInfo>> Robots = new Dictionary<Team, Dictionary<int, RobotDrawingInfo>>();
            public BallInfo Ball;
            public Dictionary<int, Marker> Markers = new Dictionary<int, Marker>();

            public int NextRobotHandle = 0;
            public int NextMarkerHandle = 0;

            public State()
            {
                Robots.Add(Core.Team.Yellow, new Dictionary<int, RobotDrawingInfo>());
                Robots.Add(Core.Team.Blue, new Dictionary<int, RobotDrawingInfo>());
            }

            public void Clear()
            {
                Robots[Core.Team.Yellow].Clear();
                Robots[Core.Team.Blue].Clear();
                Ball = null;
                Markers.Clear();

                NextRobotHandle = 0;
                NextMarkerHandle = 0;
            }
        }
        
        State _bufferedState = new State();
        State _state = new State();
        object _stateLock = new object();
        bool _collectingState = true;
        object _collectingStateLock = new object();
        bool _robotsAndBallUpdated = false;

        //Marker drag and drop
        Marker _draggedMarker; //The marker currently being dragged
        bool _movedDraggedMarker; //Have we ever moved this marker since the start of the drag?

        ServiceManager msngr;


        static private Constants.FieldType C = Constants.Field;

        public FieldDrawer() : base(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8))
        {
            AllowDrop = true;
            BackColor = System.Drawing.Color.Black;
            TabIndex = 0;
            VSync = false;
            
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener(this.Queued<RobotVisionMessage>(new object()));
            msngr.RegisterListener(this.Queued<BallVisionMessage>(new object()));
            msngr.RegisterListener(this.Queued<RobotPathMessage>(new object()));
            msngr.RegisterListener(this.Queued<RobotDestinationMessage>(new object()));
            msngr.RegisterListener(this.Queued<RefboxStateMessage>(new object()));
            msngr.RegisterListener(this.Queued<VisualDebugMessage>(new object()));
            msngr.RegisterListener(this.Queued<VisualDebugMessage<Lattice<Color>>>(new object()));

            _robotInfos = new Dictionary<Team, Dictionary<int, string>>();
            _robotInfos.Add(Core.Team.Blue, new Dictionary<int, string>());
            _robotInfos.Add(Core.Team.Yellow, new Dictionary<int, string>());
        }

        public void HandleMessage(RobotVisionMessage message)
        {
            lock (_stateLock)
            {
                List<RobotInfo> robots = message.GetRobots();
                foreach (RobotInfo robot in robots)
                    // Note: robots with duplicate ID's are ignored
                    if (_bufferedState.Robots[robot.Team].ContainsKey(robot.ID))
                        _bufferedState.Robots[robot.Team][robot.ID].RobotInfo = robot;
                    else
                        _bufferedState.Robots[robot.Team].Add(robot.ID, new RobotDrawingInfo(robot));

                _robotsAndBallUpdated = true;
            }

            UpdateState();
        }

        public void HandleMessage(BallVisionMessage message)
        {
            lock (_stateLock)
            {
                _bufferedState.Ball = message.Ball;
            }
        }

        public void HandleMessage(VisualDebugMessage msg)
        {
            if (msg.clear)
            {
                this.ClearMarkers();
            }
            else
            {
                this.AddMarker(msg.position, msg.c);
            }
        }

        public void HandleMessage(RobotPathMessage msg)
        {

            if (debug_motion)
            {
                lock (_stateLock)
                {
                    DrawPath(msg.Path);
                }
            }

        }

        public void HandleMessage(RobotDestinationMessage msg)
        {
            if (debug_motion)
            {
                lock (_stateLock)
                {
                    DrawArrow(msg.Destination.Team, msg.Destination.ID, ArrowType.Destination, msg.Destination.Position);
                }
            }
        }

        public void HandleMessage(RefboxStateMessage msg)
        {
            RefBoxCmd = msg.PlayType.ToString();
        }

        Lattice<Color> last_lattice = null;
        public void HandleMessage(VisualDebugMessage<Lattice<Color>> message)
        {
            lock (_stateLock)
            {
                last_lattice = message.value;
            }
        }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Point2 pt = FieldCoordsOf(e.Location);
            _draggedMarker = null;
            _movedDraggedMarker = false;
            lock (_stateLock)
            {
                foreach (Marker marker in _state.Markers.Values)
                    if (marker.contains(pt))
                        _draggedMarker = marker;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs loc)
        {
            if (_draggedMarker != null)
            {
                if (loc.X < 0 || loc.X > _glControlWidth || loc.Y < 0 || loc.Y > _glControlHeight)
                {
                    WaypointRemoved?.Invoke(this, new EventArgs<WaypointInfo>(
                                                new WaypointInfo(_draggedMarker.Object, _draggedMarker.Color)));
                }

                if (!_movedDraggedMarker)
                {
                    _draggedMarker.Orientation += Math.PI / 8;
                    WaypointMoved?.Invoke(this, new EventArgs<WaypointMovedInfo>(
                                                new WaypointMovedInfo(_draggedMarker.Object, _draggedMarker.Color, _draggedMarker.Location, _draggedMarker.Orientation)));
                }
                _draggedMarker = null;
                _movedDraggedMarker = false;
            }
            base.OnMouseUp(loc);
        }

        protected override void OnMouseMove(MouseEventArgs loc)
        {
            if (_draggedMarker != null)
            {
                _movedDraggedMarker = true;
                Point2 pt = FieldCoordsOf(loc.Location);
                lock (_collectingStateLock)
                {
                    _draggedMarker.Location = pt;
                    WaypointMoved?.Invoke(this, new EventArgs<WaypointMovedInfo>(
                                                new WaypointMovedInfo(_draggedMarker.Object, _draggedMarker.Color, _draggedMarker.Location, _draggedMarker.Orientation)));
                }
            }
            StateUpdated?.Invoke(this, null);

            base.OnMouseMove(loc);
        }


        protected override void OnDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Color)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            Point location = PointToClient(new Point(e.X, e.Y));

            // TODO: Add ability to set orientation (and id?)
            object data = e.Data.GetData(typeof(Color));
            if (data is Color)
            {
                EventArgs<WaypointInfo> eventArgs = new EventArgs<WaypointInfo>(
                    new WaypointInfo(
                        new RobotInfo(FieldCoordsOf(location), 0, Team.Yellow, -1),
                        (Color) data
                    )
                );
                WaypointAdded?.Invoke(this, eventArgs);
            }
            base.OnDragDrop(e);
        }

        /*public void BeginCollectState()
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    _collectingState = true;
                else
                    throw new ApplicationException("Already collecting state!");
            }
            _bufferedState.Clear();
            _robotsAndBallUpdated = false;
        }
        public void EndCollectState()
        {
            lock (_collectingStateLock)
            {
                if (_collectingState)
                    _collectingState = false;
                else
                    throw new ApplicationException("Never began collecting state!");
            }
        }*/

        public void UpdateState()
        {
            lock (_stateLock)
            {
                // Apply the modifications stored in the buffer

                if (_robotsAndBallUpdated)
                {
                    _state.Ball = _bufferedState.Ball;
                    foreach (Team team in Enum.GetValues(typeof(Team)))
                    {
                        _state.Robots[team].Clear();
                        foreach (KeyValuePair<int, RobotDrawingInfo> pair in _bufferedState.Robots[team])
                            _state.Robots[team].Add(pair.Key, pair.Value);
                    }
                }

                foreach (KeyValuePair<int, Marker> pair in _bufferedState.Markers)
                {
                    if (pair.Value != null)
                    {
                        if (_state.Markers.ContainsKey(pair.Key))
                            _state.Markers[pair.Key] = pair.Value;
                        else
                            _state.Markers.Add(pair.Key, pair.Value);
                    }
                    else
                    {
                        if (_state.Markers.ContainsKey(pair.Key))
                            _state.Markers.Remove(pair.Key);
                    }
                }
            }

            StateUpdated?.Invoke(this, null);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!controlLoaded) return;

            MakeCurrent();

            lock (_stateLock)
            {
                updateProjection();
                drawField();
                if (last_lattice != null)
                    drawLattice(last_lattice);
                // TODO actually fix instead of ignore errors--probably fine since this is only used for debugging offense mapping
                try
                {
                    foreach (Marker marker in _state.Markers.Values)
                    {
                        drawMarker(marker);

                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Markers already changed--cannot draw.");
                }

                foreach (Team team in Enum.GetValues(typeof(Team)))
                    foreach (RobotDrawingInfo robot in _state.Robots[team].Values)
                        drawRobot(robot);

                if (_state.Ball != null)
                    drawBall(_state.Ball);
            }

            SwapBuffers();
        }

        #region Data Binding
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private Team? _team = null;
        public Team? Team
        {
            get { return _team; }
            set
            {
                var last = _team;
                _team = value;
                if (value != last)
                {
                    OnPropertyChanged(nameof(Team));
                    OnPropertyChanged(nameof(TeamName));
                    OnPropertyChanged(nameof(TeamColor));
                }
            }
        }
        public string TeamName => Team?.ToString() ?? "";
        public Color TeamColor => Team == Core.Team.Blue ? Color.Blue : Color.Yellow;


        private string _refBoxCmd;
        public string RefBoxCmd
        {
            get { return _refBoxCmd; }
            set
            {
                var last = _refBoxCmd;
                _refBoxCmd = value;
                if (value != last)
                    OnPropertyChanged(nameof(RefBoxCmd));
            }
        }


        private PlayType? _playType;
        public PlayType? PlayType
        {
            get { return _playType; }
            set
            {
                var last = _playType;
                _playType = value;
                if (value != last)
                    OnPropertyChanged(nameof(PlayType));
            }
        }


        private string _playName = null;
        public string PlayName
        {
            get { return _playName; }
            set
            {
                var last = _playName;
                _playName = value;
                if (value != last)
                    OnPropertyChanged(nameof(PlayName));
            }
        }
        #endregion

        public void UpdatePlayName(Team team, int robotID, string name)
        {
            if (team == _team)
                PlayName = name;
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    return;
                if (_bufferedState.Robots[team].ContainsKey(robotID))
                    _bufferedState.Robots[team][robotID].PlayName = name + (_robotInfos[team].ContainsKey(robotID) ? _robotInfos[team][robotID] : "");
            }
        }

        Dictionary<Team, Dictionary<int, string>> _robotInfos;
        public void AddRobotInfo(Team team, int robotID, string info)
        {
            _robotInfos[team][robotID] = info;
        }

        public void UpdateRobotsAndBall(List<RobotInfo> robots, BallInfo ball)
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    return;

                _robotsAndBallUpdated = true;
                _bufferedState.Ball = ball;

                foreach (Team team in Enum.GetValues(typeof(Team)))
                    _bufferedState.Robots[team].Clear();
                foreach (RobotInfo robot in robots)
                    // Note: robots with duplicate ID's are ignored
                    if (!_bufferedState.Robots[robot.Team].ContainsKey(robot.ID))
                        _bufferedState.Robots[robot.Team].Add(robot.ID, new RobotDrawingInfo(robot));
            }
        }

        public void ClearMarkers()
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    throw new ApplicationException("Not collecting state!");
                _bufferedState.Markers.Clear();
                _state.Markers.Clear();
            }
        }

        public int AddMarker(Point2 location, Color color, Object obj = null)
        {
            lock (_stateLock)
            {
                lock (_collectingStateLock)
                {
                    if (!_collectingState)
                        throw new ApplicationException("Not collecting state!");
                    int handle = _bufferedState.NextMarkerHandle;
                    _bufferedState.Markers.Add(handle, new Marker(location, color, obj));
                    unchecked { _bufferedState.NextMarkerHandle++; }
                    return handle;
                }
            }

        }
        public void RemoveMarker(int handle)
        {
            lock (_collectingStateLock)
            {
                if (!_collectingState)
                    throw new ApplicationException("Not collecting state!");
                if (!_state.Markers.ContainsKey(handle))
                    throw new ApplicationException("Trying to remove an object that doesn't exist!");
                if (_bufferedState.Markers.ContainsKey(handle))
                    _bufferedState.Markers[handle] = null;
                else
                    _bufferedState.Markers.Add(handle, null);
            }
        }
        public void UpdateMarker(int handle, Vector2 location, Color color)
        {
            throw new NotImplementedException();
        }
        public Color GetMarkerColor(int handle)
        {
            lock (_stateLock)
            {
                return _state.Markers[handle].Color;
            }
        }

        public void DrawArrow(Team team, int robotID, ArrowType type, Point2 toPoint)
        {
            Color color = Color.Black;

            switch (type)
            {
                case ArrowType.Destination: color = Color.Red; break;
                case ArrowType.Waypoint: color = Color.LightPink; break;
            }

            lock (_collectingStateLock)
            {
                if (_bufferedState.Robots[team].ContainsKey(robotID))
                {
                    if (_bufferedState.Robots[team][robotID].Arrows.ContainsKey(type))
                        _bufferedState.Robots[team][robotID].Arrows[type].ToPoint = toPoint;
                    else
                        _bufferedState.Robots[team][robotID].Arrows.Add(type, new Arrow(toPoint, color));
                }
            }
        }

        public void DrawPath(RobotPath path)
        {
            Team team = path.Team;
            int robotID = path.ID;

            lock (_collectingStateLock)
            {
                if (_bufferedState.Robots[team].ContainsKey(robotID))
                    _bufferedState.Robots[team][robotID].Path = path;
            }
        }

        private void BuildTestScene()
        {
            List<RobotInfo> robots = new List<RobotInfo>();
            robots.Add(new RobotInfo(new Point2(0, 0), Math.PI / 2, Core.Team.Yellow, 2));
            robots.Add(new RobotInfo(new Point2(2, 1.2), Math.PI, Core.Team.Yellow, 3));
            BallInfo ball = new BallInfo(new Point2(1, 2));

            Point2 marker1loc = new Point2(-0.5, 0.5);
            Point2 marker2loc = new Point2(-1.5, 1);

            //BeginCollectState();

            UpdateRobotsAndBall(robots, ball);
            AddMarker(marker1loc, Color.Red);
            AddMarker(marker2loc, Color.Cyan);

            DrawArrow(Core.Team.Yellow, 2, ArrowType.Destination, marker1loc);
            DrawArrow(Core.Team.Blue, 2, ArrowType.Waypoint, marker2loc);

            //EndCollectState();
        }

    }
}
