using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RFC.Geometry;

namespace RFC.Core
{
	static public class Constants
	{
		//NOTE: Values are stored as float because doubles can't be volatile!

		//CONSTANTS------------------------------------------------------------------------------------
		static public class Basic
		{
			/// <summary> Number of robots and/or robot ids possible </summary>
			static public int NUM_ROBOTS { get { InitializeIfNeeded(); return _NUM_ROBOTS; } } static volatile int _NUM_ROBOTS;

			/// <summary> Radius of a robot </summary>
			static public double ROBOT_RADIUS { get { InitializeIfNeeded(); return _ROBOT_RADIUS; } } static volatile float _ROBOT_RADIUS;

			/// <summary> Distance from the center to the front of a robot </summary>
			static public double ROBOT_FRONT_RADIUS { get { InitializeIfNeeded(); return _ROBOT_FRONT_RADIUS; } } static volatile float _ROBOT_FRONT_RADIUS;

			/// <summary> Radius of the ball </summary>
			static public double BALL_RADIUS { get { InitializeIfNeeded(); return _BALL_RADIUS; } } static volatile float _BALL_RADIUS;

			static public void Reload()
			{
				_NUM_ROBOTS = ConstantsRaw.get<int>("default", "NUM_ROBOTS");
				_ROBOT_RADIUS = (float)ConstantsRaw.get<double>("default", "ROBOT_RADIUS");
				_ROBOT_FRONT_RADIUS = (float)ConstantsRaw.get<double>("default", "ROBOT_FRONT_RADIUS");
				_BALL_RADIUS = (float)ConstantsRaw.get<double>("default", "BALL_RADIUS");
			}
		}

		static public class Field
		{
			//SIZES-----------------------------------------------------------

			/// <summary> Width of the in-bounds part of the field </summary>
			static public double WIDTH { get { InitializeIfNeeded(); return _WIDTH; } } static volatile float _WIDTH;

			/// <summary> Height of the in-bounds part of the field </summary>
			static public double HEIGHT { get { InitializeIfNeeded(); return _HEIGHT; } } static volatile float _HEIGHT;

			/// <summary> Width of field including extra out-of-bounds zone for robot movement </summary>
			static public double EXTENDED_WIDTH { get { InitializeIfNeeded(); return _EXTENDED_WIDTH; } } static volatile float _EXTENDED_WIDTH;

			/// <summary> Height of field including extra out-of-bounds zone for robot movement  </summary>
			static public double EXTENDED_HEIGHT { get { InitializeIfNeeded(); return _EXTENDED_HEIGHT; } } static volatile float _EXTENDED_HEIGHT;

			/// <summary> Width of field including ref zone </summary>
			static public double FULL_WIDTH { get { InitializeIfNeeded(); return _FULL_WIDTH; } } static volatile float _FULL_WIDTH;

			/// <summary> Height of field including ref zone </summary>
			static public double FULL_HEIGHT { get { InitializeIfNeeded(); return _FULL_HEIGHT; } } static volatile float _FULL_HEIGHT;

			/// <summary> Width of the extra out-of-bounds zone for robot movement </summary>
			static public double EXTENDED_BORDER_WIDTH { get { InitializeIfNeeded(); return _EXTENDED_BORDER_WIDTH; } } static volatile float _EXTENDED_BORDER_WIDTH;

			/// <summary> Width of the referree zone </summary>
			static public double REFEREE_WIDTH { get { InitializeIfNeeded(); return _REFEREE_WIDTH; } } static volatile float _REFEREE_WIDTH;

			/// <summary> Height of the rectangular part of the defense area on each side </summary>
			static public double DEFENSE_RECT_HEIGHT { get { InitializeIfNeeded(); return _DEFENSE_RECT_HEIGHT; } } static volatile float _DEFENSE_RECT_HEIGHT;

			/// <summary> Radius and rectangular width of the defense area on each side </summary>
			static public double DEFENSE_AREA_RADIUS { get { InitializeIfNeeded(); return _DEFENSE_AREA_RADIUS; } } static volatile float _DEFENSE_AREA_RADIUS;

			/// <summary> Extended defense area radius that kicking-team robots must stay out of when positioning for free kick </summary>
			static public double EXTENDED_DEFENSE_AREA_RADIUS { get { InitializeIfNeeded(); return _EXTENDED_DEFENSE_AREA_RADIUS; } } static volatile float _EXTENDED_DEFENSE_AREA_RADIUS;

			//BOUNDS----------------------------------------------------------

			/// <summary> X coord of the left boundary line </summary>
			static public double XMIN { get { InitializeIfNeeded(); return _XMIN; } } static volatile float _XMIN;

			/// <summary> X coord of the right boundary line </summary>
			static public double XMAX { get { InitializeIfNeeded(); return _XMAX; } } static volatile float _XMAX;

			/// <summary> Y coord of the bottom boundary line </summary>
			static public double YMIN { get { InitializeIfNeeded(); return _YMIN; } } static volatile float _YMIN;

			/// <summary> Y coord of the top boundary line </summary>
			static public double YMAX { get { InitializeIfNeeded(); return _YMAX; } } static volatile float _YMAX;

			/// <summary> X coord of the left boundary including the extra out-of-bounds zone for robot movement </summary>
			static public double EXTENDED_XMIN { get { InitializeIfNeeded(); return _EXTENDED_XMIN; } } static volatile float _EXTENDED_XMIN;

			/// <summary> X coord of the right boundary including the extra out-of-bounds zone for robot movement </summary>
			static public double EXTENDED_XMAX { get { InitializeIfNeeded(); return _EXTENDED_XMAX; } } static volatile float _EXTENDED_XMAX;

			/// <summary> Y coord of the bottom boundary including the extra out-of-bounds zone for robot movement </summary>
			static public double EXTENDED_YMIN { get { InitializeIfNeeded(); return _EXTENDED_YMIN; } } static volatile float _EXTENDED_YMIN;

			/// <summary> Y coord of the top boundary including the extra out-of-bounds zone for robot movement </summary>
			static public double EXTENDED_YMAX { get { InitializeIfNeeded(); return _EXTENDED_YMAX; } } static volatile float _EXTENDED_YMAX;

			/// <summary> X coord of the left boundary including the ref zone </summary>
			static public double FULL_XMIN { get { InitializeIfNeeded(); return _FULL_XMIN; } } static volatile float _FULL_XMIN;

			/// <summary> X coord of the right boundary including the ref zone </summary>
			static public double FULL_XMAX { get { InitializeIfNeeded(); return _FULL_XMAX; } } static volatile float _FULL_XMAX;

			/// <summary> Y coord of the bottom boundary including the ref zone </summary>
			static public double FULL_YMIN { get { InitializeIfNeeded(); return _FULL_YMIN; } } static volatile float _FULL_YMIN;

			/// <summary> Y coord of the top boundary including the ref zone </summary>
			static public double FULL_YMAX { get { InitializeIfNeeded(); return _FULL_YMAX; } } static volatile float _FULL_YMAX;

			//GOAL--------------------------------------------------------------

			/// <summary> Width of the goal zone </summary>
			static public double GOAL_WIDTH { get { InitializeIfNeeded(); return _GOAL_WIDTH; } } static volatile float _GOAL_WIDTH;

			/// <summary> Height of the goal zone </summary>
			static public double GOAL_HEIGHT { get { InitializeIfNeeded(); return _GOAL_HEIGHT; } } static volatile float _GOAL_HEIGHT;

			/// <summary> Y coord of the bottom of goal zone </summary>
			static public double GOAL_YMIN { get { InitializeIfNeeded(); return _GOAL_YMIN; } } static volatile float _GOAL_YMIN;

			/// <summary> Y coord of the top of goal zone </summary>
			static public double GOAL_YMAX { get { InitializeIfNeeded(); return _GOAL_YMAX; } } static volatile float _GOAL_YMAX;

			/// <summary> X coord of the left of the LEFT goal zone </summary>
			static public double GOAL_XMIN { get { InitializeIfNeeded(); return _GOAL_XMIN; } } static volatile float _GOAL_XMIN;

			/// <summary> X coord of the right of the RIGHT goal zone </summary>
			static public double GOAL_XMAX { get { InitializeIfNeeded(); return _GOAL_XMAX; } } static volatile float _GOAL_XMAX;

			//OTHER----------------------------------------------------------

			/// <summary> Radius of the center circle of the field </summary>
			static public double CENTER_CIRCLE_RADIUS { get { InitializeIfNeeded(); return _CENTER_CIRCLE_RADIUS; } } static volatile float _CENTER_CIRCLE_RADIUS;

			//--------------------------------------------------------------

			static public void Reload()
			{
				_WIDTH = (float)ConstantsRaw.get<double>("plays", "FIELD_WIDTH");
				_HEIGHT = (float)ConstantsRaw.get<double>("plays", "FIELD_HEIGHT");
				_EXTENDED_BORDER_WIDTH = (float)ConstantsRaw.get<double>("plays", "EXTENDED_BORDER_WIDTH");
				_REFEREE_WIDTH = (float)ConstantsRaw.get<double>("plays", "REFEREE_ZONE_WIDTH");
				_GOAL_WIDTH = (float)ConstantsRaw.get<double>("plays", "GOAL_WIDTH");
				_GOAL_HEIGHT = (float)ConstantsRaw.get<double>("plays", "GOAL_HEIGHT");
				_CENTER_CIRCLE_RADIUS = (float)ConstantsRaw.get<double>("plays", "CENTER_CIRCLE_RADIUS");

				_DEFENSE_RECT_HEIGHT = (float)ConstantsRaw.get<double>("plays", "DEFENSE_RECT_HEIGHT");
				_DEFENSE_AREA_RADIUS = (float)ConstantsRaw.get<double>("plays", "DEFENSE_AREA_RADIUS");
				_EXTENDED_DEFENSE_AREA_RADIUS = (float)ConstantsRaw.get<double>("plays", "EXTENDED_DEFENSE_AREA_RADIUS");

				_EXTENDED_WIDTH = _WIDTH + 2.0f * _EXTENDED_BORDER_WIDTH;
				_EXTENDED_HEIGHT = _HEIGHT + 2.0f * _EXTENDED_BORDER_WIDTH;

				_FULL_WIDTH = _WIDTH + 2.0f * ((float)Math.Max(_EXTENDED_BORDER_WIDTH + _REFEREE_WIDTH, _GOAL_WIDTH));
				_FULL_HEIGHT = (float)Math.Max(_HEIGHT + 2.0f * (_EXTENDED_BORDER_WIDTH + _REFEREE_WIDTH), _GOAL_HEIGHT);

				_XMIN = -_WIDTH / 2.0f;
				_XMAX = _WIDTH / 2.0f;
				_YMIN = -_HEIGHT / 2.0f;
				_YMAX = _HEIGHT / 2.0f;

				_EXTENDED_XMIN = -_EXTENDED_WIDTH / 2.0f;
				_EXTENDED_XMAX = _EXTENDED_WIDTH / 2.0f;
				_EXTENDED_YMIN = -_EXTENDED_HEIGHT / 2.0f;
				_EXTENDED_YMAX = _EXTENDED_HEIGHT / 2.0f;

				_FULL_XMIN = -_FULL_WIDTH / 2.0f;
				_FULL_XMAX = _FULL_WIDTH / 2.0f;
				_FULL_YMIN = -_FULL_HEIGHT / 2.0f;
				_FULL_YMAX = _FULL_HEIGHT / 2.0f;

				_GOAL_YMIN = -_GOAL_HEIGHT / 2.0f;
				_GOAL_YMAX = _GOAL_HEIGHT / 2.0f;
				_GOAL_XMIN = _XMIN - _GOAL_WIDTH;
				_GOAL_XMAX = _XMAX + _GOAL_WIDTH;
			}
		}

		static public class FieldPts
		{
			//BASIC LOCATIONS----------------------------------------------------------------------

			/// <summary> Top left corner of field </summary>
			static public Vector2 TOP_LEFT { get { InitializeIfNeeded(); return _TOP_LEFT; } } static volatile Vector2 _TOP_LEFT;

			/// <summary> Bottom left corner of field </summary>
			static public Vector2 BOTTOM_LEFT { get { InitializeIfNeeded(); return _BOTTOM_LEFT; } } static volatile Vector2 _BOTTOM_LEFT;

			/// <summary> Top right corner of field </summary>
			static public Vector2 TOP_RIGHT { get { InitializeIfNeeded(); return _TOP_RIGHT; } } static volatile Vector2 _TOP_RIGHT;

			/// <summary> Bottom right corner of field </summary>
			static public Vector2 BOTTOM_RIGHT { get { InitializeIfNeeded(); return _BOTTOM_RIGHT; } } static volatile Vector2 _BOTTOM_RIGHT;

			/// <summary> Top center edge of field </summary>
			static public Vector2 TOP { get { InitializeIfNeeded(); return _TOP; } } static volatile Vector2 _TOP;

			/// <summary> Bottom center edge of field </summary>
			static public Vector2 BOTTOM { get { InitializeIfNeeded(); return _BOTTOM; } } static volatile Vector2 _BOTTOM;

			/// <summary> Left center edge of field </summary>
			static public Vector2 LEFT { get { InitializeIfNeeded(); return _LEFT; } } static volatile Vector2 _LEFT;

			/// <summary> Right center edge of field </summary>
			static public Vector2 RIGHT { get { InitializeIfNeeded(); return _RIGHT; } } static volatile Vector2 _RIGHT;

			/// <summary> Center of field </summary>
			static public Vector2 CENTER { get { InitializeIfNeeded(); return _CENTER; } } static volatile Vector2 _CENTER;


			//QUADRANTS (octants, really)-----------------------------------------------------------------

			/// <summary> Halfway in between center and top left corner </summary>
			static public Vector2 TOP_LEFT_QUAD { get { InitializeIfNeeded(); return _TOP_LEFT_QUAD; } } static volatile Vector2 _TOP_LEFT_QUAD;

			/// <summary> Halfway in between center and top left corner </summary>
			static public Vector2 BOTTOM_LEFT_QUAD { get { InitializeIfNeeded(); return _BOTTOM_LEFT_QUAD; } } static volatile Vector2 _BOTTOM_LEFT_QUAD;

			/// <summary> Halfway in between center and bottom right corner  </summary>
			static public Vector2 TOP_RIGHT_QUAD { get { InitializeIfNeeded(); return _TOP_RIGHT_QUAD; } } static volatile Vector2 _TOP_RIGHT_QUAD;

			/// <summary> Halfway in between center and bottom right corner </summary>
			static public Vector2 BOTTOM_RIGHT_QUAD { get { InitializeIfNeeded(); return _BOTTOM_RIGHT_QUAD; } } static volatile Vector2 _BOTTOM_RIGHT_QUAD;

			/// <summary> Halfway in between center and top edge </summary>
			static public Vector2 TOP_QUAD { get { InitializeIfNeeded(); return _TOP_QUAD; } } static volatile Vector2 _TOP_QUAD;

			/// <summary> Halfway in between center and bottom edge </summary>
			static public Vector2 BOTTOM_QUAD { get { InitializeIfNeeded(); return _BOTTOM_QUAD; } } static volatile Vector2 _BOTTOM_QUAD;

			/// <summary> Halfway in between center and left edge </summary>
			static public Vector2 LEFT_QUAD { get { InitializeIfNeeded(); return _LEFT_QUAD; } } static volatile Vector2 _LEFT_QUAD;

			/// <summary> Halfway in between center and right edge </summary>
			static public Vector2 RIGHT_QUAD { get { InitializeIfNeeded(); return _RIGHT_QUAD; } } static volatile Vector2 _RIGHT_QUAD;


			//GOAL---------------------------------------------------------

			/// <summary> Center location of the opening of our goal, same as LEFT </summary>
			static public Vector2 OUR_GOAL { get { InitializeIfNeeded(); return _OUR_GOAL; } } static volatile Vector2 _OUR_GOAL;

			/// <summary> Bottom point of our goal </summary>
			static public Vector2 OUR_GOAL_BOTTOM { get { InitializeIfNeeded(); return _OUR_GOAL_BOTTOM; } } static volatile Vector2 _OUR_GOAL_BOTTOM;

			/// <summary> Top point of our goal </summary>
			static public Vector2 OUR_GOAL_TOP { get { InitializeIfNeeded(); return _OUR_GOAL_TOP; } } static volatile Vector2 _OUR_GOAL_TOP;

			/// <summary> Center location of the opening of their goal, same as RIGHT </summary>
			static public Vector2 THEIR_GOAL { get { InitializeIfNeeded(); return _THEIR_GOAL; } } static volatile Vector2 _THEIR_GOAL;

			/// <summary> Bottom point of their goal </summary>
			static public Vector2 THEIR_GOAL_BOTTOM { get { InitializeIfNeeded(); return _THEIR_GOAL_BOTTOM; } } static volatile Vector2 _THEIR_GOAL_BOTTOM;

			/// <summary> Top point of their goal </summary>
			static public Vector2 THEIR_GOAL_TOP { get { InitializeIfNeeded(); return _THEIR_GOAL_TOP; } } static volatile Vector2 _THEIR_GOAL_TOP;

			//EXTENDED BOUNDS---------------------------------------------------------

			/// <summary> Top left corner of field including ref zone </summary>
			static public Vector2 EXTENDED_TOP_LEFT { get { InitializeIfNeeded(); return _EXTENDED_TOP_LEFT; } } static volatile Vector2 _EXTENDED_TOP_LEFT;

			/// <summary> Bottom left corner of field including ref zone </summary>
			static public Vector2 EXTENDED_BOTTOM_LEFT { get { InitializeIfNeeded(); return _EXTENDED_BOTTOM_LEFT; } } static volatile Vector2 _EXTENDED_BOTTOM_LEFT;

			/// <summary> Top right corner of field including ref zone </summary>
			static public Vector2 EXTENDED_TOP_RIGHT { get { InitializeIfNeeded(); return _EXTENDED_TOP_RIGHT; } } static volatile Vector2 _EXTENDED_TOP_RIGHT;

			/// <summary> Bottom right corner of field including ref zone </summary>
			static public Vector2 EXTENDED_BOTTOM_RIGHT { get { InitializeIfNeeded(); return _EXTENDED_BOTTOM_RIGHT; } } static volatile Vector2 _EXTENDED_BOTTOM_RIGHT;


			//FULL BOUNDS---------------------------------------------------------

			/// <summary> Top left corner of field including ref zone </summary>
			static public Vector2 FULL_TOP_LEFT { get { InitializeIfNeeded(); return _FULL_TOP_LEFT; } } static volatile Vector2 _FULL_TOP_LEFT;

			/// <summary> Bottom left corner of field including ref zone </summary>
			static public Vector2 FULL_BOTTOM_LEFT { get { InitializeIfNeeded(); return _FULL_BOTTOM_LEFT; } } static volatile Vector2 _FULL_BOTTOM_LEFT;

			/// <summary> Top right corner of field including ref zone </summary>
			static public Vector2 FULL_TOP_RIGHT { get { InitializeIfNeeded(); return _FULL_TOP_RIGHT; } } static volatile Vector2 _FULL_TOP_RIGHT;

			/// <summary> Bottom right corner of field including ref zone </summary>
			static public Vector2 FULL_BOTTOM_RIGHT { get { InitializeIfNeeded(); return _FULL_BOTTOM_RIGHT; } } static volatile Vector2 _FULL_BOTTOM_RIGHT;

			//LINES------------------------------------------------------------------------------

			/// <summary> Boundary lines of field </summary>
			static public IList<Line> BOUNDARY_LINES { get { InitializeIfNeeded(); return _BOUNDARY_LINES.AsReadOnly(); } } static volatile List<Line> _BOUNDARY_LINES;

			/// <summary> Boundary lines of field including extended out-of-bounds zone for robot movement </summary>
			static public IList<Line> EXTENDED_BOUNDARY_LINES { get { InitializeIfNeeded(); return _EXTENDED_BOUNDARY_LINES.AsReadOnly(); } } static volatile List<Line> _EXTENDED_BOUNDARY_LINES;

			/// <summary> Boundary lines of field including ref zone </summary>
			static public IList<Line> FULL_BOUNDARY_LINES { get { InitializeIfNeeded(); return _FULL_BOUNDARY_LINES.AsReadOnly(); } } static volatile List<Line> _FULL_BOUNDARY_LINES;

			//RECTANGLES-------------------------------------------------------------------------

			/// <summary> Boundary rectangle of field </summary>
			static public Rectangle FIELD_RECT { get { InitializeIfNeeded(); return _FIELD_RECT; } } static volatile Rectangle _FIELD_RECT;

			/// <summary> Boundary rectangle of field including extended out-of-bounds zone for robot movement </summary>
			static public Rectangle EXTENDED_FIELD_RECT { get { InitializeIfNeeded(); return _EXTENDED_FIELD_RECT; } } static volatile Rectangle _EXTENDED_FIELD_RECT;

			/// <summary> Boundary rectangle of field including ref zone </summary>
			static public Rectangle FULL_FIELD_RECT { get { InitializeIfNeeded(); return _FULL_FIELD_RECT; } } static volatile Rectangle _FULL_FIELD_RECT;

			/// <summary> Boundary rectangle of goal box </summary>
			static public Rectangle LEFT_GOAL_BOX { get { InitializeIfNeeded(); return _LEFT_GOAL_BOX; } } static volatile Rectangle _LEFT_GOAL_BOX;

			/// <summary> Boundary rectangle of goal box </summary>
			static public Rectangle RIGHT_GOAL_BOX { get { InitializeIfNeeded(); return _RIGHT_GOAL_BOX; } } static volatile Rectangle _RIGHT_GOAL_BOX;

			//OTHER------------------------------------------------------------------------------
			/// <summary> A list of shapes that together compose the defense area  </summary>
			static public IList<Geom> LEFT_DEFENSE_AREA { get { InitializeIfNeeded(); return _LEFT_DEFENSE_AREA; } } static volatile List<Geom> _LEFT_DEFENSE_AREA;

			/// <summary> A list of shapes that together compose the defense area  </summary>
			static public IList<Geom> RIGHT_DEFENSE_AREA { get { InitializeIfNeeded(); return _RIGHT_DEFENSE_AREA; } } static volatile List<Geom> _RIGHT_DEFENSE_AREA;

			/// <summary> A list of shapes that together compose the extended defense area  </summary>
			static public IList<Geom> LEFT_EXTENDED_DEFENSE_AREA { get { InitializeIfNeeded(); return _LEFT_EXTENDED_DEFENSE_AREA; } } static volatile List<Geom> _LEFT_EXTENDED_DEFENSE_AREA;

			/// <summary> A list of shapes that together compose the defense area  </summary>
			static public IList<Geom> RIGHT_EXTENDED_DEFENSE_AREA { get { InitializeIfNeeded(); return _RIGHT_EXTENDED_DEFENSE_AREA; } } static volatile List<Geom> _RIGHT_EXTENDED_DEFENSE_AREA;


			static public void Reload()
			{
				double dx = (float)ConstantsRaw.get<double>("plays", "FIELD_WIDTH") / 2;
				double dy = (float)ConstantsRaw.get<double>("plays", "FIELD_HEIGHT") / 2;
				double dyg = (float)ConstantsRaw.get<double>("plays", "GOAL_HEIGHT") / 2;

				_TOP_LEFT = new Vector2(-dx, dy);
				_BOTTOM_LEFT = new Vector2(-dx, -dy);
				_TOP_RIGHT = new Vector2(dx, dy);
				_BOTTOM_RIGHT = new Vector2(dx, -dy);
				_TOP = new Vector2(0, dy);
				_BOTTOM = new Vector2(0, -dy);
				_LEFT = new Vector2(-dx, 0);
				_RIGHT = new Vector2(dx, 0);
				_CENTER = new Vector2(0, 0);

				_TOP_LEFT_QUAD =     (_TOP_LEFT     + _CENTER) / 2.0;
				_BOTTOM_LEFT_QUAD =  (_BOTTOM_LEFT  + _CENTER) / 2.0;
				_TOP_RIGHT_QUAD =    (_TOP_RIGHT    + _CENTER) / 2.0;
				_BOTTOM_RIGHT_QUAD = (_BOTTOM_RIGHT + _CENTER) / 2.0;
				_TOP_QUAD =          (_TOP          + _CENTER) / 2.0;
				_BOTTOM_QUAD =       (_BOTTOM       + _CENTER) / 2.0;
				_LEFT_QUAD =         (_LEFT         + _CENTER) / 2.0;
				_RIGHT_QUAD =        (_RIGHT        + _CENTER) / 2.0;

				_OUR_GOAL = new Vector2(-dx, 0);
				_OUR_GOAL_BOTTOM = new Vector2(-dx, -dyg);
				_OUR_GOAL_TOP = new Vector2(-dx, dyg);
				_THEIR_GOAL = new Vector2(dx, 0);
				_THEIR_GOAL_BOTTOM = new Vector2(dx, -dyg);
				_THEIR_GOAL_TOP = new Vector2(dx, dyg);

				double extwidth = ConstantsRaw.get<double>("plays", "EXTENDED_BORDER_WIDTH");
				_EXTENDED_TOP_LEFT = new Vector2(-dx - extwidth, dy + extwidth);
				_EXTENDED_BOTTOM_LEFT = new Vector2(-dx - extwidth, -dy - extwidth);
				_EXTENDED_TOP_RIGHT = new Vector2(dx + extwidth, dy + extwidth);
				_EXTENDED_BOTTOM_RIGHT = new Vector2(dx + extwidth, -dy - extwidth);

				double refandextwidth = extwidth + ConstantsRaw.get<double>("plays", "REFEREE_ZONE_WIDTH");
				_FULL_TOP_LEFT = new Vector2(-dx - refandextwidth, dy + refandextwidth);
				_FULL_BOTTOM_LEFT = new Vector2(-dx - refandextwidth, -dy - refandextwidth);
				_FULL_TOP_RIGHT = new Vector2(dx + refandextwidth, dy + refandextwidth);
				_FULL_BOTTOM_RIGHT = new Vector2(dx + refandextwidth, -dy - refandextwidth);

				List<Line> boundaryLines = new List<Line>();
				boundaryLines.Add(new Line(_TOP_RIGHT, _TOP_LEFT));
				boundaryLines.Add(new Line(_TOP_LEFT, _BOTTOM_LEFT));
				boundaryLines.Add(new Line(_BOTTOM_LEFT, _BOTTOM_RIGHT));
				boundaryLines.Add(new Line(_BOTTOM_RIGHT, _TOP_RIGHT));
				_BOUNDARY_LINES = boundaryLines;

				List<Line> extBoundaryLines = new List<Line>();
				extBoundaryLines.Add(new Line(_EXTENDED_TOP_RIGHT, _EXTENDED_TOP_LEFT));
				extBoundaryLines.Add(new Line(_EXTENDED_TOP_LEFT, _EXTENDED_BOTTOM_LEFT));
				extBoundaryLines.Add(new Line(_EXTENDED_BOTTOM_LEFT, _EXTENDED_BOTTOM_RIGHT));
				extBoundaryLines.Add(new Line(_EXTENDED_BOTTOM_RIGHT, _EXTENDED_TOP_RIGHT));
				_EXTENDED_BOUNDARY_LINES = extBoundaryLines;

				List<Line> fullBoundaryLines = new List<Line>();
				fullBoundaryLines.Add(new Line(_FULL_TOP_RIGHT, _FULL_TOP_LEFT));
				fullBoundaryLines.Add(new Line(_FULL_TOP_LEFT, _FULL_BOTTOM_LEFT));
				fullBoundaryLines.Add(new Line(_FULL_BOTTOM_LEFT, _FULL_BOTTOM_RIGHT));
				fullBoundaryLines.Add(new Line(_FULL_BOTTOM_RIGHT, _FULL_TOP_RIGHT));
				_FULL_BOUNDARY_LINES = fullBoundaryLines;

				_FIELD_RECT = new Rectangle(-dx, dx, -dy, dy);
				_EXTENDED_FIELD_RECT = new Rectangle(-dx - extwidth, dx + extwidth, -dy - extwidth, dy + extwidth);
				_FULL_FIELD_RECT = new Rectangle(-dx - refandextwidth, dx + refandextwidth, -dy - refandextwidth, dy + refandextwidth);

				double gw = ConstantsRaw.get<double>("plays", "GOAL_WIDTH");
				double gh = ConstantsRaw.get<double>("plays", "GOAL_HEIGHT");
				_LEFT_GOAL_BOX = new Rectangle(-dx - gw, -dx, -gh / 2, gh / 2);
				_RIGHT_GOAL_BOX = new Rectangle(dx, dx + gw, -gh / 2, gh / 2);

				double drh = ConstantsRaw.get<double>("plays", "DEFENSE_RECT_HEIGHT");
				double dr = ConstantsRaw.get<double>("plays", "DEFENSE_AREA_RADIUS");
				double edr = ConstantsRaw.get<double>("plays", "EXTENDED_DEFENSE_AREA_RADIUS");

				List<Geom> lda = new List<Geom>();
				List<Geom> rda = new List<Geom>();
				List<Geom> leda = new List<Geom>();
				List<Geom> reda = new List<Geom>();
				lda.Add(new Rectangle(-dx, -dx + dr, -drh / 2, drh / 2));
				rda.Add(new Rectangle(dx - dr, dx, -drh / 2, drh / 2));
				leda.Add(new Rectangle(-dx, -dx + edr, -drh / 2, drh / 2));
				reda.Add(new Rectangle(dx - edr, dx, -drh / 2, drh / 2));

				lda.Add(new Circle(-dx, -drh / 2, dr));
				rda.Add(new Circle(dx, -drh / 2, dr));
				leda.Add(new Circle(-dx, -drh / 2, edr));
				reda.Add(new Circle(dx, -drh / 2, edr));

				lda.Add(new Circle(-dx, drh / 2, dr));
				rda.Add(new Circle(dx, drh / 2, dr));
				leda.Add(new Circle(-dx, drh / 2, edr));
				reda.Add(new Circle(dx, drh / 2, edr));

				_LEFT_DEFENSE_AREA = lda;
				_RIGHT_DEFENSE_AREA = rda;
				_LEFT_EXTENDED_DEFENSE_AREA = leda;
				_RIGHT_EXTENDED_DEFENSE_AREA = reda;

			}
		}

		static public class Time
		{
			/// <summary> Frequency of strategy/interpreter loop </summary>
			static public double STRATEGY_FREQUENCY { get { InitializeIfNeeded(); return _STRATEGY_FREQUENCY; } } static volatile float _STRATEGY_FREQUENCY;

			/// <summary> Frequency of simulator engine </summary>
			static public double SIM_ENGINE_FREQUENCY { get { InitializeIfNeeded(); return _SIM_ENGINE_FREQUENCY; } } static volatile float _SIM_ENGINE_FREQUENCY;

			/// <summary> Frequency of robot control loop </summary>
			static public double CONTROL_LOOP_FREQUENCY { get { InitializeIfNeeded(); return _CONTROL_LOOP_FREQUENCY; } } static volatile float _CONTROL_LOOP_FREQUENCY;

			/// <summary> Frequency of averaging predictor combos </summary>
			static public double COMBINE_FREQUENCY { get { InitializeIfNeeded(); return _COMBINE_FREQUENCY; } } static volatile float _COMBINE_FREQUENCY;

			static public void Reload()
			{
				_STRATEGY_FREQUENCY = (float)ConstantsRaw.get<double>("default", "STRATEGY_FREQUENCY");
				_SIM_ENGINE_FREQUENCY = (float)ConstantsRaw.get<double>("default", "SIM_ENGINE_FREQUENCY");
				_CONTROL_LOOP_FREQUENCY = (float)ConstantsRaw.get<double>("default", "CONTROL_LOOP_FREQUENCY");
				_COMBINE_FREQUENCY = (float)ConstantsRaw.get<double>("default", "COMBINE_FREQUENCY");
			}
		}

		static public class Motion
		{
			/// <summary> Minimum allowed speed of wheel </summary>
			static public int WHEEL_SPEED_MIN { get { InitializeIfNeeded(); return _WHEEL_SPEED_MIN; } } static volatile int _WHEEL_SPEED_MIN;

			/// <summary> Maximum allowed speed of wheel </summary>
			static public int WHEEL_SPEED_MAX { get { InitializeIfNeeded(); return _WHEEL_SPEED_MAX; } } static volatile int _WHEEL_SPEED_MAX;

			/// <summary> TODO(davidwu): Add useful comment here. </summary>
			static public double ANGLE_AXIS_TO_WHEEL { get { InitializeIfNeeded(); return _ANGLE_AXIS_TO_WHEEL; } } static volatile float _ANGLE_AXIS_TO_WHEEL;

			/// <summary> Distance from center of robot to wheels. </summary>
			static public double WHEEL_RADIUS { get { InitializeIfNeeded(); return _WHEEL_RADIUS; } } static volatile float _WHEEL_RADIUS;

			/// <summary> Basic speed used by some planners and drivers, in m/s. </summary>
			static public double STEADY_STATE_SPEED { get { InitializeIfNeeded(); return _STEADY_STATE_SPEED; } } static volatile float _STEADY_STATE_SPEED;

			/// <summary> Distance from center of robot to wheels. </summary>
			static public string LOG_FILE { get { InitializeIfNeeded(); return _LOG_FILE; } } static volatile string _LOG_FILE;

			/// <summary> Distance from center of robot to wheels. </summary>
			static public int LOG_EVERY_MSEC { get { InitializeIfNeeded(); return _LOG_EVERY_MSEC; } } static volatile int _LOG_EVERY_MSEC;

			/// <summary> Global speed scaling for all robots. </summary>
			static public double SPEED_SCALING_FACTOR_ALL { get { InitializeIfNeeded(); return _SPEED_SCALING_FACTOR_ALL; } } static volatile float _SPEED_SCALING_FACTOR_ALL;

			/// <summary> Individual robot speed scaling. </summary>
			static public double[] SPEED_SCALING_FACTORS { get { InitializeIfNeeded(); return _SPEED_SCALING_FACTORS; } } static volatile double[] _SPEED_SCALING_FACTORS;

			/// <summary> How far away should the controller ask the planners to stay away from the ball?. </summary>
			static public double BALL_AVOID_DIST { get { InitializeIfNeeded(); return _BALL_AVOID_DIST; } } static volatile float _BALL_AVOID_DIST;

			/// <summary> If we're within this much of a destination and we're facing the right way, we're there. </summary>
			static public double MIN_DIST_TO_WP { get { InitializeIfNeeded(); return _MIN_DIST_TO_WP; } } static volatile float _MIN_DIST_TO_WP;

			/// <summary> If we're within this much of an orientation and we're in the right location, we're there. </summary>
			static public double MIN_ANGLE_DIFF_TO_WP { get { InitializeIfNeeded(); return _MIN_ANGLE_DIFF_TO_WP; } } static volatile float _MIN_ANGLE_DIFF_TO_WP;


			static public void Reload()
			{
				_WHEEL_SPEED_MAX = ConstantsRaw.get<int>("motionplanning", "MAX_SINGLE_WHEEL_SPEED");
				_WHEEL_SPEED_MIN = -_WHEEL_SPEED_MAX;

				_ANGLE_AXIS_TO_WHEEL = (float)((Math.PI/180.0) * ConstantsRaw.get<double>("motionplanning", "ANGLE_AXIS_TO_WHEEL"));
				_WHEEL_RADIUS = (float)ConstantsRaw.get<double>("motionplanning", "WHEEL_RADIUS");

				_STEADY_STATE_SPEED = (float)ConstantsRaw.get<double>("motionplanning", "STEADY_STATE_SPEED");
				_LOG_FILE = ConstantsRaw.get<string>("motionplanning", "LOG_FILE");
				_LOG_EVERY_MSEC = ConstantsRaw.get<int>("motionplanning", "LOG_EVERY_MSEC");

				_SPEED_SCALING_FACTOR_ALL = (float)ConstantsRaw.get<double>("motionplanning", "SPEED_SCALING_FACTOR_ALL");
				int numRobots = ConstantsRaw.get<int>("default", "NUM_ROBOTS");
				double[] factors = new double[numRobots];
				for (int i = 0; i < numRobots; i++)
					factors[i] = ConstantsRaw.get<double>("motionplanning", "SPEED_SCALING_FACTOR_" + i.ToString());
				_SPEED_SCALING_FACTORS = factors;

				_BALL_AVOID_DIST = (float)ConstantsRaw.get<double>("motionplanning", "BALL_AVOID_DIST");

				_MIN_DIST_TO_WP = (float)ConstantsRaw.get<double>("motionplanning", "MIN_DIST_TO_WP");
				_MIN_ANGLE_DIFF_TO_WP = (float)ConstantsRaw.get<double>("motionplanning", "MIN_ANGLE_DIFF_TO_WP");

				TBug.Reload();
				RRT.Reload();
			}

			/// <summary> Constants for TangentBug motion planner </summary>
			static public class TBug
			{
				static public double LOOK_AHEAD_DIST { get { InitializeIfNeeded(); return _LOOK_AHEAD_DIST; } } static volatile float _LOOK_AHEAD_DIST;
				static public double AVOID_DIST { get { InitializeIfNeeded(); return _AVOID_DIST; } } static volatile float _AVOID_DIST;
				static public double WAYPOINT_DIST { get { InitializeIfNeeded(); return _WAYPOINT_DIST; } } static volatile float _WAYPOINT_DIST;
				static public double MIN_ABS_VAL_STICK { get { InitializeIfNeeded(); return _MIN_ABS_VAL_STICK; } } static volatile float _MIN_ABS_VAL_STICK;
				static public double EXTRA_GOAL_DIST { get { InitializeIfNeeded(); return _EXTRA_GOAL_DIST; } } static volatile float _EXTRA_GOAL_DIST;
				static public double BOUNDARY_AVOID { get { InitializeIfNeeded(); return _BOUNDARY_AVOID; } } static volatile float _BOUNDARY_AVOID;

				static public void Reload()
				{
					_LOOK_AHEAD_DIST = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_LOOK_AHEAD_DIST");
					_AVOID_DIST = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_AVOID_DIST");
					_WAYPOINT_DIST = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_WAYPOINT_DIST");
					_MIN_ABS_VAL_STICK = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_MIN_ABS_VAL_STICK");
					_EXTRA_GOAL_DIST = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_EXTRA_GOAL_DIST");
					_BOUNDARY_AVOID = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_BOUNDARY_AVOID");
				}
			}

			/// <summary> Constants for plain RRT motion planner </summary>
			static public class RRT
			{
				static public double OBSTACLE_AVOID_DIST { get { InitializeIfNeeded(); return _OBSTACLE_AVOID_DIST; } } static volatile float _OBSTACLE_AVOID_DIST;

				static public void Reload()
				{
					_OBSTACLE_AVOID_DIST = (float)ConstantsRaw.get<double>("motionplanning", "RRT_OBSTACLE_AVOID_DIST");
				}
			}

		}

		static public class PlayFiles
		{
			/// <summary>  </summary>
			static public bool USE_C_SHARP_PLAY_SYSTEM { get { InitializeIfNeeded(); return _USE_C_SHARP_PLAY_SYSTEM; } } static volatile bool _USE_C_SHARP_PLAY_SYSTEM;

			/// <summary>  </summary>
			static public string TACTIC_DIR { get { InitializeIfNeeded(); return _TACTIC_DIR; } } static volatile string _TACTIC_DIR;

			/// <summary>  </summary>
			static public string PLAY_DIR_YELLOW { get { InitializeIfNeeded(); return _PLAY_DIR_YELLOW; } } static volatile string _PLAY_DIR_YELLOW;

			/// <summary>  </summary>
			static public string PLAY_DIR_BLUE { get { InitializeIfNeeded(); return _PLAY_DIR_BLUE; } } static volatile string _PLAY_DIR_BLUE;

			static public void Reload()
			{
				_USE_C_SHARP_PLAY_SYSTEM = ConstantsRaw.get<bool>("default", "USE_C_SHARP_PLAY_SYSTEM");
				_TACTIC_DIR = ConstantsRaw.get<string>("default", "TACTIC_DIR");
				_PLAY_DIR_YELLOW = ConstantsRaw.get<string>("default", "PLAY_DIR_YELLOW");
				_PLAY_DIR_BLUE = ConstantsRaw.get<string>("default", "PLAY_DIR_BLUE");

			}
		}

		static public class Predictor
		{
			/// <summary>  </summary>
			static public bool FLIP_COORDINATES { get { InitializeIfNeeded(); return _FLIP_COORDINATES; } } static volatile bool _FLIP_COORDINATES;

			/// <summary>  </summary>
			static public double DELTA_DIST_SQ_MERGE { get { InitializeIfNeeded(); return _DELTA_DIST_SQ_MERGE; } } static volatile float _DELTA_DIST_SQ_MERGE;

			/// <summary>  </summary>
			static public double MAX_SECONDS_TO_KEEP_INFO { get { InitializeIfNeeded(); return _MAX_SECONDS_TO_KEEP_INFO; } } static volatile float _MAX_SECONDS_TO_KEEP_INFO;

			/// <summary>  </summary>
			static public double MAX_SECONDS_TO_KEEP_BALL { get { InitializeIfNeeded(); return _MAX_SECONDS_TO_KEEP_BALL; } } static volatile float _MAX_SECONDS_TO_KEEP_BALL;

			/// <summary>  </summary>
			static public double VELOCITY_DT { get { InitializeIfNeeded(); return _VELOCITY_DT; } } static volatile float _VELOCITY_DT;

			/// <summary>  </summary>
			static public double VELOCITY_WEIGHT_OLD { get { InitializeIfNeeded(); return _VELOCITY_WEIGHT_OLD; } } static volatile float _VELOCITY_WEIGHT_OLD;

			/// <summary>  </summary>
			static public double VELOCITY_WEIGHT_NEW { get { InitializeIfNeeded(); return _VELOCITY_WEIGHT_NEW; } } static volatile float _VELOCITY_WEIGHT_NEW;

			/// <summary>  </summary>
			static public double POSITION_WEIGHT_OLD { get { InitializeIfNeeded(); return _POSITION_WEIGHT_OLD; } } static volatile float _POSITION_WEIGHT_OLD;

			/// <summary>  </summary>
			static public double POSITION_WEIGHT_NEW { get { InitializeIfNeeded(); return _POSITION_WEIGHT_NEW; } } static volatile float _POSITION_WEIGHT_NEW;

			/// <summary>  </summary>
			static public double BALL_POSITION_WEIGHT_OLD { get { InitializeIfNeeded(); return _BALL_POSITION_WEIGHT_OLD; } } static volatile float _BALL_POSITION_WEIGHT_OLD;

			/// <summary>  </summary>
			static public double BALL_POSITION_WEIGHT_NEW { get { InitializeIfNeeded(); return _BALL_POSITION_WEIGHT_NEW; } } static volatile float _BALL_POSITION_WEIGHT_NEW;

			/// <summary>  </summary>
			static public double WEIGHT_OLD { get { InitializeIfNeeded(); return _WEIGHT_OLD; } } static volatile float _WEIGHT_OLD;

			/// <summary>  </summary>
			static public double WEIGHT_NEW { get { InitializeIfNeeded(); return _WEIGHT_NEW; } } static volatile float _WEIGHT_NEW;


			static public void Reload()
			{
				_FLIP_COORDINATES = ConstantsRaw.get<bool>("default", "FLIP_COORDINATES");
				_DELTA_DIST_SQ_MERGE = (float)ConstantsRaw.get<double>("default", "DELTA_DIST_SQ_MERGE");
				_MAX_SECONDS_TO_KEEP_INFO = (float)ConstantsRaw.get<double>("default", "MAX_SECONDS_TO_KEEP_INFO");
				_MAX_SECONDS_TO_KEEP_BALL = (float)ConstantsRaw.get<double>("default", "MAX_SECONDS_TO_KEEP_BALL");
				_VELOCITY_DT = (float)ConstantsRaw.get<double>("default", "VELOCITY_DT");
				_VELOCITY_WEIGHT_OLD = (float)ConstantsRaw.get<double>("default", "VELOCITY_WEIGHT_OLD");
				_VELOCITY_WEIGHT_NEW = (float)ConstantsRaw.get<double>("default", "VELOCITY_WEIGHT_NEW");
				_POSITION_WEIGHT_OLD = (float)ConstantsRaw.get<double>("default", "POSITION_WEIGHT_OLD");
				_POSITION_WEIGHT_NEW = (float)ConstantsRaw.get<double>("default", "POSITION_WEIGHT_NEW");
				_BALL_POSITION_WEIGHT_OLD = (float)ConstantsRaw.get<double>("default", "BALL_POSITION_WEIGHT_OLD");
				_BALL_POSITION_WEIGHT_NEW = (float)ConstantsRaw.get<double>("default", "BALL_POSITION_WEIGHT_NEW");
				_WEIGHT_OLD = (float)ConstantsRaw.get<double>("default", "WEIGHT_OLD");
				_WEIGHT_NEW = (float)ConstantsRaw.get<double>("default", "WEIGHT_NEW");
			}
		}

		static public class RobotInfo
		{
			/// <summary> Does this robot have a working kicker? </summary>
			static public bool HAS_KICKER(int i) { InitializeIfNeeded(); return _HAS_KICKER[i]; } static volatile bool[] _HAS_KICKER;

			/// <summary> Is this robot a goalie? </summary>
			static public bool IS_GOALIE(int i) { InitializeIfNeeded(); return _IS_GOALIE[i]; } static volatile bool[] _IS_GOALIE;

			static public void Reload()
			{
				int numRobots = ConstantsRaw.get<int>("default", "NUM_ROBOTS");
				bool[] hasKicker = new bool[numRobots];
				bool[] isGoalie = new bool[numRobots];
				for (int i = 0; i < numRobots; i++)
				{
					hasKicker[i] = true;
					isGoalie[i] = false;

					bool value;
					if (ConstantsRaw.tryGet("default", "ROBOT_HAS_KICKER_" + i, out value))
						hasKicker[i] = value;
					if (ConstantsRaw.tryGet("default", "ROBOT_IS_GOALIE_" + i, out value))
						isGoalie[i] = value;
				}
				_HAS_KICKER = hasKicker;
				_IS_GOALIE = isGoalie;
			}
		}

		static public class Plays
		{
			/// <summary> Delta distance for deciding that ball has moved. </summary>
			static public double BALL_MOVED_DIST { get { InitializeIfNeeded(); return _BALL_MOVED_DIST; } } static volatile float _BALL_MOVED_DIST;

			static public void Reload()
			{
				_BALL_MOVED_DIST = (float)ConstantsRaw.get<double>("plays", "BALL_MOVED_DIST");
			}
		}

		static public class RadioProtocol
		{
			/// <summary> Calculate a checksum when sending commands to the brushless boards. </summary>
			static public bool SEND_BRUSHLESSBOARD_CHECKSUM { get { InitializeIfNeeded(); return _SEND_BRUSHLESSBOARD_CHECKSUM; } } static volatile bool _SEND_BRUSHLESSBOARD_CHECKSUM;

			/// <summary> Calculate a checksum when sending commands to the aux kicker boards. </summary>
			static public bool SEND_AUXBOARD_CHECKSUM { get { InitializeIfNeeded(); return _SEND_AUXBOARD_CHECKSUM; } } static volatile bool _SEND_AUXBOARD_CHECKSUM;

			static public void Reload()
			{
				_SEND_BRUSHLESSBOARD_CHECKSUM = ConstantsRaw.get<bool>("default", "SEND_BRUSHLESSBOARD_CHECKSUM");
				_SEND_AUXBOARD_CHECKSUM = ConstantsRaw.get<bool>("default", "SEND_AUXBOARD_CHECKSUM");
			}
		}

		//INITIALIZATION AND RELOAD MECHANISM---------------------------------------------------------

		static Object _lock = new Object();
		static volatile bool _is_initialized = false;
		static volatile bool _is_reloading = false;
		static void InitializeIfNeeded()
		{
			//Fast check first, if it fails, then we lock and check again, for thread-safety
			if (!_is_initialized)
			{
				lock (_lock)
				{
					if (!_is_initialized)
					{
						Reload();
						_is_initialized = true;
					}
				}
			}
		}

		public static void Reload()
		{
			lock (_lock)
			{
				if (_is_reloading)
				{
					Console.WriteLine("Somehow we made it in to Constants.reload() when we are already reloading!");
					Console.WriteLine("THIS IS EXTREMELY BAD!");
					throw new Exception("Somehow we made it in to Constants.reload() when we are already reloading!");
				}
				_is_reloading = true;
				Basic.Reload();
				Field.Reload();
				FieldPts.Reload();
				Time.Reload();
				Motion.Reload();
				PlayFiles.Reload();
				Predictor.Reload();
				RobotInfo.Reload();
				Plays.Reload();
				RadioProtocol.Reload();
				_is_reloading = false;
			}
		}
	}




	static public class ConstantsRaw
	{
		//INTERFACE-------------------------------------------------------------------------------------

		/// <summary>
		/// The directory in which we'll look for the constants files.
		/// </summary>
		const string directory = "../../../constants/";

		/// <summary>
		/// Each entry in this dictionary is a map from a name (such as "default" or "vision")
		/// to the set of constants in that category ("BLOBSIZE = 1000")
		/// </summary>
		static private Dictionary<string, Dictionary<string, object>> dictionaries = 
			new Dictionary<string, Dictionary<string, object>>();

		/// <summary>
		/// Reloads all constants files that have been loaded so far.  (constants files get loaded the first time they are used)
		/// </summary>
		static public void Load()
		{
			lock (dictionaries)
			{
				List<string> categories = new List<string>(dictionaries.Keys);
				foreach (string category in categories)
					LoadFromFile(category);
			}
		}

		/// <summary>
		/// A helper function that will convert things like "int","5" to the integer 5.
		/// </summary>
		static private object convert(string type, string s)
		{
			switch (type)
			{
				case "int":    return int.Parse(s);
				case "string": return s;
				case "float":  return float.Parse(s);
				case "double": return double.Parse(s);
				case "bool":   return bool.Parse(s);
				default:
				throw new ApplicationException("Unhandled type: \"" + type + "\"");
			}
		}

		/// <summary>
		/// Loads the given constants file into memory if they have not yet been
		/// </summary>
		/// <param name="category">The category to load.  This is not the pathname, but the name of the constants category
		/// (ie "default" instead of "C:\default.txt")</param>
		static private void LoadFromFileIfNeeded(string category)
		{
			lock (dictionaries)
			{
				if (!dictionaries.ContainsKey(category) || dictionaries[category].Count == 0)
					LoadFromFile(category);
			}
		}

		/// <summary>
		/// Loads the given constants file into memory
		/// </summary>
		/// <param name="category">The category to load.  This is not the pathname, but the name of the constants category
		/// (ie "default" instead of "C:\default.txt")</param>
		static private void LoadFromFile(string category)
		{
			lock(dictionaries)
			{
				string fname = directory + category + ".txt";
				dictionaries.Remove(category);
				Dictionary<string, object> dict = new Dictionary<string, object>();
				dictionaries.Add(category, dict);

				if (!File.Exists(fname))
					throw new ApplicationException("sorry, could not find the constants file \""+category+"\", looked in "+fname);
				StreamReader reader = new StreamReader(fname);
				while (!reader.EndOfStream)
				{
					string s = reader.ReadLine();
					if (s == null)
						break;

					//Remove comments (if we find a # anywhere on the line, remove all chars after it)
					int commentIndex = s.IndexOf('#');
					if (commentIndex >= 0)
						s = s.Remove(commentIndex);

					//Trim whitespace
					s = s.Trim();

					if (s.Length == 0)
						continue;

					string[] strings = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					//format is:
					//type name value   
					try {
						dict.Add(strings[1], convert(strings[0], string.Join(" ", strings, 2, strings.Length - 2)));
					} catch (ArgumentException e) {
						reader.Close();
						throw e;
					}
				}
				reader.Close();
			}
		}

		/// <summary>
		/// Tries to get the value of a constant.  If the constant exists, returns the value,
		/// otherwise throws an exception.
		/// (If you don't want it to throw an exception, check nondestructiveGet)
		/// </summary>
		/// <typeparam name="T">The type of object to get</typeparam>
		/// <param name="category">The category in which to look for the constant</param>
		/// <param name="name">The name of the constant</param>
		/// <returns>The value of the constant</returns>
		static public T get<T>(string category, string name)
		{
			lock (dictionaries)
			{
				LoadFromFileIfNeeded(category);

				object val;
				bool worked = dictionaries[category].TryGetValue(name, out val);
				if (!worked)
					throw new ApplicationException("tried to get an unknown variable called \"" + name + "\" in category \"" + category + "\"");

				if (typeof(T) != val.GetType())
					throw new ApplicationException("you asked for a different type of variable than is stored\nGiven: " + typeof(T).Name + ", Stored: " + val.GetType().Name);

				return (T)val;
			}
		}

		/// <summary>
		/// Tries to get the value of a constant; if it exists, returns true and puts the value in val, otherwise returns false.
		/// </summary>
		/// <typeparam name="T">The type of object to get</typeparam>
		/// <param name="category">The category in which to look for the constant</param>
		/// <param name="name">The name of the constant</param>
		/// <param name="val">The variable that will have the value loaded into</param>
		/// <returns>Whether or not the constant exists</returns>
		static public bool tryGet<T>(string category, string name, out T val)
		{
			lock (dictionaries)
			{
				LoadFromFileIfNeeded(category);

				object getter;
				if (dictionaries[category].TryGetValue(name, out getter))
				{
					if (typeof(T) != getter.GetType())
						throw new ApplicationException("you asked for a different type of variable than is stored\nGiven: " + typeof(T).Name + ", Stored: " + getter.GetType().Name);

					val = (T)getter;
					return true;
				}
				else
				{
					val = default(T);
					return false;
				}
			}
		}

		/// <summary>
		/// Gets whether or not the specified constant has been defined
		/// </summary>
		/// <param name="name">The name of the constant</param>
		/// <returns>Whether or not the constant exists</returns>
		//TODO(davidwu): This is wrong. It checks whether a category is defined, not whether a constant is defined!
		static public bool isDefined(string name)
		{
			lock (dictionaries)
			{
				return dictionaries.ContainsKey(name);
			}
		}
	}
}
