using System;
using System.Collections.Generic;

namespace Raydreams.Xamarin.Controls
{
	/// <summary></summary>
	public static class LabelMaker
	{
		/// <summary></summary>
		/// <param name="value"></param>
		/// <param name="scale"></param>
		/// <returns></returns>
		public static float Above(float value, int scale = 0)
		{
			if (scale < 0)
				scale = 0;

			double factor = Math.Pow(10.0, scale);

			double adj = Math.Round(value / factor, MidpointRounding.AwayFromZero) * factor;

			return Convert.ToSingle(adj);
		}

		/// <summary></summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="divs"></param>
		/// <returns></returns>
		/// <remarks>Be careful doesnt quite work yet</remarks>
		public static List<LineLabel> YLables(float min, float max, int divs)
		{
			List<LineLabel> labels = new List<LineLabel>();

			labels.Add(new LineLabel(min, String.Format("${0}", min)));
			labels.Add(new LineLabel(max, String.Format("${0}", max)));

			double span = Convert.ToDouble(max - min) / (divs + 1);

			while (divs > 0)
			{
				float halfY = Convert.ToSingle(Math.Floor(min + (span * divs)));
				labels.Add(new LineLabel(halfY, String.Format("${0}", halfY)));
				--divs;
			}

			return labels;
		}

		/// <summary></summary>
		/// <param name="syr"></param>
		/// <param name="smonth"></param>
		/// <param name="eyr"></param>
		/// <param name="emonth"></param>
		/// <returns></returns>
		public static List<LineLabel> XMonths(int syr, int smonth, int eyr, int emonth)
		{
			List<LineLabel> labels = new List<LineLabel>();

			DateTime start = new DateTime(syr, smonth, 1);
			DateTime end = new DateTime(eyr, emonth, 1);
			DateTime cur = start;
			int m = cur.Month;

			while (cur <= end)
			{
				labels.Add(new LineLabel(m, cur.ToString("MMM").ToLower(), true));
				cur = cur.AddMonths(1);
				++m;
			}

			return labels;
		}
	}
}
