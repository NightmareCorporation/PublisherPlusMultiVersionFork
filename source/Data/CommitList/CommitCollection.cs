using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PublisherPlus.Data.CommitList
{
    public class CommitCollection : IEnumerable<CommitEntry>
    {
        List<CommitEntry> entries;

        public CommitCollection(string gitLogText)
        {
            entries = gitLogText
                .Split("---")
                .Where(entry => entry.Trim() != string.Empty)
                .Select(commit => new CommitEntry(commit.Trim()))
                .ToList();
        }

        public IEnumerator<CommitEntry> GetEnumerator()
        {
            return ((IEnumerable<CommitEntry>)entries).GetEnumerator();
        }

        /// <summary>
        /// Take the given <paramref name="lastUploadedHash"/> and remove it and all its ancestors from the collection. This will leave all the new commits since the last push to the workshop left in the collection to display in the UI
        /// </summary>
        /// <param name="lastUploadedHash">This commit and all commits chronologically before it will be removed. Can work with both short hash (6 characters) and full hash</param>
        public void ExcludeAllCommitsBefore(string lastUploadedHash)
        {
            CommitEntry markerCommit = entries.FirstOrDefault(commit => commit.Hash.StartsWith(lastUploadedHash));
            if(markerCommit == null)
            {
                Log.Error($"Could not find commit entry for hash {lastUploadedHash}");
                return;
            }
            foreach(CommitEntry entry in entries)
            {
                entry.IsIncludedInChangeLog = entry.Date > markerCommit.Date;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)entries).GetEnumerator();
        }
    }
}
