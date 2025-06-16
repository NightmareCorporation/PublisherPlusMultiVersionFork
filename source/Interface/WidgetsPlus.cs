using UnityEngine;
using Verse;

namespace PublisherPlus.Interface
{
	internal static class WidgetsPlus
	{
		private static readonly Color DisabledColor = new Color(0.5f, 0.5f, 0.5f);

		public static bool ButtonText(Rect rect, string label, bool enabled = true)
		{
			Color previousColor = GUI.color;

			if(!enabled)
			{
				GUI.color = DisabledColor;
			}
			bool result = Widgets.ButtonText(rect, label, active: enabled);

			GUI.color = previousColor;
			return result;
		}
	}
}
