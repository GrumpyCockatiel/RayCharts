namespace Raydreams.Xamarin.Controls
{
	/// <summary>Captures sizes needed for the X and Y label from pre calculations for later usage.</summary>
	public struct LabelMetrics
	{
		public LabelMetrics(float fs, float h, float desc)
		{
			this.FontSize = fs;
			this.Height = h;
			this.Descent = desc;
		}

		public static LabelMetrics Default()
        {
			return new LabelMetrics( 10, 10, 1 );
        }

		/// <summary>The calculated font size</summary>
		public float FontSize { get; set; }

		/// <summary>The calculcated height needed from Ascent to Descent to cover the max height of any possible text.</summary>
		public float Height { get; set; }

		/// <summary>The calculcated descent of the font needed</summary>
		public float Descent { get; set; }
	}
}
