using System;
using SkiaSharp;

namespace Raydreams.Xamarin.Controls
{
	public static class QuickCalc
	{
		/// <summary>Converts a angle value in degrees to equivalent radians</summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static float Deg2Rad( float angle )
		{
			return Convert.ToSingle( System.Math.PI * angle / 180.0F );
		}

		/// <summary>Returns a new X, Y point in cartesian coordinates from angle in radians in polar</summary>
		/// <param name="radians">Value to convert in radians where 2Pi = 360 deg</param>
		/// <param name="radius">The radius of the polar coordinates to locate the point on</param>
		/// <returns>X,Y tuple as float</returns>
		public static Tuple<float, float> Transform( float radians, float radius )
		{
			return new Tuple<float, float>( radius * Convert.ToSingle( Math.Cos( radians ) ),
				radius * Convert.ToSingle( Math.Sin( radians ) ) );
		}

		/// <summary>Calculates the length of a chord some distance d from the origin.</summary>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="d">Distance from the origin</param>
		/// <returns>Total length of the chord from circumference</returns>
		public static double CalcChord(double radius, double d )
		{
			if (d >= radius)
				throw new System.ArithmeticException("Distance cannot be greater than the radius.");

			return 2 * Math.Sqrt( (radius * radius) - (d * d) );
		}

		/// <summary>Calculates the square inscribe the specfied circle of radius and origin</summary>
        /// <param name="org"></param>
        /// <param name="rad"></param>
        /// <returns></returns>
		public static SKRect InscribeSquare(SKPoint org, float rad)
		{
			if (rad <= 0)
				return new SKRect();

			float h = Convert.ToSingle(Math.Sin( Math.PI * 0.25 ) * rad);

			return new SKRect(org.X-h, org.Y-h, org.X+h, org.Y+h);
		}

		/// <summary>Determines the maximum font size that will fit in the width of the rect</summary>
        /// <param name="brush"></param>
        /// <param name="rect"></param>
        /// <param name="text"></param>
        /// <returns></returns>
		public static LabelMetrics CalcSizeForWidth(SKPaint brush, float rectWidth, string text)
        {
			// this is about the largest font size to fit '888' in a iPhone 11 max pro
			float fontSize = 500.0F;

			// set the brush to the max font size
			brush.TextSize = fontSize;

			// measure the text
			SKRect textBounds = new SKRect();
			brush.MeasureText( text, ref textBounds );

			// scale the font size
			fontSize = brush.TextSize * (rectWidth / textBounds.Width);

			SKFontMetrics metrics;
			brush.GetFontMetrics( out metrics );

			return new LabelMetrics( fontSize, metrics.Descent - metrics.Ascent, metrics.Descent );
		}

		/// <summary>Normalizes the input angle to an equivalent positive angle between 0 and 360.</summary>
		/// <param name="theta">Input angle in degrees.</param>
		/// <returns>Normalized angle in degrees.</returns>
		/// <example>An input of -1677831.2621266 would return 128.737873400096</example>
		public static float Revolution( float theta )
		{
			//if ( theta >= 0 )
			//    return Math.IEEERemainder(theta, 360.0);
			//else
			//    return 360.0 + Math.IEEERemainder(theta, 360.0);

			while ( theta < 0.0 || theta > 360.0F )
			{
				if ( theta > 360.0F )
					theta -= 360.0F;
				else if ( theta < 0.0 )
					theta += 360.0F;
				else break;
			}

			return theta;
		}

		/// <summary>Normalizes the input angle to an equivalent angle between +180.0 and -180.0</summary>
		/// <param name="theta">input angle in degrees</param>
		/// <returns>normalized angle (double) in degrees</returns>
		public static double Revolution180( double theta )
		{
			while ( theta < -180.0 || theta > 180.0 )
			{
				if ( theta > 180.0 )
					theta -= 180.0;
				else if ( theta < -180.0 )
					theta += 180.0;
				else break;
			}

			return theta;
		}
	}
}
