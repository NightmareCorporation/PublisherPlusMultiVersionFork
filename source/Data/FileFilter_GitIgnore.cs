using GitignoreParserNet;
using System;
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
		private Dictionary<FileSystemInfo, GitignoreParser> gitIgnoreParsers;

		private const string GitIgnoreName = ".gitignore";

		ManagedWorkshopPackage package;

		public void SetWorkshopPackage(ManagedWorkshopPackage package)
		{
			this.package = package;
			ParseGitIgnore();
		}

		public string FilterReason => ".gitignore";

		public string GitIgnoreInfoText => gitIgnoreParsers.NullOrEmpty() ?
			Language.Get("GitIgnore.Info.NoGitIgnoreFound") :
			Language.Get("GitIgnore.Info.GitIgnores", gitIgnoreParsers.Count, String.Join("\n", gitIgnoreParsers.Keys.Select(key => key.FullName)));

		public bool AllowsPublishing(FileSystemInfo file)
		{
			if(gitIgnoreParsers.NullOrEmpty())
			{
				return true;
			}
			return gitIgnoreParsers.All(kvp =>
			{
				string relativePath = file.GetRelativePathTo(kvp.Key);
				return relativePath == null || kvp.Value.Accepts(relativePath);
			});
		}

		public bool IsActive => package.SerializedData.GitIgnore.UseGitIgnore;

		public void Reset()
		{
			package.SerializedData.GitIgnore.UseGitIgnore = false;
		}

		public void ParseGitIgnore()
		{
			gitIgnoreParsers = package.AllFiles.Where(item => item.Name == GitIgnoreName)
				.ToDictionary(file => file, file => new GitignoreParser(file.FullName, Encoding.UTF8));
			if(gitIgnoreParsers.NullOrEmpty())
			{
				Startup.Error($"Could not find and parse any .gitignore files");
			}
			else
			{
				Startup.Log($"Parsed {gitIgnoreParsers.Count} .gitignore files located at: \n{String.Join("\n", gitIgnoreParsers.Keys)}");
			}
		}
	}
}
