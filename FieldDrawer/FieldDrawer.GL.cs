using OpenTK.Graphics.OpenGL;
using RFC.Core;
using RFC.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Disable warnings about deprecated objects (the GLU classes)
#pragma warning disable 0612, 0618

namespace RFC.FieldDrawer
{
    partial class FieldDrawer : OpenTK.GLControl
    {
        OpenTK.Graphics.TextPrinter _printer = new OpenTK.Graphics.TextPrinter();
        double[] _modelViewMatrix = new double[16];
        double[] _projectionMatrix = new double[16];
        int[] _viewport = new int[4];
        double _glControlWidth;
        double _glControlHeight;

        bool controlLoaded = false;

        private void resizeGL(int w, int h)
        {
            if (!controlLoaded) return;

            _glControlWidth = w;
            _glControlHeight = h;
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.DarkGreen);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            controlLoaded = true;
            resizeGL(Width, Height);
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            resizeGL(Width, Height);
            Invalidate();
            base.OnResize(e);
        }

        private void updateProjection()
        {
            double actualAspect = _glControlWidth / _glControlHeight;
            double desiredW = C.FULL_XMAX - C.FULL_XMIN;
            double desiredH = C.FULL_YMAX - C.FULL_YMIN;
            double desiredAspect = desiredW / desiredH;

            double marginX = 0, marginY = 0;

            if (actualAspect > desiredAspect)
            {
                // render window is wider than field
                marginX = (desiredH * actualAspect - desiredW) / 2;
            }
            else
            {
                // render window is taller than field
                marginY = (desiredW / actualAspect - desiredH) / 2;
            }
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(
                C.FULL_XMIN - marginX, C.FULL_XMAX + marginX,
                C.FULL_YMIN - marginY, C.FULL_YMAX + marginY,
                -1, 1
            );
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

        private OpenTK.Vector3 screenToWorld(OpenTK.Vector3 screen)
        {
            screen.Y = _viewport[3] - screen.Y;
            OpenTK.Vector3 world;

            GL.GetInteger(GetPName.Viewport, _viewport);
            GL.GetDouble(GetPName.ModelviewMatrix, _modelViewMatrix);
            GL.GetDouble(GetPName.ProjectionMatrix, _projectionMatrix);

            OpenTK.Graphics.Glu.UnProject(screen, _modelViewMatrix, _projectionMatrix, _viewport,
                                        out world);

            return world;
        }

        private void drawString(string s, Vector2 location, Color color, float size)
        {
            OpenTK.Vector3 screen = worldToScreen(new OpenTK.Vector3((float)location.X, (float)location.Y, 0.0f));

            _printer.Begin();
            GL.Translate(screen);
            _printer.Print(s, new Font(FontFamily.GenericSansSerif, size), color);
            _printer.End();
        }

        void drawCircle(double radius, bool fill = false)
        {
            if (fill)
                GL.Begin(BeginMode.Polygon);
            else
                GL.Begin(BeginMode.LineLoop);

            for (int i = 0; i < 360; i++)
            {
                double degInRad = i * Math.PI / 180;
                GL.Vertex2(Math.Cos(degInRad) * radius, Math.Sin(degInRad) * radius);
            }
            GL.End();
        }

        private void drawPath(RobotPath path)
        {
            const double WAYPOINT_RADIUS = 0.02;

            if (path.isEmpty())
                return;

            foreach (RobotInfo waypoint in path.Waypoints)
            {
                //GL.Vertex2(waypoint.Position.X, waypoint.Position.Y);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.Color3(path.Team == Core.Team.Yellow ? Color.Yellow : Color.Blue);
                GL.Translate(waypoint.Position.X, waypoint.Position.Y, 0);
                drawCircle(WAYPOINT_RADIUS, true);
            }
        }

        private void drawArrow(Point2 fromPoint, Point2 toPoint, Color color)
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

        void drawPartialCircle(double radius, int startAngle, int endAngle)
        {
            GL.Begin(BeginMode.LineStrip);

            for (int i = startAngle; i < endAngle; i++)
            {
                double degInRad = i * Math.PI / 180;
                GL.Vertex2(Math.Cos(degInRad) * radius, Math.Sin(degInRad) * radius);
            }
            GL.End();
        }

        private void drawBall(BallInfo ball)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(ball.Position.X, ball.Position.Y, 0);
            GL.Color3(Color.Orange);
            drawCircle(Constants.Basic.BALL_RADIUS, true);
        }

        private void drawMarker(Marker marker)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(marker.Location.X, marker.Location.Y, 0);
            GL.Rotate(marker.Orientation * 180.0 / Math.PI, 0, 0, 1.0);
            GL.Color3(marker.Color);

            if (!marker.DoDrawOrientation)
            {
                GL.Begin(BeginMode.Quads);
                GL.Vertex2(-Marker.SIZE, Marker.SIZE);
                GL.Vertex2(Marker.SIZE, Marker.SIZE);
                GL.Vertex2(Marker.SIZE, -Marker.SIZE);
                GL.Vertex2(-Marker.SIZE, -Marker.SIZE);
                GL.End();
            }
            else
            {
                GL.Begin(BeginMode.Triangles);
                GL.Vertex2(Marker.SIZE * 2, 0);
                GL.Vertex2(-Marker.SIZE * 2, Marker.SIZE);
                GL.Vertex2(-Marker.SIZE * 2, -Marker.SIZE);
                GL.End();
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
            GL.Vertex2(-C.EXTENDED_WIDTH / 2, -C.EXTENDED_HEIGHT / 2);
            GL.Vertex2(C.EXTENDED_WIDTH / 2, -C.EXTENDED_HEIGHT / 2);
            GL.Vertex2(C.EXTENDED_WIDTH / 2, C.EXTENDED_HEIGHT / 2);
            GL.Vertex2(-C.EXTENDED_WIDTH / 2, C.EXTENDED_HEIGHT / 2);
            GL.End();

            // Field border
            GL.Color3(Color.White);
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(-C.WIDTH / 2, -C.HEIGHT / 2);
            GL.Vertex2(C.WIDTH / 2, -C.HEIGHT / 2);
            GL.Vertex2(C.WIDTH / 2, C.HEIGHT / 2);
            GL.Vertex2(-C.WIDTH / 2, C.HEIGHT / 2);
            GL.End();

            // Center line
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(0, C.HEIGHT / 2);
            GL.Vertex2(0, -C.HEIGHT / 2);
            GL.End();
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(C.WIDTH / 2, 0);
            GL.Vertex2(C.WIDTH / 2, 0);
            GL.End();

            // Center circle
            GL.LoadIdentity();
            GL.Translate(0, 0, 0);
            drawCircle(C.CENTER_CIRCLE_RADIUS);


            //Goals
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(-C.WIDTH / 2, C.GOAL_HEIGHT / 2);
            GL.Vertex2(-C.WIDTH / 2 - C.GOAL_WIDTH, C.GOAL_HEIGHT / 2);
            GL.Vertex2(-C.WIDTH / 2 - C.GOAL_WIDTH, -C.GOAL_HEIGHT / 2);
            GL.Vertex2(-C.WIDTH / 2, -C.GOAL_HEIGHT / 2);
            GL.End();

            GL.Begin(BeginMode.LineLoop);
            GL.Vertex2(C.WIDTH / 2, C.GOAL_HEIGHT / 2);
            GL.Vertex2(C.WIDTH / 2 + C.GOAL_WIDTH, C.GOAL_HEIGHT / 2);
            GL.Vertex2(C.WIDTH / 2 + C.GOAL_WIDTH, -C.GOAL_HEIGHT / 2);
            GL.Vertex2(C.WIDTH / 2, -C.GOAL_HEIGHT / 2);
            GL.End();

            //Defense areas
            GL.LoadIdentity();
            GL.Translate(-C.WIDTH / 2, C.DEFENSE_RECT_HEIGHT / 2, 0);
            drawPartialCircle(C.DEFENSE_AREA_RADIUS, 0, 90);
            GL.LoadIdentity();
            GL.Translate(-C.WIDTH / 2, -C.DEFENSE_RECT_HEIGHT / 2, 0);
            drawPartialCircle(C.DEFENSE_AREA_RADIUS, -90, 0);
            GL.LoadIdentity();
            GL.Translate(C.WIDTH / 2, C.DEFENSE_RECT_HEIGHT / 2, 0);
            drawPartialCircle(C.DEFENSE_AREA_RADIUS, 90, 180);
            GL.LoadIdentity();
            GL.Translate(C.WIDTH / 2, -C.DEFENSE_RECT_HEIGHT / 2, 0);
            drawPartialCircle(C.DEFENSE_AREA_RADIUS, 180, 270);
            GL.LoadIdentity();
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(-C.WIDTH / 2 + C.DEFENSE_AREA_RADIUS, -C.DEFENSE_RECT_HEIGHT / 2);
            GL.Vertex2(-C.WIDTH / 2 + C.DEFENSE_AREA_RADIUS, C.DEFENSE_RECT_HEIGHT / 2);
            GL.End();
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(C.WIDTH / 2 - C.DEFENSE_AREA_RADIUS, -C.DEFENSE_RECT_HEIGHT / 2);
            GL.Vertex2(C.WIDTH / 2 - C.DEFENSE_AREA_RADIUS, C.DEFENSE_RECT_HEIGHT / 2);
            GL.End();
        }

        private void drawRobot(RobotDrawingInfo drawingInfo)
        {
            // const double ROBOT_ARC_SWEEP = 270; // degrees
            // const int SLICES = 10;

            RobotInfo robot = drawingInfo.RobotInfo;
            double angle = (robot.Orientation + Math.PI) * 180 / Math.PI;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Translate(robot.Position.X, robot.Position.Y, 0);
            GL.Rotate(angle, 0, 0, 1);
            GL.Color3(robot.Team == Core.Team.Yellow ? Color.Yellow : Color.Blue);
            //GL.Begin(BeginMode.Polygon);

            //OpenTK.Graphics.Glu.PartialDisk(_robotQuadric, 0, Constants.Basic.ROBOT_RADIUS, SLICES, 1,
            //                                -(360 - ROBOT_ARC_SWEEP) / 2, ROBOT_ARC_SWEEP);
            //GL.End();
            // TODO: better drawing of robot
            drawCircle(Constants.Basic.ROBOT_RADIUS, true);

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

        private void drawLattice(Lattice<Color> l)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            for (int i = 0; i < l.Spec.Samples; i++)
            {
                for (int j = 0; j < l.Spec.Samples; j++)
                {
                    Geometry.Rectangle cell = l.Spec.indexToRect(i, j);
                    Color c = l.Get(i, j);

                    GL.Begin(BeginMode.Polygon);
                    GL.Color4(c);
                    GL.Vertex2(cell.XMin, cell.YMin);
                    GL.Vertex2(cell.XMin, cell.YMax);
                    GL.Vertex2(cell.XMax, cell.YMax);
                    GL.Vertex2(cell.XMax, cell.YMin);
                    GL.End();
                }
            }
        }
    }
}

#pragma warning restore 0612, 0618