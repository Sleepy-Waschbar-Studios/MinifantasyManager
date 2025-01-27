#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MinifantasyManager.Runtime.Assets.Temporary;

namespace MinifantasyManager.Editor.Assets
{
    public enum AssetTypes
    {
        Tileset,
        Props,
        Characters
    }

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

        public T FindAsset<T>(string name) where T: TemporaryAsset
        {
            return Files.FirstOrDefault(f => f.FilenameNoExt.Equals(name, StringComparison.OrdinalIgnoreCase)) as T
                ?? throw new FileNotFoundException("Couldn't find " + name);
        }

        public FileTree Find(string path)
        {
            var components = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var current = this;
            foreach (var component in components)
            {
                current = current.SubTrees.FirstOrDefault(t => t.Name.Equals(component, StringComparison.OrdinalIgnoreCase));
                if (current == null) throw new DirectoryNotFoundException(path);
            }

            return current;
        }


        public FileTree Find(AssetTypes asset)
        {
            foreach (var tree in SubTrees)
            {
                switch (asset)
                {
                    case AssetTypes.Tileset:
                    {
                        if (tree.Name.TrimEnd('s').Equals("Tileset", StringComparison.OrdinalIgnoreCase)) return tree;
                        break;
                    }
                }
            }

            throw new DirectoryNotFoundException(asset.ToString());
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