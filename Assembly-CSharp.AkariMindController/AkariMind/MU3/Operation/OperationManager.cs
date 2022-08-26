using MU3.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkariMindControllers.AkariMind.MU3.Operation
{
	public class OperationManager : Singleton<OperationManager>
	{
		public bool isAuthGood => true;

		public bool isAliveAimeServer => true;

		public bool isAliveServer => true;

		public bool isDataVersionOnline => true;

		public bool wasDownloadSuccessOnce => true;

		public bool isLastAlive => true;

		public bool IsServerGameSettingValid => true;

		public bool KOPOpened => true;
	}
}
