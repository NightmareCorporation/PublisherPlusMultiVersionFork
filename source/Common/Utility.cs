using System.IO;
using UnityEngine;
using Verse;

namespace PublisherPlus
{
	public static class Utility
	{
		#region File
		public static bool IsDirectory(this FileSystemInfo info)
		{
			return info.Attributes.HasFlag(FileAttributes.Directory);
		}

		/// <summary>
		/// Gets the relative path of <paramref name="item"/> to the given <paramref name="directory"/>.
		/// Returns <see cref="null"/> if the given <paramref name="item"/> is not located within the given <paramref name="directory"/>.
		/// </summary>
		public static string GetRelativePathTo(this FileSystemInfo item, FileSystemInfo directory)
		{
			if(!item.FullName.Contains(directory.FullName))
			{
				return null;
			}
			return item.FullName.Replace(directory.FullName, "");
		}

		public static bool ExistsNow(this FileSystemInfo self)
		{
			self.Refresh();
			return self.Exists;
		}
		#endregion

		#region Text
		public static string Italic(this string self) => "<i>" + self + "</i>";
		public static string Bold(this string self) => "<b>" + self + "</b>";
		public static string Indent(this string self, int indentCount, string indentString = "  ")
		{
			for(int i = 0; i < indentCount; i++)
			{
				self = indentString + self;
			}
			return self;
		}
		#endregion

		public static void CheckboxLabeled(this Listing_Standard list, string label, ref bool checkOn, string tooltip, Color? color)
		{
			Color previousColor = GUI.color;
			GUI.color = color ?? GUI.color;
			list.CheckboxLabeled(label, ref checkOn, tooltip);
			GUI.color = previousColor;
		}
	}
}
