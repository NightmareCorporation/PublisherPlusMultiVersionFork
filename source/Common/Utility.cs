using System.IO;
using UnityEngine;
using Verse;

namespace PublisherPlus
{
	public static class Utility
	{
		public static bool IsDirectory(this FileSystemInfo info) => info.Attributes.HasFlag(FileAttributes.Directory);

		public static void CheckboxLabeled(this Listing_Standard list, string label, ref bool checkOn, string tooltip, Color? color)
		{
			Color previousColor = GUI.color;
			GUI.color = color ?? GUI.color;

			list.CheckboxLabeled(label, ref checkOn, tooltip);

			GUI.color = previousColor;
		}

		public static string Italic(this string self) => "<i>" + self + "</i>";
		public static string Bold(this string self) => "<b>" + self + "</b>";

		public static bool ExistsNow(this FileSystemInfo self)
		{
			self.Refresh();
			return self.Exists;
		}
	}
}
