#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using MinifantasyManager.Runtime.Assets.Temporary;

namespace MinifantasyManager.Editor.Assets
{

    public class FileTree
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public int Depth { get; set; }

        public List<FileTree> SubTrees = new();
        public List<TemporaryAsset> Files = new();

        public FileTree(string name, string fullPath, int depth)
        {
            Name = name;
            FullPath = fullPath;
            Depth = depth;
        }

        public FileTree Insert(TemporaryAsset file)
        {
            // Note: pretty horribly written, but shouldn't matter.

            // 1. Found parent
            if (file.FullDirPath.Equals(FullPath, StringComparison.OrdinalIgnoreCase))
            {
                this.Files.Add(file);
                return this;
            }

            // 2. Else get relative difference and create file tree structure
            FileTree parent = this;
            var nodes = file.FullDirPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[Depth..];
            foreach (var node in nodes)
            {
                var cur = parent.SubTrees.Find(t => t.Name.Equals(node, StringComparison.OrdinalIgnoreCase));

                if (cur == null)
                {
                    cur = new FileTree(node, Path.Join(parent.FullPath, node), Depth + 1);
                    parent.SubTrees.Add(cur);
                }

                parent = cur;
            }

            parent.Files.Add(file);
            return parent;
        }
    }
}