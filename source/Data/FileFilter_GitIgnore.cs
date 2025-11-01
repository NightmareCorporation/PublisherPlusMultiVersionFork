using GitignoreParserNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Verse;

namespace PublisherPlus.Data
{
    public class FileFilter_GitIgnore : FileFilter
    {
        /// <summary>
        /// There can be multiple .gitignore files located throughout a solution, each gitignore must apply its filters relative to the file location
        /// </summary>
        private Dictionary<FileInfo, GitignoreParser> gitIgnoreParsers;

        public override string FilterReasonKey => "FilterReason.Gitignore";
        public override bool IsActive => package.SerializedData.GitIgnore.UseGitIgnore;

        private string gitIgnoreInfoText;
        public string GitIgnoreInfoText => gitIgnoreInfoText;

        public FileFilter_GitIgnore(ManagedWorkshopPackage package) : base(package)
        {
            ParseGitIgnore();
        }

        public override bool AllowsPublishing(FileSystemInfo file)
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

        public override void Reset()
        {
            package.SerializedData.GitIgnore.UseGitIgnore = false;
            ParseGitIgnore();
        }

        public void ParseGitIgnore()
        {
            const string pattern = ".gitignore";

            gitIgnoreParsers = package.ModRootDirectory.GetFiles(pattern, SearchOption.AllDirectories)
                .ToDictionary(file => file, file => new GitignoreParser(file.FullName, Encoding.UTF8));

            if(gitIgnoreParsers.NullOrEmpty())
            {
                gitIgnoreInfoText = Language.Get("GitIgnore.Info.NoGitIgnoreFound");
            }
            else
            {
                string formattedParsers = string.Join("\n", gitIgnoreParsers.Keys.Select(key => key.GetRelativePathTo(package.ModRootDirectory)));
                gitIgnoreInfoText = Language.Get("GitIgnore.Info.GitIgnores", gitIgnoreParsers.Count, formattedParsers);
            }
        }
    }
}
