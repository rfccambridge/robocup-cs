using System;
using System.Collections.Generic;
using System.Text;

namespace RFC.Core
{
	[Serializable]
	/// <summary>
	/// A generic class for associating data with wheels of a robot.
	/// </summary>
	public class WheelsInfo<T>
	{
		public T lf, rf, lb, rb;
		public WheelsInfo(T rf, T lf, T lb, T rb)
		{
			this.lf = lf;
			this.lb = lb;
			this.rf = rf;
			this.rb = rb;
		}

		/// <summary>
		/// Creates a WheelInfo object with default values
		/// </summary>
		public WheelsInfo()
			: this(default(T), default(T), default(T), default(T))
		{
		}

		public override string ToString()
		{
			return rf.ToString() + "," + lf.ToString() + "," + lb.ToString() + "," + rb.ToString();
		}

	}

	/// <summary>
	/// A storage class for holding the four wheel speeds.
	/// 
	/// The convention for wheel speeds is that positive values contribute to the robot going counterclockwise.
	/// </summary>
	public class WheelSpeeds : WheelsInfo<double>
	{
		public WheelSpeeds(double rf, double lf, double lb, double rb)
			: base(rf, lf, lb, rb)
		{ }

		/// <summary>
		/// Creates a WheelSpeeds object with all speeds defaulting to 0.
		/// </summary>
		public WheelSpeeds()
			: this(0, 0, 0, 0)
		{ }

		static public WheelSpeeds operator +(WheelSpeeds lhs, WheelSpeeds rhs)
		{
			return new WheelSpeeds(rhs.rf + lhs.rf, rhs.lf + lhs.lf, rhs.lb + lhs.lb, rhs.rb + lhs.rb);
		}
		static public WheelSpeeds operator -(WheelSpeeds lhs, WheelSpeeds rhs)
		{
			return new WheelSpeeds(lhs.rf - rhs.rf, lhs.lf - rhs.lf, lhs.lb - rhs.lb, lhs.rb - rhs.rb);
		}
		static public WheelSpeeds operator *(double d, WheelSpeeds ws)
		{
			return new WheelSpeeds(d * ws.rf, d * ws.lf, d * ws.lb, d * ws.rb);
		}
		static public WheelSpeeds operator *(WheelSpeeds ws, double d)
		{
			return new WheelSpeeds(d * ws.rf, d * ws.lf, d * ws.lb, d * ws.rb);
		}
		static public WheelSpeeds operator /(WheelSpeeds ws, double d)
		{
			return new WheelSpeeds(ws.rf / d, ws.lf / d, ws.lb / d, ws.rb / d);
		}

		//Dot product
		static public double operator *(WheelSpeeds lhs, WheelSpeeds rhs)
		{
			return lhs.rf * rhs.rf + lhs.lf * rhs.lf + lhs.lb * rhs.lb + lhs.rb * rhs.rb;
		}

		public double magnitudeSq()
		{
			return rf * rf + lf * lf + lb * lb + rb * rb;
		}

		public double magnitude()
		{
			return Math.Sqrt(rf * rf + lf * lf + lb * lb + rb * rb);
		}

		//Projection - returns the factor by which the other wheelspeeds should be scaled to produce
		//the projection of this vector on to the other (as a 4-vector)
		public double getProjectionFactor(WheelSpeeds other)
		{
			return this * other / other.magnitudeSq();
		}

		public String toString()
		{
			return "WheelSpeeds <rf>: " + rf + " <lf>: " + lf + " <lb>: " + lb + " <rb>: " + rb;
		}
	}
}
