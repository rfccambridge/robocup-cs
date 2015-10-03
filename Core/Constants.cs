using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RFC.Geometry;

namespace RFC.Core
{
	public static class Constants
	{
		//NOTE: Values are stored as float because doubles can't be volatile!

		//CONSTANTS------------------------------------------------------------------------------------
		public class BasicType
		{
			/// <summary> Number of robots and/or robot ids possible </summary>
			volatile int _NUM_ROBOTS; public int NUM_ROBOTS => _NUM_ROBOTS;

			/// <summary> Radius of a robot </summary>
			volatile float _ROBOT_RADIUS; public double ROBOT_RADIUS => _ROBOT_RADIUS;

			/// <summary> Distance from the center to the front of a robot </summary>
			volatile float _ROBOT_FRONT_RADIUS; public double ROBOT_FRONT_RADIUS => _ROBOT_FRONT_RADIUS;

			/// <summary> Radius of the ball </summary>
			volatile float _BALL_RADIUS; public double BALL_RADIUS => _BALL_RADIUS;

			internal BasicType()
			{
				_NUM_ROBOTS = ConstantsRaw.get<int>("default", "NUM_ROBOTS");
				_ROBOT_RADIUS = (float)ConstantsRaw.get<double>("default", "ROBOT_RADIUS");
				_ROBOT_FRONT_RADIUS = (float)ConstantsRaw.get<double>("default", "ROBOT_FRONT_RADIUS");
				_BALL_RADIUS = (float)ConstantsRaw.get<double>("default", "BALL_RADIUS");
			}
		}
		public static volatile BasicType Basic;

		public class FieldType
		{
			//SIZES-----------------------------------------------------------

			/// <summary> Width of the in-bounds part of the field </summary>
			volatile float _WIDTH; public double WIDTH => _WIDTH;

			/// <summary> Height of the in-bounds part of the field </summary>
			volatile float _HEIGHT; public double HEIGHT => _HEIGHT;

			/// <summary> Width of field including extra out-of-bounds zone for robot movement </summary>
			volatile float _EXTENDED_WIDTH; public double EXTENDED_WIDTH => _EXTENDED_WIDTH;

			/// <summary> Height of field including extra out-of-bounds zone for robot movement  </summary>
			volatile float _EXTENDED_HEIGHT; public double EXTENDED_HEIGHT => _EXTENDED_HEIGHT;

			/// <summary> Width of field including ref zone </summary>
			volatile float _FULL_WIDTH; public double FULL_WIDTH => _FULL_WIDTH;

			/// <summary> Height of field including ref zone </summary>
			volatile float _FULL_HEIGHT; public double FULL_HEIGHT => _FULL_HEIGHT;

			/// <summary> Width of the extra out-of-bounds zone for robot movement </summary>
			volatile float _EXTENDED_BORDER_WIDTH; public double EXTENDED_BORDER_WIDTH => _EXTENDED_BORDER_WIDTH;

			/// <summary> Width of the referree zone </summary>
			volatile float _REFEREE_WIDTH; public double REFEREE_WIDTH => _REFEREE_WIDTH;

			/// <summary> Height of the rectangular part of the defense area on each side </summary>
			volatile float _DEFENSE_RECT_HEIGHT; public double DEFENSE_RECT_HEIGHT => _DEFENSE_RECT_HEIGHT;

			/// <summary> Radius and rectangular width of the defense area on each side </summary>
			volatile float _DEFENSE_AREA_RADIUS; public double DEFENSE_AREA_RADIUS => _DEFENSE_AREA_RADIUS;

			/// <summary> Extended defense area radius that kicking-team robots must stay out of when positioning for free kick </summary>
			volatile float _EXTENDED_DEFENSE_AREA_RADIUS; public double EXTENDED_DEFENSE_AREA_RADIUS => _EXTENDED_DEFENSE_AREA_RADIUS;

			//BOUNDS----------------------------------------------------------

			/// <summary> X coord of the left boundary line </summary>
			volatile float _XMIN; public double XMIN => _XMIN;

			/// <summary> X coord of the right boundary line </summary>
			volatile float _XMAX; public double XMAX => _XMAX;

			/// <summary> Y coord of the bottom boundary line </summary>
			volatile float _YMIN; public double YMIN => _YMIN;

			/// <summary> Y coord of the top boundary line </summary>
			volatile float _YMAX; public double YMAX => _YMAX;

			/// <summary> X coord of the left boundary including the extra out-of-bounds zone for robot movement </summary>
			volatile float _EXTENDED_XMIN; public double EXTENDED_XMIN => _EXTENDED_XMIN;

			/// <summary> X coord of the right boundary including the extra out-of-bounds zone for robot movement </summary>
			volatile float _EXTENDED_XMAX; public double EXTENDED_XMAX => _EXTENDED_XMAX;

			/// <summary> Y coord of the bottom boundary including the extra out-of-bounds zone for robot movement </summary>
			volatile float _EXTENDED_YMIN; public double EXTENDED_YMIN => _EXTENDED_YMIN;

			/// <summary> Y coord of the top boundary including the extra out-of-bounds zone for robot movement </summary>
			volatile float _EXTENDED_YMAX; public double EXTENDED_YMAX => _EXTENDED_YMAX;

			/// <summary> X coord of the left boundary including the ref zone </summary>
			volatile float _FULL_XMIN; public double FULL_XMIN => _FULL_XMIN;

			/// <summary> X coord of the right boundary including the ref zone </summary>
			volatile float _FULL_XMAX; public double FULL_XMAX => _FULL_XMAX;

			/// <summary> Y coord of the bottom boundary including the ref zone </summary>
			volatile float _FULL_YMIN; public double FULL_YMIN => _FULL_YMIN;

			/// <summary> Y coord of the top boundary including the ref zone </summary>
			volatile float _FULL_YMAX; public double FULL_YMAX => _FULL_YMAX;

			//GOAL--------------------------------------------------------------

			/// <summary> Width of the goal zone </summary>
			volatile float _GOAL_WIDTH; public double GOAL_WIDTH => _GOAL_WIDTH;

			/// <summary> Height of the goal zone </summary>
			volatile float _GOAL_HEIGHT; public double GOAL_HEIGHT => _GOAL_HEIGHT;

			/// <summary> Y coord of the bottom of goal zone </summary>
			volatile float _GOAL_YMIN; public double GOAL_YMIN => _GOAL_YMIN;

			/// <summary> Y coord of the top of goal zone </summary>
			volatile float _GOAL_YMAX; public double GOAL_YMAX => _GOAL_YMAX;

			/// <summary> X coord of the left of the LEFT goal zone </summary>
			volatile float _GOAL_XMIN; public double GOAL_XMIN => _GOAL_XMIN;

			/// <summary> X coord of the right of the RIGHT goal zone </summary>
			volatile float _GOAL_XMAX; public double GOAL_XMAX => _GOAL_XMAX;

			//OTHER----------------------------------------------------------

			/// <summary> Radius of the center circle of the field </summary>
			volatile float _CENTER_CIRCLE_RADIUS; public double CENTER_CIRCLE_RADIUS => _CENTER_CIRCLE_RADIUS;

			//--------------------------------------------------------------

			internal FieldType()
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
		public static volatile FieldType Field;

		public class FieldPtsType
		{
			//BASIC LOCATIONS----------------------------------------------------------------------

			/// <summary> Top left corner of field </summary>
			volatile Vector2 _TOP_LEFT; public Vector2 TOP_LEFT => _TOP_LEFT;

			/// <summary> Bottom left corner of field </summary>
			volatile Vector2 _BOTTOM_LEFT; public Vector2 BOTTOM_LEFT => _BOTTOM_LEFT;

			/// <summary> Top right corner of field </summary>
			volatile Vector2 _TOP_RIGHT; public Vector2 TOP_RIGHT => _TOP_RIGHT;

			/// <summary> Bottom right corner of field </summary>
			volatile Vector2 _BOTTOM_RIGHT; public Vector2 BOTTOM_RIGHT => _BOTTOM_RIGHT;

			/// <summary> Top center edge of field </summary>
			volatile Vector2 _TOP; public Vector2 TOP => _TOP;

			/// <summary> Bottom center edge of field </summary>
			volatile Vector2 _BOTTOM; public Vector2 BOTTOM => _BOTTOM;

			/// <summary> Left center edge of field </summary>
			volatile Vector2 _LEFT; public Vector2 LEFT => _LEFT;

			/// <summary> Right center edge of field </summary>
			volatile Vector2 _RIGHT; public Vector2 RIGHT => _RIGHT;

			/// <summary> Center of field </summary>
			volatile Vector2 _CENTER; public Vector2 CENTER => _CENTER;


			//QUADRANTS (octants, really)-----------------------------------------------------------------

			/// <summary> Halfway in between center and top left corner </summary>
			volatile Vector2 _TOP_LEFT_QUAD; public Vector2 TOP_LEFT_QUAD => _TOP_LEFT_QUAD;

			/// <summary> Halfway in between center and top left corner </summary>
			volatile Vector2 _BOTTOM_LEFT_QUAD; public Vector2 BOTTOM_LEFT_QUAD => _BOTTOM_LEFT_QUAD;

			/// <summary> Halfway in between center and bottom right corner  </summary>
			volatile Vector2 _TOP_RIGHT_QUAD; public Vector2 TOP_RIGHT_QUAD => _TOP_RIGHT_QUAD;

			/// <summary> Halfway in between center and bottom right corner </summary>
			volatile Vector2 _BOTTOM_RIGHT_QUAD; public Vector2 BOTTOM_RIGHT_QUAD => _BOTTOM_RIGHT_QUAD;

			/// <summary> Halfway in between center and top edge </summary>
			volatile Vector2 _TOP_QUAD; public Vector2 TOP_QUAD => _TOP_QUAD;

			/// <summary> Halfway in between center and bottom edge </summary>
			volatile Vector2 _BOTTOM_QUAD; public Vector2 BOTTOM_QUAD => _BOTTOM_QUAD;

			/// <summary> Halfway in between center and left edge </summary>
			volatile Vector2 _LEFT_QUAD; public Vector2 LEFT_QUAD => _LEFT_QUAD;

			/// <summary> Halfway in between center and right edge </summary>
			volatile Vector2 _RIGHT_QUAD; public Vector2 RIGHT_QUAD => _RIGHT_QUAD;


			//GOAL---------------------------------------------------------

			/// <summary> Center location of the opening of our goal, same as LEFT </summary>
			volatile Vector2 _OUR_GOAL; public Vector2 OUR_GOAL => _OUR_GOAL;

			/// <summary> Bottom point of our goal </summary>
			volatile Vector2 _OUR_GOAL_BOTTOM; public Vector2 OUR_GOAL_BOTTOM => _OUR_GOAL_BOTTOM;

			/// <summary> Top point of our goal </summary>
			volatile Vector2 _OUR_GOAL_TOP; public Vector2 OUR_GOAL_TOP => _OUR_GOAL_TOP;

			/// <summary> Center location of the opening of their goal, same as RIGHT </summary>
			volatile Vector2 _THEIR_GOAL; public Vector2 THEIR_GOAL => _THEIR_GOAL;

			/// <summary> Bottom point of their goal </summary>
			volatile Vector2 _THEIR_GOAL_BOTTOM; public Vector2 THEIR_GOAL_BOTTOM => _THEIR_GOAL_BOTTOM;

			/// <summary> Top point of their goal </summary>
			volatile Vector2 _THEIR_GOAL_TOP; public Vector2 THEIR_GOAL_TOP => _THEIR_GOAL_TOP;

			/// <summary> Their PenaltyKick Mark </summary>
			volatile Vector2 _THEIR_PENALTY_KICK_MARK; public Vector2 THEIR_PENALTY_KICK_MARK => _THEIR_PENALTY_KICK_MARK;

			/// <summary> Our PenaltyKick Mark </summary>
			volatile Vector2 _OUR_PENALTY_KICK_MARK; public Vector2 OUR_PENALTY_KICK_MARK => _OUR_PENALTY_KICK_MARK;

			//EXTENDED BOUNDS---------------------------------------------------------

			/// <summary> Top left corner of field including ref zone </summary>
			volatile Vector2 _EXTENDED_TOP_LEFT; public Vector2 EXTENDED_TOP_LEFT => _EXTENDED_TOP_LEFT;

			/// <summary> Bottom left corner of field including ref zone </summary>
			volatile Vector2 _EXTENDED_BOTTOM_LEFT; public Vector2 EXTENDED_BOTTOM_LEFT => _EXTENDED_BOTTOM_LEFT;

			/// <summary> Top right corner of field including ref zone </summary>
			volatile Vector2 _EXTENDED_TOP_RIGHT; public Vector2 EXTENDED_TOP_RIGHT => _EXTENDED_TOP_RIGHT;

			/// <summary> Bottom right corner of field including ref zone </summary>
			volatile Vector2 _EXTENDED_BOTTOM_RIGHT; public Vector2 EXTENDED_BOTTOM_RIGHT => _EXTENDED_BOTTOM_RIGHT;


			//FULL BOUNDS---------------------------------------------------------

			/// <summary> Top left corner of field including ref zone </summary>
			volatile Vector2 _FULL_TOP_LEFT; public Vector2 FULL_TOP_LEFT => _FULL_TOP_LEFT;

			/// <summary> Bottom left corner of field including ref zone </summary>
			volatile Vector2 _FULL_BOTTOM_LEFT; public Vector2 FULL_BOTTOM_LEFT => _FULL_BOTTOM_LEFT;

			/// <summary> Top right corner of field including ref zone </summary>
			volatile Vector2 _FULL_TOP_RIGHT; public Vector2 FULL_TOP_RIGHT => _FULL_TOP_RIGHT;

			/// <summary> Bottom right corner of field including ref zone </summary>
			volatile Vector2 _FULL_BOTTOM_RIGHT; public Vector2 FULL_BOTTOM_RIGHT => _FULL_BOTTOM_RIGHT;

			//LINES------------------------------------------------------------------------------

			/// <summary> Boundary lines of field </summary>
			volatile IList<Line> _BOUNDARY_LINES; public IList<Line> BOUNDARY_LINES => _BOUNDARY_LINES;

			/// <summary> Boundary lines of field including extended out-of-bounds zone for robot movement </summary>
			volatile IList<Line> _EXTENDED_BOUNDARY_LINES; public IList<Line> EXTENDED_BOUNDARY_LINES => _EXTENDED_BOUNDARY_LINES;

			/// <summary> Boundary lines of field including ref zone </summary>
			volatile IList<Line> _FULL_BOUNDARY_LINES; public IList<Line> FULL_BOUNDARY_LINES => _FULL_BOUNDARY_LINES;

			//RECTANGLES-------------------------------------------------------------------------

			/// <summary> Boundary rectangle of field </summary>
			volatile Rectangle _FIELD_RECT; public Rectangle FIELD_RECT => _FIELD_RECT;

			/// <summary> Boundary rectangle of field including extended out-of-bounds zone for robot movement </summary>
			volatile Rectangle _EXTENDED_FIELD_RECT; public Rectangle EXTENDED_FIELD_RECT => _EXTENDED_FIELD_RECT;

			/// <summary> Boundary rectangle of field including ref zone </summary>
			volatile Rectangle _FULL_FIELD_RECT; public Rectangle FULL_FIELD_RECT => _FULL_FIELD_RECT;

			/// <summary> Boundary rectangle of goal box </summary>
			volatile Rectangle _LEFT_GOAL_BOX; public Rectangle LEFT_GOAL_BOX => _LEFT_GOAL_BOX;

			/// <summary> Boundary rectangle of goal box </summary>
			volatile Rectangle _RIGHT_GOAL_BOX; public Rectangle RIGHT_GOAL_BOX => _RIGHT_GOAL_BOX;

			//OTHER------------------------------------------------------------------------------
			/// <summary> A list of shapes that together compose the defense area  </summary>
			volatile IList<AreaGeom> _LEFT_DEFENSE_AREA; public IList<AreaGeom> LEFT_DEFENSE_AREA => _LEFT_DEFENSE_AREA;

			/// <summary> A list of shapes that together compose the defense area  </summary>
			volatile IList<AreaGeom> _RIGHT_DEFENSE_AREA; public IList<AreaGeom> RIGHT_DEFENSE_AREA => _RIGHT_DEFENSE_AREA;

			/// <summary> A list of shapes that together compose the extended defense area  </summary>
			volatile IList<AreaGeom> _LEFT_EXTENDED_DEFENSE_AREA; public IList<AreaGeom> LEFT_EXTENDED_DEFENSE_AREA => _LEFT_EXTENDED_DEFENSE_AREA;

			/// <summary> A list of shapes that together compose the defense area  </summary>
			volatile IList<AreaGeom> _RIGHT_EXTENDED_DEFENSE_AREA; public IList<AreaGeom> RIGHT_EXTENDED_DEFENSE_AREA => _RIGHT_EXTENDED_DEFENSE_AREA;


			internal FieldPtsType()
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

				_THEIR_PENALTY_KICK_MARK = _THEIR_GOAL - new Vector2(.75,0);
				_OUR_PENALTY_KICK_MARK = _OUR_GOAL + new Vector2(.75,0);

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
				_BOUNDARY_LINES = boundaryLines.AsReadOnly();

				List<Line> extBoundaryLines = new List<Line>();
				extBoundaryLines.Add(new Line(_EXTENDED_TOP_RIGHT, _EXTENDED_TOP_LEFT));
				extBoundaryLines.Add(new Line(_EXTENDED_TOP_LEFT, _EXTENDED_BOTTOM_LEFT));
				extBoundaryLines.Add(new Line(_EXTENDED_BOTTOM_LEFT, _EXTENDED_BOTTOM_RIGHT));
				extBoundaryLines.Add(new Line(_EXTENDED_BOTTOM_RIGHT, _EXTENDED_TOP_RIGHT));
				_EXTENDED_BOUNDARY_LINES = extBoundaryLines.AsReadOnly();

				List<Line> fullBoundaryLines = new List<Line>();
				fullBoundaryLines.Add(new Line(_FULL_TOP_RIGHT, _FULL_TOP_LEFT));
				fullBoundaryLines.Add(new Line(_FULL_TOP_LEFT, _FULL_BOTTOM_LEFT));
				fullBoundaryLines.Add(new Line(_FULL_BOTTOM_LEFT, _FULL_BOTTOM_RIGHT));
				fullBoundaryLines.Add(new Line(_FULL_BOTTOM_RIGHT, _FULL_TOP_RIGHT));
				_FULL_BOUNDARY_LINES = fullBoundaryLines.AsReadOnly();

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

				List<AreaGeom> lda = new List<AreaGeom>();
				List<AreaGeom> rda = new List<AreaGeom>();
				List<AreaGeom> leda = new List<AreaGeom>();
				List<AreaGeom> reda = new List<AreaGeom>();
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

				_LEFT_DEFENSE_AREA = lda.AsReadOnly();
				_RIGHT_DEFENSE_AREA = rda.AsReadOnly();
				_LEFT_EXTENDED_DEFENSE_AREA = leda.AsReadOnly();
				_RIGHT_EXTENDED_DEFENSE_AREA = reda.AsReadOnly();
			}
		}
		public static volatile FieldPtsType FieldPts;

		public class TimeType
		{
			/// <summary> Frequency of strategy/interpreter loop </summary>
			volatile float _STRATEGY_FREQUENCY; public double STRATEGY_FREQUENCY => _STRATEGY_FREQUENCY;

			/// <summary> Frequency of simulator engine </summary>
			volatile float _SIM_ENGINE_FREQUENCY; public double SIM_ENGINE_FREQUENCY => _SIM_ENGINE_FREQUENCY;

			/// <summary> Frequency of robot control loop </summary>
			volatile float _CONTROL_LOOP_FREQUENCY; public double CONTROL_LOOP_FREQUENCY => _CONTROL_LOOP_FREQUENCY;

			/// <summary> Frequency of averaging predictor combos </summary>
			volatile float _COMBINE_FREQUENCY; public double COMBINE_FREQUENCY => _COMBINE_FREQUENCY;

			internal TimeType() {
				_STRATEGY_FREQUENCY = (float)ConstantsRaw.get<double>("default", "STRATEGY_FREQUENCY");
				_SIM_ENGINE_FREQUENCY = (float)ConstantsRaw.get<double>("default", "SIM_ENGINE_FREQUENCY");
				_CONTROL_LOOP_FREQUENCY = (float)ConstantsRaw.get<double>("default", "CONTROL_LOOP_FREQUENCY");
				_COMBINE_FREQUENCY = (float)ConstantsRaw.get<double>("default", "COMBINE_FREQUENCY");
			}
		}
		public static volatile TimeType Time;


		public class MotionType
		{
			/// <summary> Minimum allowed speed of wheel </summary>
			volatile int _WHEEL_SPEED_MIN; public int WHEEL_SPEED_MIN => _WHEEL_SPEED_MIN;

			/// <summary> Maximum allowed speed of wheel </summary>
			volatile int _WHEEL_SPEED_MAX; public int WHEEL_SPEED_MAX => _WHEEL_SPEED_MAX;

			/// <summary> TODO(davidwu): Add useful comment here. </summary>
			volatile float _ANGLE_AXIS_TO_WHEEL; public double ANGLE_AXIS_TO_WHEEL => _ANGLE_AXIS_TO_WHEEL;

			/// <summary> Distance from center of robot to wheels. </summary>
			volatile float _WHEEL_RADIUS; public double WHEEL_RADIUS => _WHEEL_RADIUS;

			/// <summary> Basic speed used by some planners and drivers, in m/s. </summary>
			volatile float _STEADY_STATE_SPEED; public double STEADY_STATE_SPEED => _STEADY_STATE_SPEED;

			/// <summary> Distance from center of robot to wheels. </summary>
			volatile string _LOG_FILE; public string LOG_FILE => _LOG_FILE;

			/// <summary> Distance from center of robot to wheels. </summary>
			volatile int _LOG_EVERY_MSEC; public int LOG_EVERY_MSEC => _LOG_EVERY_MSEC;

			/// <summary> Global speed scaling for all robots. </summary>
			volatile float _SPEED_SCALING_FACTOR_ALL; public double SPEED_SCALING_FACTOR_ALL => _SPEED_SCALING_FACTOR_ALL;

			/// <summary> Individual robot speed scaling. </summary>
			volatile double[] _SPEED_SCALING_FACTORS; public double[] SPEED_SCALING_FACTORS => _SPEED_SCALING_FACTORS;

			/// <summary> How far away should the controller ask the planners to stay away from the ball?. </summary>
			volatile float _BALL_AVOID_DIST; public double BALL_AVOID_DIST => _BALL_AVOID_DIST;

			/// <summary> If we're within this much of a destination and we're facing the right way, we're there. </summary>
			volatile float _MIN_DIST_TO_WP; public double MIN_DIST_TO_WP => _MIN_DIST_TO_WP;

			/// <summary> If we're within this much of an orientation and we're in the right location, we're there. </summary>
			volatile float _MIN_ANGLE_DIFF_TO_WP; public double MIN_ANGLE_DIFF_TO_WP => _MIN_ANGLE_DIFF_TO_WP;


			/// <summary> Constants for TangentBug motion planner </summary>
			public class TBugType
			{
				volatile float _LOOK_AHEAD_DIST; public double LOOK_AHEAD_DIST => _LOOK_AHEAD_DIST;
				volatile float _AVOID_DIST; public double AVOID_DIST => _AVOID_DIST;
				volatile float _WAYPOINT_DIST; public double WAYPOINT_DIST => _WAYPOINT_DIST;
				volatile float _MIN_ABS_VAL_STICK; public double MIN_ABS_VAL_STICK => _MIN_ABS_VAL_STICK;
				volatile float _EXTRA_GOAL_DIST; public double EXTRA_GOAL_DIST => _EXTRA_GOAL_DIST;
				volatile float _BOUNDARY_AVOID; public double BOUNDARY_AVOID => _BOUNDARY_AVOID;

				internal TBugType()
				{
					_LOOK_AHEAD_DIST = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_LOOK_AHEAD_DIST");
					_AVOID_DIST = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_AVOID_DIST");
					_WAYPOINT_DIST = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_WAYPOINT_DIST");
					_MIN_ABS_VAL_STICK = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_MIN_ABS_VAL_STICK");
					_EXTRA_GOAL_DIST = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_EXTRA_GOAL_DIST");
					_BOUNDARY_AVOID = (float)ConstantsRaw.get<double>("motionplanning", "TBUG_BOUNDARY_AVOID");
				}
			}
			public volatile TBugType TBug;

			/// <summary> Constants for plain RRT motion planner </summary>
			public class RRTType
			{
				volatile float _OBSTACLE_AVOID_DIST; public double OBSTACLE_AVOID_DIST => _OBSTACLE_AVOID_DIST;

				internal RRTType()
				{
					_OBSTACLE_AVOID_DIST = (float)ConstantsRaw.get<double>("motionplanning", "RRT_OBSTACLE_AVOID_DIST");
				}
			}
			public volatile RRTType RRT;

			internal MotionType()
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

				TBug = new TBugType();
				RRT = new RRTType();
			}

		}
		public static volatile MotionType Motion;

		public class PlayFilesType
		{
			/// <summary>  </summary>
			volatile bool _USE_C_SHARP_PLAY_SYSTEM; public bool USE_C_SHARP_PLAY_SYSTEM => _USE_C_SHARP_PLAY_SYSTEM;

			/// <summary>  </summary>
			volatile string _TACTIC_DIR; public string TACTIC_DIR => _TACTIC_DIR;

			/// <summary>  </summary>
			volatile string _PLAY_DIR_YELLOW; public string PLAY_DIR_YELLOW => _PLAY_DIR_YELLOW;

			/// <summary>  </summary>
			volatile string _PLAY_DIR_BLUE; public string PLAY_DIR_BLUE => _PLAY_DIR_BLUE;

			internal PlayFilesType()
			{
				_USE_C_SHARP_PLAY_SYSTEM = ConstantsRaw.get<bool>("default", "USE_C_SHARP_PLAY_SYSTEM");
				_TACTIC_DIR = ConstantsRaw.get<string>("default", "TACTIC_DIR");
				_PLAY_DIR_YELLOW = ConstantsRaw.get<string>("default", "PLAY_DIR_YELLOW");
				_PLAY_DIR_BLUE = ConstantsRaw.get<string>("default", "PLAY_DIR_BLUE");
			}
		}
		public static volatile PlayFilesType PlayFiles;

		public class PredictorType
		{
			/// <summary>  </summary>
			volatile bool _FLIP_COORDINATES; public bool FLIP_COORDINATES => _FLIP_COORDINATES;

			/// <summary>  </summary>
			volatile float _DELTA_DIST_SQ_MERGE; public double DELTA_DIST_SQ_MERGE => _DELTA_DIST_SQ_MERGE;

			/// <summary>  </summary>
			volatile float _MAX_SECONDS_TO_KEEP_INFO; public double MAX_SECONDS_TO_KEEP_INFO => _MAX_SECONDS_TO_KEEP_INFO;

			/// <summary>  </summary>
			volatile float _MAX_SECONDS_TO_KEEP_BALL; public double MAX_SECONDS_TO_KEEP_BALL => _MAX_SECONDS_TO_KEEP_BALL;

			/// <summary>  </summary>
			volatile float _VELOCITY_DT; public double VELOCITY_DT => _VELOCITY_DT;

			/// <summary>  </summary>
			volatile float _VELOCITY_WEIGHT_OLD; public double VELOCITY_WEIGHT_OLD => _VELOCITY_WEIGHT_OLD;

			/// <summary>  </summary>
			volatile float _VELOCITY_WEIGHT_NEW; public double VELOCITY_WEIGHT_NEW => _VELOCITY_WEIGHT_NEW;

			/// <summary>  </summary>
			volatile float _POSITION_WEIGHT_OLD; public double POSITION_WEIGHT_OLD => _POSITION_WEIGHT_OLD;

			/// <summary>  </summary>
			volatile float _POSITION_WEIGHT_NEW; public double POSITION_WEIGHT_NEW => _POSITION_WEIGHT_NEW;

			/// <summary>  </summary>
			volatile float _BALL_POSITION_WEIGHT_OLD; public double BALL_POSITION_WEIGHT_OLD => _BALL_POSITION_WEIGHT_OLD;

			/// <summary>  </summary>
			volatile float _BALL_POSITION_WEIGHT_NEW; public double BALL_POSITION_WEIGHT_NEW => _BALL_POSITION_WEIGHT_NEW;

			/// <summary>  </summary>
			volatile float _WEIGHT_OLD; public double WEIGHT_OLD => _WEIGHT_OLD;

			/// <summary>  </summary>
			volatile float _WEIGHT_NEW; public double WEIGHT_NEW => _WEIGHT_NEW;


			internal PredictorType()
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
		public static volatile PredictorType Predictor;

		public class RobotInfoType
		{
			/// <summary> Does this robot have a working kicker? </summary>
			static public bool HAS_KICKER(int i) => _HAS_KICKER[i]; static volatile bool[] _HAS_KICKER;

			/// <summary> Is this robot a goalie? </summary>
			static public bool IS_GOALIE(int i) => _IS_GOALIE[i]; static volatile bool[] _IS_GOALIE;

			internal RobotInfoType()
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
		public static volatile RobotInfoType RobotInfo;

		public class PlaysType
		{
			/// <summary> Delta distance for deciding that ball has moved. </summary>
			volatile float _BALL_MOVED_DIST; public double BALL_MOVED_DIST => _BALL_MOVED_DIST;

			internal PlaysType()
			{
				_BALL_MOVED_DIST = (float)ConstantsRaw.get<double>("plays", "BALL_MOVED_DIST");
			}
		}
		public static volatile PlaysType Plays;

		public class RadioProtocolType
		{
			/// <summary> Calculate a checksum when sending commands to the brushless boards. </summary>
			volatile bool _SEND_BRUSHLESSBOARD_CHECKSUM; public bool SEND_BRUSHLESSBOARD_CHECKSUM => _SEND_BRUSHLESSBOARD_CHECKSUM;

			/// <summary> Calculate a checksum when sending commands to the aux kicker boards. </summary>
			volatile bool _SEND_AUXBOARD_CHECKSUM; public bool SEND_AUXBOARD_CHECKSUM => _SEND_AUXBOARD_CHECKSUM;

			internal RadioProtocolType()
			{
				_SEND_BRUSHLESSBOARD_CHECKSUM = ConstantsRaw.get<bool>("default", "SEND_BRUSHLESSBOARD_CHECKSUM");
				_SEND_AUXBOARD_CHECKSUM = ConstantsRaw.get<bool>("default", "SEND_AUXBOARD_CHECKSUM");
			}
		}
		public static volatile RadioProtocolType RadioProtocol;

		//INITIALIZATION AND RELOAD MECHANISM---------------------------------------------------------

		static Object _lock = new Object();
		static volatile bool _is_reloading = false;
		static Constants()
		{
			Reload();
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
				Basic = new BasicType();
				Field = new FieldType();
				FieldPts = new FieldPtsType();
				Time = new TimeType();
				Motion = new MotionType();
				PlayFiles = new PlayFilesType();
				Predictor = new PredictorType();
				RobotInfo = new RobotInfoType();
				Plays = new PlaysType();
				RadioProtocol = new RadioProtocolType();
				_is_reloading = false;
			}
		}
	}

	public static class ConstantsRaw
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
