using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Verse;

namespace PublisherPlus.Data.CommitList
{
    public class CommitEntry : IComparable<CommitEntry>
    {
        readonly DateTime date;
        readonly string hash;
        readonly string content;
        bool isIncludedInChangeLog = true;
        public const int HashLength = 6;

        public string Hash => hash;
        public string ShortHash => hash.Substring(0, HashLength);
        public DateTime Date => date;
        public bool IsIncludedInChangeLog
        {
            get => isIncludedInChangeLog;
            set => isIncludedInChangeLog = value;
        }
        public string Content => content;

        public CommitEntry(string hash, DateTime date, string content)
        {
            this.hash = hash;
            this.date = date;
            this.content = content;
        }

        public CommitEntry(string gitLogEntry)
        {
            Log.Message($"parsing git log: {gitLogEntry}");
            Queue<string> lines = new Queue<string>(gitLogEntry.Split('\n'));
            this.hash = lines.Dequeue();
            this.date = DateTime.Parse(lines.Dequeue(), null, DateTimeStyles.RoundtripKind); // DateTimeStyles.RoundtripKind == ISO 8601 - which is what %cI in the git log format produces
            this.content = string.Join("\n", lines);
        }

        public void Draw(Listing_Standard list)
        {
            Rect rect = list.GetRect(Text.LineHeight);

            RectDivider divider = new RectDivider(rect, this.GetHashCode());
            Rect checkboxRect = divider.NewCol(Text.LineHeight);
            Widgets.Checkbox(checkboxRect.position, ref isIncludedInChangeLog, checkboxRect.width);

            float hashWidth = Text.CalcSize(ShortHash + "  ").x;
            Rect hashLabelRect = divider.NewCol(hashWidth, HorizontalJustification.Right);
            Widgets.TextArea(hashLabelRect, ShortHash, true);   //text area to allow copy-pasting the value for overriding the "last pushed" commit quickly
            //Widgets.Label(hashLabelRect, ShortHash);

            string dateLabel = Date.ToLongDateString();
            float dateWidth = Text.CalcSize(dateLabel).x;
            Rect dateRect = divider.NewCol(dateWidth, HorizontalJustification.Right);
            Widgets.Label(dateRect, dateLabel);

            Widgets.Label(divider.Rect, content);
        }

        public override string ToString()
        {
            return $"date={date},hash={hash},content={content}";
        }

        public int CompareTo(CommitEntry other)
        {
            return this.date.CompareTo(other.date);
        }
    }
}
