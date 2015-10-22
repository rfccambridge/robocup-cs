using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;
using RFC.Utilities;


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

        private class RobotDrawingInfo
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

        private class Marker
        {
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
        }

        private class Arrow
        {
            public Point2 ToPoint;
            public Color Color;

            public Arrow(Point2 toPoint, Color color)
            {
                ToPoint = toPoint;
                Color = color;
            }
        }

        private class State
        {
            public Dictionary<Team, Dictionary<int, RobotDrawingInfo>> Robots = new Dictionary<Team, Dictionary<int, RobotDrawingInfo>>();
            public BallInfo Ball;
            public Dictionary<int, Marker> Markers = new Dictionary<int, Marker>();

            public int NextRobotHandle = 0;
            public int NextMarkerHandle = 0;

            public State()
            {
                Robots.Add(Team.Yellow, new Dictionary<int, RobotDrawingInfo>());
                Robots.Add(Team.Blue, new Dictionary<int, RobotDrawingInfo>());
            }

            public void Clear()
            {
                Robots[Team.Yellow].Clear();
                Robots[Team.Blue].Clear();
                Ball = null;
                Markers.Clear();

                NextRobotHandle = 0;
                NextMarkerHandle = 0;
            }
        }

        const double MARKER_SIZE = 0.02;

        //Local copies of constants for the field size and parameters
        //They will not change when constants are reloaded!
        double FIELD_WIDTH;
        double FIELD_HEIGHT;
        double EXTENDED_FIELD_WIDTH;
        double EXTENDED_FIELD_HEIGHT;
        double CENTER_CIRCLE_RADIUS;
        double GOAL_WIDTH;
        double GOAL_HEIGHT;
        double DEFENSE_RECT_HEIGHT;
        double DEFENSE_AREA_RADIUS;
        double FIELD_FULL_XMIN;
        double FIELD_FULL_XMAX;
        double FIELD_FULL_YMIN;
        double FIELD_FULL_YMAX;

        FieldDrawerForm _fieldDrawerForm;
        State _bufferedState = new State();
        State _state = new State();
        object _stateLock = new object();
        bool _collectingState = true;
        object _collectingStateLock = new object();
        bool _robotsAndBallUpdated = false;

        //Marker drag and drop
        Marker _draggedMarker; //The marker currently being dragged
        bool _movedDraggedMarker; //Have we ever moved this marker since the start of the drag?

        double _glControlWidth;
        double _glControlHeight;

        OpenTK.Graphics.TextPrinter _printer = new OpenTK.Graphics.TextPrinter();
        double[] _modelViewMatrix = new double[16];
        double[] _projectionMatrix = new double[16];
        int[] _viewport = new int[4];

        const int NUM_VALUES_TO_AVG = 30;
        double interpretFreqAvg, interpretDurationAvg, controllerDurationAvg;
        int interpretFreqCnt = 0, interpretDurationCnt = 0, controllerDurationCnt = 0;
        ServiceManager msngr;

        public bool Visible
        {
            get { return _fieldDrawerForm.Visible; }
        }

        public FieldDrawer()
        {
            FIELD_WIDTH = Constants.Field.WIDTH;
            FIELD_HEIGHT = Constants.Field.HEIGHT;
            EXTENDED_FIELD_WIDTH = Constants.Field.EXTENDED_WIDTH;
            EXTENDED_FIELD_HEIGHT = Constants.Field.EXTENDED_HEIGHT;
            CENTER_CIRCLE_RADIUS = Constants.Field.CENTER_CIRCLE_RADIUS;
            GOAL_HEIGHT = Constants.Field.GOAL_HEIGHT;
            GOAL_WIDTH = Constants.Field.GOAL_WIDTH;
            DEFENSE_RECT_HEIGHT = Constants.Field.DEFENSE_RECT_HEIGHT;
            DEFENSE_AREA_RADIUS = Constants.Field.DEFENSE_AREA_RADIUS;
            FIELD_FULL_XMIN = Constants.Field.FULL_XMIN;
            FIELD_FULL_XMAX = Constants.Field.FULL_XMAX;
            FIELD_FULL_YMIN = Constants.Field.FULL_YMIN;
            FIELD_FULL_YMAX = Constants.Field.FULL_YMAX;

            double ratio = FIELD_HEIGHT / FIELD_WIDTH;
            _fieldDrawerForm = new FieldDrawerForm(this, ratio);
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener(this.Queued<RobotVisionMessage>(new object()));
            msngr.RegisterListener(this.Queued<BallVisionMessage>(new object()));
            msngr.RegisterListener(this.Queued<RobotPathMessage>(new object()));
            msngr.RegisterListener(this.Queued<RobotDestinationMessage>(new object()));
            msngr.RegisterListener(this.Queued<RefboxStateMessage>(new object()));
            msngr.RegisterListener(this.Queued<VisualDebugMessage>(new object()));
            msngr.RegisterListener(this.Queued<VisualDebugMessage<Lattice<Color>>>(new object()));
        }

        public void Init(int w, int h)
        {
            GL.ClearColor(Color.DarkGreen);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            Resize(w, h);


            _robotInfos = new Dictionary<Team, Dictionary<int, string>>();
            _robotInfos.Add(Team.Blue, new Dictionary<int, string>());
            _robotInfos.Add(Team.Yellow, new Dictionary<int, string>());

            // For debugging
            //BuildTestScene();
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
            UpdateRefBoxCmd(msg.PlayType.ToString());
        }

        Lattice<Color> last_lattice = null;
        public void HandleMessage(VisualDebugMessage<Lattice<Color>> message)
        {
            lock (_stateLock)
            {
                last_lattice = message.value;
            }
        }

        public void Resize(int w, int h)
        {
            _glControlWidth = w;
            _glControlHeight = h;
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }

        public void Show()
        {
            _fieldDrawerForm.Show();
        }

        public void Hide()
        {
            _fieldDrawerForm.Hide();
        }

        public void MouseDown(Point loc)
        {
            Point2 pt = controlToFieldCoords(loc);
            _draggedMarker = null;
            _movedDraggedMarker = false;
            lock (_stateLock)
            {
                foreach (Marker marker in _state.Markers.Values)
                    if (ptInsideMarker(marker, pt))
                        _draggedMarker = marker;
            }
        }

        public void MouseUp(Point loc)
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
        }

        public void MouseMove(Point loc)
        {
            if (_draggedMarker != null)
            {
                _movedDraggedMarker = true;
                Point2 pt = controlToFieldCoords(loc);
                lock (_collectingStateLock)
                {
                    _draggedMarker.Location = pt;
                    WaypointMoved?.Invoke(this, new EventArgs<WaypointMovedInfo>(
                                                new WaypointMovedInfo(_draggedMarker.Object, _draggedMarker.Color, _draggedMarker.Location, _draggedMarker.Orientation)));
                }
            }
            _fieldDrawerForm.InvalidateGLControl();
        }

        public void DragDrop(object obj, Point loc)
        {
            if (obj.GetType() == typeof(EventArgs<WaypointInfo>))
            {
                EventArgs<WaypointInfo> eventArgs = obj as EventArgs<WaypointInfo>;
                RobotInfo waypoint = eventArgs.Data.Object as RobotInfo;
                waypoint.Position = controlToFieldCoords(loc);
                WaypointAdded?.Invoke(this, eventArgs);
            }
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

            _fieldDrawerForm.InvalidateGLControl();
        }

        public void Paint()
        {
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
        }

        Team _team;
        public void UpdateTeam(Team team)
        {
            _team = team;
            _fieldDrawerForm.UpdateTeam(team);
        }

        public void UpdateRefBoxCmd(string refBoxCmd)
        {
            _fieldDrawerForm.UpdateRefBoxCmd(refBoxCmd);
        }

        public void UpdatePlayType(PlayType playType)
        {
            _fieldDrawerForm.UpdatePlayType(playType);
        }

        public void UpdateInterpretFreq(double freq)
        {
            if (interpretFreqCnt < NUM_VALUES_TO_AVG)
                interpretFreqCnt++;

            double prop = 1.0 / interpretFreqCnt;
            interpretFreqAvg = freq * prop + interpretFreqAvg * (1.0 - prop);
            _fieldDrawerForm.UpdateInterpretFreq(interpretFreqAvg);
        }

        public void UpdateInterpretDuration(double duration)
        {
            if (interpretDurationCnt < NUM_VALUES_TO_AVG)
                interpretDurationCnt++;

            double prop = 1.0 / interpretDurationCnt;
            interpretDurationAvg = duration * prop + interpretDurationAvg * (1.0 - prop);
            _fieldDrawerForm.UpdateInterpretDuration(interpretDurationAvg);
        }

        public void UpdateLapDuration(double duration)
        {
            _fieldDrawerForm.UpdateLapDuration(duration);
        }

        public void UpdateControllerDuration(double duration)
        {
            if (controllerDurationCnt < NUM_VALUES_TO_AVG)
                controllerDurationCnt++;

            double prop = 1.0 / controllerDurationCnt;
            controllerDurationAvg = duration * prop + controllerDurationAvg * (1.0 - prop);
            _fieldDrawerForm.UpdateControllerDuration(controllerDurationAvg);
        }

        public void UpdatePlayName(Team team, int robotID, string name)
        {
            if (team == _team)
                _fieldDrawerForm.UpdatePlayName(name);
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
        public int AddMarker(Point2 location, Color color)
        {
            return AddMarker(location, color, null);
        }

        public int AddMarker(Point2 location, Color color, Object obj)
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


        private bool ptInsideMarker(Marker marker, Point2 pt)
        {
            if (pt.X < marker.Location.X - MARKER_SIZE || pt.X > marker.Location.X + MARKER_SIZE
                || pt.Y < marker.Location.Y - MARKER_SIZE || pt.Y > marker.Location.Y + MARKER_SIZE)
                return false;
            return true;
        }

        private Point2 controlToFieldCoords(Point loc)
        {
            OpenTK.Vector3 world = screenToWorld(new OpenTK.Vector3(loc.X, loc.Y, 0));
            return new Point2(world.X, world.Y);
        }

        private void BuildTestScene()
        {
            List<RobotInfo> robots = new List<RobotInfo>();
            robots.Add(new RobotInfo(new Point2(0, 0), Math.PI / 2, Team.Yellow, 2));
            robots.Add(new RobotInfo(new Point2(2, 1.2), Math.PI, Team.Yellow, 3));
            BallInfo ball = new BallInfo(new Point2(1, 2));

            Point2 marker1loc = new Point2(-0.5, 0.5);
            Point2 marker2loc = new Point2(-1.5, 1);

            //BeginCollectState();

            UpdateRobotsAndBall(robots, ball);
            AddMarker(marker1loc, Color.Red);
            AddMarker(marker2loc, Color.Cyan);

            DrawArrow(Team.Yellow, 2, ArrowType.Destination, marker1loc);
            DrawArrow(Team.Blue, 2, ArrowType.Waypoint, marker2loc);

            //EndCollectState();
        }

    }
}
