#nullable enable

using System.IO.Compression;

namespace MinifantasyManager.Runtime.Extensions
{
    public static class ZipEntryExtensions
    {
        // IsDirectory/IsFile inspired from https://github.com/icsharpcode/SharpZipLib/blob/master/src/ICSharpCode.SharpZipLib/Zip/ZipEntry.cs
        // but we ignore the attributes.

        /// <summary>
		/// Gets a value indicating if the entry is a directory.
		/// however.
		/// </summary>
		/// <remarks>
		/// A directory is determined by an entry name with a trailing slash '/'.
		/// </remarks>
		public static bool IsDirectory(this ZipArchiveEntry entry) {
            var name = entry.Name;
            return name.Length > 0 && (name[^1] == '/' || name[^1] == '\\');
        }

		/// <summary>
		/// Get a value of true if the entry appears to be a file; false otherwise
		/// </summary>
		public static bool IsFile(this ZipArchiveEntry entry) => !entry.IsDirectory();
    }
}