using System;
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
			string path = item.FullName.Replace(directory.FullName, "");

			if(path.StartsWith(Path.DirectorySeparatorChar))
			{
				path = path.Substring(1);
			}

			return path;
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
				self = $"{indentString}{self}";
			}
			return self;
		}
		const int byteOrderSize = 1024;
		// copied from https://stackoverflow.com/a/4975942
		public static string HumanReadable(this long bytes)
		{
			string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
			if(bytes == 0)
			{
				return "0" + suffixes[0];
			}
			int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, byteOrderSize)));
			double num = Math.Round(bytes / Math.Pow(byteOrderSize, place), 1);
			return $"{Math.Sign(bytes) * num} {suffixes[place]}";
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
