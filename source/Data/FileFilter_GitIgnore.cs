using GitignoreParserNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Verse;

namespace PublisherPlus.Data
{
    public class FileFilter_GitIgnore : IFileFilter
    {
        /// <summary>
        /// There can be multiple .gitignore files located throughout a solution, each gitignore must apply its filters relative to the file location
        /// </summary>
        private Dictionary<FileInfo, GitignoreParser> gitIgnoreParsers;

        private const string GitIgnoreName = ".gitignore";

        ManagedWorkshopPackage package;

        public void SetWorkshopPackage(ManagedWorkshopPackage package)
        {
            this.package = package;
        }

        public string FilterReason => ".gitignore";

        private string _gitIgnoreInfoText;
        public string GitIgnoreInfoText
        {
            get
            {
                if(_gitIgnoreInfoText == null)
                {
                    if(gitIgnoreParsers.NullOrEmpty())
                    {
                        _gitIgnoreInfoText = Language.Get("GitIgnore.Info.NoGitIgnoreFound");
                    }
                    else
                    {
                        string formattedParsers = string.Join("\n", gitIgnoreParsers.Keys.Select(key => key.GetRelativePathTo(package.ModRootDirectory)));
                        _gitIgnoreInfoText = Language.Get("GitIgnore.Info.GitIgnores", gitIgnoreParsers.Count, formattedParsers);
                    }
                }
                return _gitIgnoreInfoText;
            }
        }

        public bool AllowsPublishing(FileSystemInfo file)
        {
            if(gitIgnoreParsers.NullOrEmpty())
            {
                return true;
            }
            return gitIgnoreParsers.Keys.All(parserFile => ParserAllows(parserFile, file));
        }

        private bool ParserAllows(FileInfo parserFile, FileSystemInfo info)
        {
            string relativePath = info.GetRelativePathTo(parserFile.Directory);
            if(relativePath.NullOrEmpty())
            {
                return true;
            }
            return gitIgnoreParsers[parserFile].Accepts(relativePath);
        }

        public bool IsActive => package.SerializedData.GitIgnore.UseGitIgnore;

        public void Reset()
        {
            package.SerializedData.GitIgnore.UseGitIgnore = false;
            ParseGitIgnore();
        }

        public void ParseGitIgnore()
        {
            gitIgnoreParsers = package.AllFiles.Where(item => item.Name == GitIgnoreName)
                .ToDictionary(file => file, file => new GitignoreParser(file.FullName, Encoding.UTF8));
            _gitIgnoreInfoText = null;
        }
    }
}
