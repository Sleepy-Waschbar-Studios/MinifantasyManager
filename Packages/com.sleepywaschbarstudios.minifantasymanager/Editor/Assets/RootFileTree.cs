#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MinifantasyManager.Editor.Assets.Loaders;
using MinifantasyManager.Runtime.Assets.Temporary;
using Unity.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine;

namespace MinifantasyManager.Editor.Assets
{
    public class RootFileTree : IDisposable
    {
        public FileTree? Tree = null;
        private ZipFile ZipFile;
        public HashSet<string> AllLoadedFiles = new();

        public RootFileTree(ZipFile zipFile)
        {
            ZipFile = zipFile;
        }

        // Zip traversal in c# seems to be BFS/DFS
        // (I'm not going to test to confirm which one)
        // which means that having a fast insert parent
        // will speed up most inserts
        private FileTree? FastInsertParent = null;
        private bool disposed;

        private const string ProgressTitle = "Importing Minifantasy Assets";
        private const string ProgressDescription = "Building Asset Tree";

        public void Insert(TemporaryAsset file)
        {
            if (FastInsertParent != null && file.FullDirPath.StartsWith(FastInsertParent.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                FastInsertParent.Insert(file);
            }
            else
            {
                Tree ??= new FileTree(file.Segments.Last(), file.Segments.Last(), 1);
                FastInsertParent = Tree.Insert(file);
            }
        }

        public static RootFileTree? OpenFromFile(string path, bool showProgress)
        {
            var unzip = new ZipFile(new FileStream(path, FileMode.Open), leaveOpen: false);
            var tree = new RootFileTree(unzip);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            if (showProgress && EditorUtility.DisplayCancelableProgressBar(ProgressTitle, ProgressDescription, 0.0f))
            {
                Debug.LogWarning("Cancelled");
                return null;
            }

            int progress = 0;
            var count = unzip.Count;

            // Saves about 75% ish for the large patreon file (4s to <<1s, I've had it as low as 200ms to load :D).
            // I thought I should use parallel to load, but that makes literally no difference, it's just very fast library.
            foreach (var entry in unzip.Cast<ZipEntry>())
            {
                progress++;
                if (showProgress && EditorUtility.DisplayCancelableProgressBar(ProgressTitle, "Building Asset Tree", progress / (float)count))
                {
                    Debug.LogWarning("Cancelled");
                    return null;
                }

                // Skip dirs
                if (entry.IsDirectory) continue;

                var entryPath = entry.Name;

                // Safe files to skip
                if (PackageLoader.FilesToSkip.Contains(Path.GetFileName(entry.Name))) continue;

                // Skip files that we don't care about.
                if (entryPath.Contains("Premade", StringComparison.OrdinalIgnoreCase)) continue;
                if (entryPath.Contains("Mockup", StringComparison.OrdinalIgnoreCase)) continue;
                if (entryPath.Contains("_GIFs", StringComparison.OrdinalIgnoreCase)) continue;
                if (entryPath.Contains("Legacy", StringComparison.OrdinalIgnoreCase)) continue;

                // Use extension to determine handler
                var extension = Path.GetExtension(entryPath);
                if (!PackageLoader.ExtensionHandlers.TryGetValue(extension, out var handler))
                {
                    Debug.LogError($"Found an extension we couldn't handle {extension}, ignoring.");
                    continue;
                }

                var asset = handler.HandleFile(entryPath, unzip, entry);
                if (asset == null) continue;

                if (asset.Segments.Length == 0)
                {
                    Debug.LogWarning($"Skipped file {entryPath} because it didn't match a folder structure we were familiar with.");
                    continue;
                }

                tree.AllLoadedFiles.Add(asset.FullPath);
                tree.Insert(asset);
            }

            if (showProgress) EditorUtility.ClearProgressBar();
            Debug.LogFormat("Built full tree in {0} found {1} files", sw.Elapsed, progress);

            return tree;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ((IDisposable)ZipFile).Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}