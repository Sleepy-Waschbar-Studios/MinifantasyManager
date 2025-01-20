#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MinifantasyManager.Runtime.Assets.Temporary;

namespace MinifantasyManager.Editor.Assets
{
    public class RootFileTree
    {
        private FileTree? Tree = null;

        // Zip traversal in c# seems to be BFS/DFS
        // (I'm not going to test to confirm which one)
        // which means that having a fast insert parent
        // will speed up most inserts
        private FileTree? FastInsertParent = null;

        public void Insert(TemporaryAsset file)
        {
            if (FastInsertParent != null && file.FullDirPath.StartsWith(FastInsertParent.FullPath, StringComparison.InvariantCultureIgnoreCase))
            {
                FastInsertParent.Insert(file);
            }
            else
            {
                Tree ??= new FileTree(file.Segments.Last(), file.Segments.Last());
                FastInsertParent = Tree.Insert(file);
            }
        }
    }

    public class FileTree
    {
        public string Name { get; set; }
        public string FullPath { get; set; }

        public List<FileTree> SubTrees = new();
        public List<TemporaryAsset> Files = new();

        public FileTree(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }

        public FileTree Insert(TemporaryAsset file)
        {
            // 1. Found parent
            if (file.FullDirPath.Equals(FullPath, StringComparison.InvariantCultureIgnoreCase))
            {
                this.Files.Add(file);
                return this;
            }

            // 2. Else get relative difference and create file tree structure
            string relative = Path.GetRelativePath(FullPath, file.FullDirPath);

        }
    }
}