using AkariMindControllers.Base;
using MonoMod;
using MU3.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Reader
{
	[MonoModPatch("global::MU3.Reader.ReaderMain")]
	public class ReaderMainEx : ReaderMain
	{
		public AutoplayLaneCollection APFLanes { get; private set; }

		public extern bool orig_loadScore(string path);
		public bool loadScore(string path)
		{
			if (!orig_loadScore(path))
				return false;

			APFLanes = AutoplayLaneCollection.decode(path,composition);

			return true;
		}

		private extern RecordMap orig_parse(string path);
		private RecordMap parse(string path) => orig_parse(path);
	}

}
