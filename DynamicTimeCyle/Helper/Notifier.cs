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

		public static void DisplayMessageNotification(string message, ENotificationDurationType duration = ENotificationDurationType.Default, ENotificationIconType iconType = ENotificationIconType.Default, Color? textColor = null)
		{
			notifierMessageMethod.Invoke(null, new object[] { message, duration, iconType, textColor });
		}
	}
}
