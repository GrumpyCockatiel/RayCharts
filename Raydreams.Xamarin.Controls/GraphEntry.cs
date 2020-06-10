using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Raydreams.Xamarin.Controls
{
	/// <summary>A single line label entry point on either axis</summary>
	public class LineLabel
	{
		public LineLabel(float loc, string text, bool line = true)
		{
			this.Location = loc;
			this.Line = line;
			this.Text = text.Trim();
		}

		/// <summary></summary>
		public string Text { get; set; }

		/// <summary></summary>
		public float Location { get; set; }

		/// <summary></summary>
		public bool Line { get; set; }
	}

	/// <summary>A line data series</summary>
	public class LineSeries
	{
		public LineSeries()
		{
			this.StrokeWidth = 0.0F;
			this.LineColor = new SKColor( 255, 255, 255 );
		}

		public SKPoint[] Values { get; set; }

		/// <summary></summary>
		public SKColor LineColor { get; set; }

		/// <summary></summary>
		public float StrokeWidth { get; set; }
	}

	/// <summary>The logical data for each bar entry</summary>
	public class GraphEntry
	{
		private float _value = 0.0F;
		private float _threshold = 0.0F;

		public GraphEntry() : this( 0.0F, 0.0F, true )
		{
		}

		public GraphEntry( float value = 0.0F, float threshold = 0.0F, bool isEmpty = false )
		{
			this.IsEmpty = isEmpty;
			this.Value = value;
			this.Threshold = threshold;
			this.ValueColor = new SKColor( 255, 0, 0 );
			this.ThresholdColor = new SKColor( 100, 100, 100 );
			this.Label = String.Empty;
		}

		/// <summary>There is no data for this group, but still include it in calculating layouts. Possibly Show some other symbol like a dot</summary>
		public bool IsEmpty { get; set; }

		/// <summary>The actual value obtained</summary>
		public float Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = (value < 0) ? 0.0F : value;
			}
		}

		/// <summary>The value needed to break the threshold</summary>
		public float Threshold
		{
			get
			{
				return this._threshold;
			}
			set
			{
				this._threshold = (value < 0) ? 0.0F : value;
			}
		}

		/// <summary>Color on the bar in the back</summary>
		public SKColor ThresholdColor { get; set; }

		/// <summary></summary>
		public SKColor ValueColor { get; set; }

		/// <summary>X Axies Label for each bar</summary>
		public string Label { get; set; }

	}
}
