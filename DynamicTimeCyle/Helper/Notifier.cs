using Aki.Reflection.Utils;
using EFT.Communications;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace r1ft.DynamicTimeCyle
{
    class Notifier
    {
		private static readonly MethodInfo notifierMessageMethod;

		static Notifier()
		{
			var notifierType = PatchConstants.EftTypes.Single(x => x.GetMethod("DisplayMessageNotification") != null);
			notifierMessageMethod = notifierType.GetMethod("DisplayMessageNotification");
		}

		private static void DisplayMessageNotification(string message, Color color)
        {
			notifierMessageMethod.Invoke(null, new object[] { message, ENotificationDurationType.Infinite, ENotificationIconType.Hideout, color });
        }

		public static void DrawCurrentTime(bool hideout, double hour, double min)
		{
			if (hideout || hour == 99)
				DisplayMessageNotification($"Hideout", Color.white);
			else
				DisplayMessageNotification($"Current Raid Time: {(hour < 10 ? "0" : "")}{string.Format("{0:0,0}", hour)}:{string.Format("{0:0,0}", min)}", Color.white);

			return;
		}
	}
}
