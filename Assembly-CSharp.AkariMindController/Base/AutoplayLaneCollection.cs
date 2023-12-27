using MU3.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AkariMindControllers.Base
{
	public class AutoplayLaneCollection
	{
		private List<List<LanePoint>> laneList;

		public AutoplayLaneCollection(List<List<LanePoint>> laneList)
		{
			this.laneList = laneList;
		}

		public class LanePoint : IComparable<LanePoint>
		{
			public float Frame;
			public float XUnit;

			public int CompareTo(LanePoint other)
			{
				return Frame.CompareTo(other.Frame);
			}

			public override string ToString()
			{
				return $"Frame={Frame}, XUnit={XUnit}";
			}
		}

		public static AutoplayLaneCollection decode(string ogkrFilePath, Composition composition)
		{
			var lines = File.ReadAllLines(ogkrFilePath);
			var map = new Dictionary<int, List<LanePoint>>();

			foreach (var line in lines)
			{
				if (!line.StartsWith("[APF"))
					continue;

				var splitStr = line.Split('\t');
				var groupId = int.Parse(splitStr[1]);
				var tUnit = int.Parse(splitStr[2]);
				var tGrid = int.Parse(splitStr[3]);
				var xUnit = int.Parse(splitStr[4]);

				if (!map.TryGetValue(groupId, out var list))
					list = map[groupId] = new List<LanePoint>();

				var xw = new TGrid(tGrid + tUnit * 1920);
				composition.bpmList.calcTGrid(xw);

				list.Add(new() { Frame = xw.frame, XUnit = xUnit });
			}

			foreach (var list in map.Values)
				list.Sort();

			var laneList = map.Values.Where(x => x.Count > 1).ToList();
			PatchLog.WriteLine($"Decode APFLane: {laneList.Count} lanes.");
			return new AutoplayLaneCollection(laneList);
		}

		public double? CalculateFaderXUnit(double tUnit)
		{
			var lane = laneList.FirstOrDefault(x => x[0].Frame <= tUnit && tUnit <= x.Last().Frame);
			if (lane == null)
				return default;

			for (var i = 0; i < lane.Count - 1; i++)
			{
				var cur = lane[i];
				var next = lane[i + 1];

				if (!(cur.Frame <= tUnit && tUnit <= next.Frame))
					continue;

				var fromX = cur.XUnit;
				var toX = next.XUnit;
				var fromTime = cur.Frame;
				var toTime = next.Frame;

				var curTime = tUnit;

				var curX = toTime == fromTime ? fromX : (fromX + (toX - fromX) * ((curTime - fromTime) / (toTime - fromTime)));

				return curX;
			}

			return default;
		}
	}
}
