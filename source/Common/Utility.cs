using System;
using System.Diagnostics;
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

        public static string GetRelativePathTo(string filePath, string directoryPath)
        {
            if(!filePath.Contains(directoryPath))
            {
                // file is not in directory, no relative path
                return filePath;
            }
            return filePath.Replace(directoryPath, "");
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
        public static string HumanReadableBytes(this long bytes)
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

        public static void Label(Rect inRect, string label, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            TextAnchor previousAnchor = Text.Anchor;
            Text.Anchor = anchor;
            Widgets.Label(inRect, label);
            Text.Anchor = previousAnchor;
        }
        #endregion

        #region UI
        public static void CheckboxLabeled(this Listing_Standard list, string label, ref bool checkOn, string tooltip, Color? color)
        {
            Color previousColor = GUI.color;
            GUI.color = color ?? GUI.color;
            list.CheckboxLabeled(label, ref checkOn, tooltip);
            GUI.color = previousColor;
        }

        public static void TextAreaWithSaveResetButtons(this Listing_Standard list, ref string textContent, Action saveAction, Action resetAction)
        {
            RectDivider divider = new RectDivider(list.GetRect(Text.LineHeight), textContent.GetHashCode());

            DoSave();
            DoReset();
            textContent = Widgets.TextArea(divider.Rect, textContent);

            void DoSave()
            {
                string label = Language.Get("Settings.Save");
                Rect labelRect = divider.NewCol(Text.CalcSize(label + "    ").x, HorizontalJustification.Right, 2).Rect;
                if(Widgets.ButtonText(labelRect, label))
                {
                    saveAction.Invoke();
                }
            }
            void DoReset()
            {
                string label = Language.Get("Settings.Reset");
                Rect labelRect = divider.NewCol(Text.CalcSize(label + "    ").x, HorizontalJustification.Right, 2).Rect;
                if(Widgets.ButtonText(labelRect, label))
                {
                    resetAction.Invoke();
                }
            }
        }
        #endregion

        #region Scroll View (Taken from NightmareCore)
        /// <summary>
        /// will be subtracted from width for inner windows in scroll views
        /// </summary>
        public const float DefaultSliderWidth = 16f;

        /// <summary>
        /// Convenience method to create a rectangle that fits perfectly as the inner rectangle (the content) of a scroll view rect
        /// </summary>
        /// <param name="outerRect">Outer rectangle that will hold the slider and bounding box</param>
        /// <param name="height">Height of the inner rectangle, you must calculate and preferrably cache it</param>
        /// <param name="sliderWidth">The width of the slider on the side</param>
        /// <returns></returns>
        public static Rect CreateInnerScrollRect(Rect outerRect, float height, float sliderWidth = DefaultSliderWidth)
        {
            return new Rect
            (
                0,
                0,
                outerRect.width - sliderWidth,
                height
            );
        }
        /// <summary>
        /// Convenience method to properly begin a scroll view rect and produce a Listing_Standard for it
        /// </summary>
        /// <param name="inRect">Rect that contains the full scroll view (bounding box)</param>
        /// <param name="requiredHeight">Height required to display all the contents of the inner scroll view. You should initialize this with a high number and then overwrite with the result of <see cref="EndScrollView(Listing_Standard, ref float)"/></param>
        /// <param name="scrollPosition">keeps track of the current scroll position</param>
        /// <param name="list">Produced Listing_Standard, which will keep track of the used height for <see cref="EndScrollView(Listing_Standard, ref float)"/></param>
        public static void MakeAndBeginScrollView(Rect inRect, float requiredHeight, ref Vector2 scrollPosition, out Listing_Standard list)
        {
            MakeAndBeginScrollView(inRect, requiredHeight, ref scrollPosition, out Rect innerRect);
            list = new Listing_Standard()
            {
                ColumnWidth = innerRect.width,
                maxOneColumn = true
            };
            list.Begin(innerRect);
        }

        /// <summary>
        /// Convenience method to properly begin a scroll view rect
        /// </summary>
        /// <param name="inRect">Rect that contains the full scroll view (bounding box)</param>
        /// <param name="requiredHeight">Height required to display all the contents of the inner scroll view. You should initialize this with a high number and then overwrite with the result of <see cref="EndScrollView(Listing_Standard, ref float)"/></param>
        /// <param name="scrollPosition">keeps track of the current scroll position</param>
        /// <param name="outRect">Inner scrollView rectangle</param>
        public static void MakeAndBeginScrollView(Rect inRect, float requiredHeight, ref Vector2 scrollPosition, out Rect outRect)
        {
            outRect = CreateInnerScrollRect(inRect, requiredHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, outRect);
        }

        /// <summary>
        /// Convenience method to wrap up <see cref="MakeAndBeginScrollView(Rect, float, ref Vector2, out Listing_Standard)"/>
        /// </summary>
        /// <param name="list">The Listing_Standard used by the scroll views inner rectangle</param>
        /// <param name="requiredHeight">The height used by the Listing_Standard</param>
        public static void EndScrollView(this Listing_Standard list, out float requiredHeight)
        {
            requiredHeight = list.CurHeight;
            list.End();
            Widgets.EndScrollView();
        }
        #endregion

        public static string RunGitCommand(string arguments, string workingDirectory = null)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = "git",   // using "git" refers to git.exe on windows systems and *should* refer to git.sh on linux systems. Hit me up if it doesn't and I will move this to the settings to make it configurable :)
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = arguments
            };
            if(workingDirectory != null)
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            const int maxWaitMs = 2000;
            process.WaitForExit(maxWaitMs);
            return output;
        }
    }
}
