using MinifantasyManager.Editor.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable

public abstract class PackageDefinition
{
    public static Dictionary<string, PackageDefinition> PackageLookups = new();

    static PackageDefinition()
    {
        PackageLookups = typeof(PackageDefinition).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(PackageDefinition)))
            .Select(t => (PackageDefinition)Activator.CreateInstance(t))
            .ToDictionary(t => t.Name);
    }

    public string Name { get; set; }

    public string Version { get; set; }

    protected PackageDefinition(string name, string version)
    {
        Name = name;
        Version = version;
    }

    public abstract IEnumerable<LoadedAsset> Classify(FileTree assetGroup);
}