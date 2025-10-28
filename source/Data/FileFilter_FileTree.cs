using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace PublisherPlus.Data
{
    public enum NodeInclusionState
    {
        Included,
        Excluded,
        Partial
    }

    public class FileFilter_FileTree : FileFilter
    {
        public FileFilter_FileTree(ManagedWorkshopPackage package) : base(package) { }
        public override string FilterReason => "FileTreeExclusion";
        public override bool IsActive => true;

        /// <remarks>
        /// <see cref="NodeInclusionState.Included"/> is not explicitely tracked in this dictionary. absent == INCLUDED
        /// </remarks>
        Dictionary<FileTreeNode, NodeInclusionState> states = new Dictionary<FileTreeNode, NodeInclusionState>();
        public void ToggleState(FileTreeNode node)
        {
            NodeInclusionState currentState = states.TryGetValue(node, NodeInclusionState.Included);
            switch(currentState)
            {
                case NodeInclusionState.Partial:
                    List<FloatMenuOption> options = new List<FloatMenuOption>()
                    {
                        new FloatMenuOption(Language.Get("IncludeAll"), () => SetState(node, NodeInclusionState.Included)),
                        new FloatMenuOption(Language.Get("ExcludeAll"), () => SetState(node, NodeInclusionState.Excluded))
                    };
                    Find.WindowStack.Add(new FloatMenu(options));
                    break;
                case NodeInclusionState.Included:
                    // children lose their state (become included) when their parent gets excluded, so we must check that the nodes parent is not tracking the "excluded" state for this node
                    if(AllowsPublishing(node))
                    {
                        SetState(node, NodeInclusionState.Excluded);
                    }
                    else
                    {
                        // in this specific case a parent is excluded. We must exclude all the siblings of this node and then include the node itself
                        if(node.parent != null)
                        {
                            foreach(FileTreeNode child in node.parent.children)
                            {
                                states[child] = NodeInclusionState.Excluded;
                            }
                        }
                        SetState(node, NodeInclusionState.Included);
                    }
                    break;
                case NodeInclusionState.Excluded:
                    SetState(node, NodeInclusionState.Included);
                    break;
            }

            Log.Message($"Toggled state for node {node} from old state {currentState} to new state {states.TryGetValue(node, NodeInclusionState.Included)}");
            Log.Message($"Current dictionary: {states.ToStringFullContents()}");
        }

        private void SetState(FileTreeNode node, NodeInclusionState state)
        {
            switch(state)
            {
                case NodeInclusionState.Included:
                    states.Remove(node);    // ignore FALSE result, if it didn't exist before, it doesn't need to exist
                    RemoveChildStates(node);
                    break;
                case NodeInclusionState.Excluded:
                    states.SetOrAdd(node, state);
                    RemoveChildStates(node);
                    break;
                case NodeInclusionState.Partial:    // partial is only set when a child prompts a directory to be partial, so it doesn't need to update its children
                    states.SetOrAdd(node, state);
                    break;
            }

            UpdateParent(node);
        }

        /// <summary>
        /// Each call to this method steps into the direct parent of the given <paramref name="node"/> and calculates its state. This always leads to calling <see cref="SetState(FileTreeNode, NodeInclusionState)"/> for the parent, which will in turn call to <see cref="UpdateParent(FileTreeNode)"/> from the original nodes parent to iterate its parents
        /// </summary>
        private void UpdateParent(FileTreeNode node)
        {
            FileTreeNode parent = node.parent;
            if(parent == null)  // reached the root
            {
                return;
            }
            // the parent (directory) is included if all children (files or directories) are included (not tracked in the states dict)
            if(parent.children.All(child => !states.ContainsKey(child)))
            {
                SetState(parent, NodeInclusionState.Included);
            }
            // otherwise we must check the states and set to excluded or partial
            else
            {
                // if all children (files or directories) are flagged as excluded, the parent (directory) becomes excluded
                if(parent.children.All(child => states.TryGetValue(child, NodeInclusionState.Included) == NodeInclusionState.Excluded))
                {
                    // this will remove the tracked states for the children as well, as it is unnecessary to keep track of them when the parent is excluded
                    SetState(parent, NodeInclusionState.Excluded);
                }
                else
                {
                    SetState(parent, NodeInclusionState.Partial);
                }
            }
        }

        private void RemoveChildStates(FileTreeNode node)
        {
            foreach(FileTreeNode child in node.children)
            {
                states.Remove(child);    // ignore FALSE result
                RemoveChildStates(child);
            }
        }

        public override bool AllowsPublishing(FileSystemInfo entry)
        {
            return AllowsPublishing(package.fileTree.NodeForEntry(entry));

        }

        public bool IsPartial(FileTreeNode node)
        {
            return states.TryGetValue(node, NodeInclusionState.Included) == NodeInclusionState.Partial;
        }

        public bool AllowsPublishing(FileTreeNode node)
        {
            return states.TryGetValue(node, NodeInclusionState.Included) != NodeInclusionState.Excluded
                && (node.parent == null || AllowsPublishing(node.parent));
        }

        public override void StartSaving()
        {
            base.StartSaving();
            package.SerializedData.FileTree.ExcludedPaths = states
                .Where(state => state.Value == NodeInclusionState.Excluded)
                .Select(state => state.Key.entryInfo.GetRelativePathTo(package.ModRootDirectory))
                .ToHashSet();
        }

        public override void FinishLoading()
        {
            base.FinishLoading();
            List<FileTreeNode> nodes = package.SerializedData.FileTree.ExcludedPaths
                .Select(path => GetNode(path))
                .ToList();

            foreach(FileTreeNode entry in nodes)
            {
                states[entry] = NodeInclusionState.Excluded;
            }

            // this will trigger the calculations for "partial" state in directories. Makes the most sense AFTER all excluded states are set
            foreach(FileTreeNode entry in nodes)
            {
                UpdateParent(entry);
            }
        }

        private FileTreeNode GetNode(string relativeFilePath)
        {
            string path = Path.Combine(package.ModRootDirectory.FullName, relativeFilePath);
            FileSystemInfo entryInfo;
            if(Directory.Exists(path))
            {
                entryInfo = new DirectoryInfo(path);
            }
            else if(File.Exists(path))
            {
                entryInfo = new FileInfo(path);
            }
            else
            {
                throw new Exception($"Could not find file or directory for path {relativeFilePath}");
            }
            FileTreeNode entry = package.fileTree.NodeForEntry(entryInfo);
            if(entry == null)
            {
                throw new Exception($"Could not retrieve file tree node for {(entryInfo is FileInfo ? "file" : "directory")} {entryInfo.FullName}");
            }
            return entry;
        }
    }
}
