using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;

//Disable warnings about deprecated objects (the GLU classes)
#pragma warning disable 0612, 0618

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
        public Vector2 NewLocation;
        public double NewOrientation;
        public WaypointMovedInfo(Object obj, Color color, Vector2 newLocation, double newOrientation)
            : base(obj, color)
        {
            NewLocation = newLocation;
            NewOrientation = newOrientation;
        }
    }

    public class FieldDrawer
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
            public Vector2 Location;
            public Color Color;
            public object Object;

            public bool DoDrawOrientation;
            public double Orientation;

            public Marker(Vector2 loc, Color col, Object obj)
            {
                Location = loc;
                Color = col;
                Object = obj;

                DoDrawOrientation = false;
                Orientation = 0;
            }

            public Marker(Vector2 loc, double orientation, Color col, Object obj)
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
            public Vector2 ToPoint;
            public Color Color;

            public Arrow(Vector2 toPoint, Color color)
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

        IntPtr _ballQuadric, _centerCircleQuadric, _robotQuadric;
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
            msngr.RegisterListener<RobotVisionMessage>(HandleRobotMessage, new object());
            msngr.RegisterListener<BallVisionMessage>(HandleBallMessage, new object());
            msngr.RegisterListener<VisualDebugMessage>(HandleVisualDebugMessage, new object());
            msngr.RegisterListener<RobotPathMessage>(HandlePathMessage, new object());
            msngr.RegisterListener<RobotDestinationMessage>(HandleDestinationMessage, new object());
            msngr.RegisterListener<RefboxStateMessage>(HandleRefboxStateMessage, new object());
        }

        public void Init(int w, int h)
        {
            _glControlWidth = w;
            _glControlHeight = h;

            GL.ClearColor(Color.DarkGreen);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(FIELD_FULL_XMIN, FIELD_FULL_XMAX, FIELD_FULL_YMIN, FIELD_FULL_YMAX, -1, 1);
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area

            _ballQuadric = OpenTK.Graphics.Glu.NewQuadric();
            OpenTK.Graphics.Glu.QuadricDrawStyle(_ballQuadric, OpenTK.Graphics.QuadricDrawStyle.Fill);

            _centerCircleQuadric = OpenTK.Graphics.Glu.NewQuadric();
            OpenTK.Graphics.Glu.QuadricDrawStyle(_centerCircleQuadric, OpenTK.Graphics.QuadricDrawStyle.Silhouette);

            _robotQuadric = OpenTK.Graphics.Glu.NewQuadric();
            OpenTK.Graphics.Glu.QuadricDrawStyle(_robotQuadric, OpenTK.Graphics.QuadricDrawStyle.Line);

            _robotInfos = new Dictionary<Team, Dictionary<int, string>>();
            _robotInfos.Add(Team.Blue, new Dictionary<int, string>());
            _robotInfos.Add(Team.Yellow, new Dictionary<int, string>());

            // For debugging
            //BuildTestScene();
        }

        public void HandleRobotMessage(RobotVisionMessage message)
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

        public void HandleBallMessage(BallVisionMessage message)
        {
            lock (_stateLock)
            {
                _bufferedState.Ball = message.Ball;
            }
        }

        public void HandleVisualDebugMessage(VisualDebugMessage msg)
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

        public void HandlePathMessage(RobotPathMessage msg)
        {

            if (debug_motion)
            {
                lock (_stateLock)
                {
                    DrawPath(msg.Path);
                }
            }
            
        }

        public void HandleDestinationMessage(RobotDestinationMessage msg)
        {
            if (debug_motion)
            {
                lock(_stateLock)
                {
                    DrawArrow(msg.Destination.Team, msg.Destination.ID, ArrowType.Destination, msg.Destination.Position);
                }
            }
        }

        public void HandleRefboxStateMessage(RefboxStateMessage msg)
        {
            UpdateRefBoxCmd(msg.PlayType.ToString());
        }

        public void Resize(int w, int h)
        {
            _glControlWidth = w;
            _glControlHeight = h;            
            GL.Viewport(0, 0, w, h);
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
            Vector2 pt = controlToFieldCoords(loc);
            _draggedMarker = null;
            _movedDraggedMarker = false;
            lock (_stateLock) {
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
                    if (WaypointRemoved != null)
                        WaypointRemoved(this, new EventArgs<WaypointInfo>(
                                                new WaypointInfo(_draggedMarker.Object, _draggedMarker.Color)));
                }

                if (!_movedDraggedMarker)
                {
                    _draggedMarker.Orientation += Math.PI / 8;
                    if (WaypointMoved != null)
                        WaypointMoved(this, new EventArgs<WaypointMovedInfo>(
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
                Vector2 pt = controlToFieldCoords(loc);
                lock (_collectingStateLock)
                {
                    _draggedMarker.Location = pt;
                    if (WaypointMoved != null)
                        WaypointMoved(this, new EventArgs<WaypointMovedInfo>(
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
                if (WaypointAdded != null)
                    WaypointAdded(this, eventArgs);
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

        public void UpdateState() {
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
                drawField();
                foreach (Marker marker in _state.Markers.Values)
                {
                    drawMarker(marker);

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
            if(team == _team)
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
        public int AddMarker(Vector2 location, Color color)
        {
            return AddMarker(location, color, null);
        }

        public int AddMarker(Vector2 location, Color color, Object obj)
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
            lock (_stateLock) {
                return _state.Markers[handle].Color;
            }
        }

        public void DrawArrow(Team team, int robotID, ArrowType type, Vector2 toPoint)
        {
            Color color = Color.Black;

            switch (type) {
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
        

        private void drawField()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Extended Field border
            GL.Color3(Color.ForestGreen);
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(-EXTENDED_FIELD_WIDTH / 2, -EXTENDED_FIELD_HEIGHT / 2);
            GL.Vertex2(EXTENDED_FIELD_WIDTH / 2, -EXTENDED_FIELD_HEIGHT / 2);
            GL.Vertex2(EXTENDED_FIELD_WIDTH / 2, EXTENDED_FIELD_HEIGHT / 2);
            GL.Vertex2(-EXTENDED_FIELD_WIDTH / 2, EXTENDED_FIELD_HEIGHT / 2);
            GL.End();

            // Field border
            GL.Color3(Color.White);
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(-FIELD_WIDTH / 2, -FIELD_HEIGHT / 2);
            GL.Vertex2(FIELD_WIDTH / 2, -FIELD_HEIGHT / 2);
            GL.Vertex2(FIELD_WIDTH / 2, FIELD_HEIGHT / 2);
            GL.Vertex2(-FIELD_WIDTH / 2, FIELD_HEIGHT / 2);
            GL.End();

            // Center line
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(0, FIELD_HEIGHT / 2);
            GL.Vertex2(0, -FIELD_HEIGHT / 2);
            GL.End();
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(FIELD_WIDTH / 2, 0);
            GL.Vertex2(FIELD_WIDTH / 2, 0);
            GL.End();

            // Center circle
            const int SLICES = 20;
            GL.LoadIdentity();
            //GL.Translate(0, 0, 0);            
            GL.Begin(BeginMode.LineLoop);
            OpenTK.Graphics.Glu.Disk(_centerCircleQuadric, 0, CENTER_CIRCLE_RADIUS, SLICES, 1);
            GL.End();

            //Goals
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(-FIELD_WIDTH / 2, GOAL_HEIGHT / 2);
            GL.Vertex2(-FIELD_WIDTH / 2 - GOAL_WIDTH, GOAL_HEIGHT / 2);
            GL.Vertex2(-FIELD_WIDTH / 2 - GOAL_WIDTH, -GOAL_HEIGHT / 2);
            GL.Vertex2(-FIELD_WIDTH / 2, -GOAL_HEIGHT / 2);
            GL.End();

            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(FIELD_WIDTH / 2, GOAL_HEIGHT / 2);
            GL.Vertex2(FIELD_WIDTH / 2 + GOAL_WIDTH, GOAL_HEIGHT / 2);
            GL.Vertex2(FIELD_WIDTH / 2 + GOAL_WIDTH, -GOAL_HEIGHT / 2);
            GL.Vertex2(FIELD_WIDTH / 2, -GOAL_HEIGHT / 2);
            GL.End();

            //Defense areas
            GL.LoadIdentity();
            GL.Translate(-FIELD_WIDTH / 2, DEFENSE_RECT_HEIGHT/2, 0);            
            GL.Begin(BeginMode.LineLoop);
            OpenTK.Graphics.Glu.PartialDisk(_centerCircleQuadric,0,DEFENSE_AREA_RADIUS,SLICES,1,0,90);
            GL.End();
            GL.LoadIdentity();
            GL.Translate(-FIELD_WIDTH / 2, -DEFENSE_RECT_HEIGHT / 2, 0);
            GL.Begin(BeginMode.LineLoop);
            OpenTK.Graphics.Glu.PartialDisk(_centerCircleQuadric,0,DEFENSE_AREA_RADIUS,SLICES,1,90,90);
            GL.End();
            GL.LoadIdentity();
            GL.Translate(FIELD_WIDTH / 2, DEFENSE_RECT_HEIGHT / 2, 0);
            GL.Begin(BeginMode.LineLoop);
            OpenTK.Graphics.Glu.PartialDisk(_centerCircleQuadric, 0, DEFENSE_AREA_RADIUS, SLICES, 1, 270, 90);
            GL.End();
            GL.LoadIdentity();
            GL.Translate(FIELD_WIDTH / 2, -DEFENSE_RECT_HEIGHT / 2, 0);
            GL.Begin(BeginMode.LineLoop);
            OpenTK.Graphics.Glu.PartialDisk(_centerCircleQuadric, 0, DEFENSE_AREA_RADIUS, SLICES, 1, 180, 90);
            GL.End();
            GL.LoadIdentity();
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(-FIELD_WIDTH / 2 + DEFENSE_AREA_RADIUS, -DEFENSE_RECT_HEIGHT / 2);
            GL.Vertex2(-FIELD_WIDTH / 2 + DEFENSE_AREA_RADIUS, DEFENSE_RECT_HEIGHT / 2);
            GL.End();
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(FIELD_WIDTH / 2 - DEFENSE_AREA_RADIUS, -DEFENSE_RECT_HEIGHT / 2);
            GL.Vertex2(FIELD_WIDTH / 2 - DEFENSE_AREA_RADIUS, DEFENSE_RECT_HEIGHT / 2);
            GL.End();
        }

        private void drawRobot(RobotDrawingInfo drawingInfo)
        {
            const double ROBOT_ARC_SWEEP = 270; // degrees
            const int SLICES = 10;

            RobotInfo robot = drawingInfo.RobotInfo;
            double angle = (robot.Orientation + Math.PI) * 180 / Math.PI;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(robot.Position.X, robot.Position.Y, 0);
            GL.Rotate(angle, 0, 0, 1);
            GL.Color3(robot.Team == Team.Yellow ? Color.Yellow : Color.Blue);            
            GL.Begin(BeginMode.Polygon);

            OpenTK.Graphics.Glu.PartialDisk(_robotQuadric, 0, Constants.Basic.ROBOT_RADIUS, SLICES, 1,
                                            -(360 - ROBOT_ARC_SWEEP) / 2, ROBOT_ARC_SWEEP);
            GL.End();
            
            /*
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            // TODO: Figure out the real way to render text!
            renderString(robot.ID.ToString(), robot.Position + new Vector2(-0.03, 0.045), Color.Red, 8);
            renderString(drawingInfo.PlayName, robot.Position + new Vector2(-0.05, -0.05), Color.Cyan, 8);
            */
            
            foreach (Arrow arrow in drawingInfo.Arrows.Values)                
                drawArrow(robot.Position, arrow.ToPoint, arrow.Color);

            if (drawingInfo.Path != null)
                drawPath(drawingInfo.Path);
        }

        private void drawBall(BallInfo ball)
        {
            const int SLICES = 8;
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(ball.Position.X, ball.Position.Y, 0);
            GL.Color3(Color.Orange);
            GL.Begin(BeginMode.Polygon);
            OpenTK.Graphics.Glu.Disk(_ballQuadric, 0, Constants.Basic.BALL_RADIUS, SLICES, 1);
            GL.End();
        }

        private void drawMarker(Marker marker)
        {            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(marker.Location.X, marker.Location.Y, 0);
            GL.Rotate(marker.Orientation * 180.0/Math.PI, 0, 0, 1.0);
            GL.Color3(marker.Color);

            if (!marker.DoDrawOrientation)
            {
                GL.Begin(BeginMode.Quads);
                GL.Vertex2(-MARKER_SIZE, MARKER_SIZE);
                GL.Vertex2(MARKER_SIZE, MARKER_SIZE);
                GL.Vertex2(MARKER_SIZE, -MARKER_SIZE);
                GL.Vertex2(-MARKER_SIZE, -MARKER_SIZE);
                GL.End();
            }
            else
            {
                GL.Begin(BeginMode.Triangles);
                GL.Vertex2(MARKER_SIZE * 2, 0);
                GL.Vertex2(-MARKER_SIZE * 2, MARKER_SIZE);
                GL.Vertex2(-MARKER_SIZE * 2, -MARKER_SIZE);
                GL.End();
            }
        }

        private void drawPath(RobotPath path)
        {
            const double WAYPOINT_RADIUS = 0.02;
            const int SLICES = 8;

            if (path.isEmpty())
                return;

            foreach (RobotInfo waypoint in path.Waypoints)
            {
                //GL.Vertex2(waypoint.Position.X, waypoint.Position.Y);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.Color3(path.Team == Team.Yellow ? Color.Yellow : Color.Blue);
                GL.Translate(waypoint.Position.X, waypoint.Position.Y, 0);
                GL.Begin(BeginMode.Polygon);
                OpenTK.Graphics.Glu.Disk(_ballQuadric, 0, WAYPOINT_RADIUS, SLICES, 1);
                GL.End();
            }
        }

        private bool ptInsideMarker(Marker marker, Vector2 pt)
        {
            if (pt.X < marker.Location.X - MARKER_SIZE || pt.X > marker.Location.X + MARKER_SIZE
                || pt.Y < marker.Location.Y - MARKER_SIZE || pt.Y > marker.Location.Y + MARKER_SIZE)
                return false;
            return true;
        }

        private void drawArrow(Vector2 fromPoint, Vector2 toPoint, Color color)
        {
            const double TIP_HEIGHT = 0.15;
            const double TIP_BASE = 0.1;
            double angle = Math.Atan2(toPoint.Y - fromPoint.Y, toPoint.X - fromPoint.X) * 180 / Math.PI;
            double length = Math.Sqrt(fromPoint.distanceSq(toPoint));
            GL.Color3(color);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(fromPoint.X, fromPoint.Y, 0);
            GL.Rotate(angle, 0, 0, 1);
            GL.Begin(BeginMode.Triangles);
            GL.Vertex2(0, 0);
            GL.Vertex2(length - TIP_HEIGHT, TIP_BASE / 4);
            GL.Vertex2(length - TIP_HEIGHT, -TIP_BASE / 4);
            GL.Vertex2(length, 0);
            GL.Vertex2(length - TIP_HEIGHT, TIP_BASE / 2);
            GL.Vertex2(length - TIP_HEIGHT, -TIP_BASE / 2);
            GL.End();
        }

        private OpenTK.Vector3 worldToScreen(OpenTK.Vector3 world)
        {
            OpenTK.Vector3 screen;          

            GL.GetInteger(GetPName.Viewport, _viewport);
            GL.GetDouble(GetPName.ModelviewMatrix, _modelViewMatrix);
            GL.GetDouble(GetPName.ProjectionMatrix, _projectionMatrix);


            OpenTK.Graphics.Glu.Project(world, _modelViewMatrix, _projectionMatrix, _viewport,
                                        out screen);

            screen.Y = _viewport[3] - screen.Y;
            return screen;
        }

        private void renderString(string s, Vector2 location, Color color, float size)
        {
            OpenTK.Vector3 screen = worldToScreen(new OpenTK.Vector3((float)location.X, (float)location.Y, 0.0f));            

            _printer.Begin();
            GL.Translate(screen);
            _printer.Print(s, new Font(FontFamily.GenericSansSerif, size), color);
            _printer.End();
        }

        private Vector2 controlToFieldCoords(Point loc)
        {
            double viewWidth = FIELD_FULL_XMAX - FIELD_FULL_XMIN;
            double viewHeight = FIELD_FULL_YMAX - FIELD_FULL_YMIN;
            double translateX = FIELD_FULL_XMIN;
            double translateY = FIELD_FULL_YMIN;
            return new Vector2((double)loc.X / _glControlWidth * viewWidth + translateX, 
                               (1 - (double)loc.Y / _glControlHeight) * viewHeight + translateY);
        }

        private void BuildTestScene()
        {
            List<RobotInfo> robots = new List<RobotInfo>();
            robots.Add(new RobotInfo(new Vector2(0, 0), Math.PI / 2, 2));
            robots.Add(new RobotInfo(new Vector2(2, 1.2), Math.PI, 3));
            BallInfo ball = new BallInfo(new Vector2(1, 2));

            Vector2 marker1loc = new Vector2(-0.5, 0.5);
            Vector2 marker2loc = new Vector2(-1.5, 1);

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

#pragma warning restore 0612, 0618
