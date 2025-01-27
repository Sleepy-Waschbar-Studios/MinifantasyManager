#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PackageUtils
{
    public static string? PackageName { get; set; }
    public static string? UniqueId { get; set; }
    public static HashSet<string> PathsToHandle { get; set; }

    public static void SetUniqueId()
    {
        UniqueId = Guid.NewGuid().ToString();
    }

    public static void ClearUniqueId()
    {
        UniqueId = null;
    }

    public static string GetUniqueId()
    {
        return UniqueId ?? throw new NullReferenceException("UniqueId not inititalized");
    }
    

    public static string GetPackageName()
    {
        return PackageName ?? throw new NullReferenceException("PackageName not inititalized");
    }

    public static string GetAssetPathToFile(string path)
    {
        return Path.Combine(Application.dataPath, path);
    }

    public static void EnsureAssetFoldersExist(string path)
    {
        if (Path.IsPathFullyQualified(path))
        {
            path = Path.GetRelativePath(Application.dataPath, path);
        }

        if (Directory.Exists(Path.Combine(Application.dataPath, path))) return;

        string pathSoFar = "Assets/";
        foreach (var component in path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            string newPath = Path.Combine(pathSoFar, component);
            try
            {
                if (Directory.Exists(Path.Combine(Path.GetDirectoryName(Application.dataPath), newPath))) continue;
                AssetDatabase.CreateFolder(pathSoFar, component);
            }
            finally
            {
                pathSoFar = newPath;
            }
        }

        //AssetDatabase.Refresh();
        //if (!AssetDatabase.IsValidFolder(Path.Combine("Assets", path)))
        //{
        //    throw new Exception("AssetDatabase didn't refresh.");
        //} 
    }

    private static string GetTempPath(string? asset = null)
    {
        var path = Path.Combine("MinifantasyManager", "TempFilesToLoadWillBeDeleted", GetUniqueId(), GetPackageName());
        if (asset != null)
        {
            path = Path.Combine(path, asset);
        }
        return GetAssetPathToFile(path);
    }

    public static void CreateMainAsset(ScriptableObject asset, string path)
    {
        path = GetTempPath(path);
        EnsureAssetFoldersExist(Path.GetDirectoryName(path));
        path = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path));
        AssetDatabase.CreateAsset(asset, path);
    }

    public static void MoveAsset(UnityEngine.Object obj, string path)
    {
        path = GetTempPath(path);
        EnsureAssetFoldersExist(Path.GetDirectoryName(path));
        path = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path));
        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), Path.GetFileName(path));
        var result = AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(obj), path);
        if (!string.IsNullOrEmpty(result)) throw new Exception(result);
    }

    public static void CreateTemporaryPackage(string packageName)
    {
        PackageName = packageName;
        EnsureAssetFoldersExist(GetTempPath());
    }

    /// <summary>
    /// This is purely done so we can do an initial import, this is then moved to a real asset.
    /// </summary>
    public static T WriteFileToTemporaryAsset<T>(string name, string ext, Stream stream) where T : UnityEngine.Object
    {
        var path = GetTempPath(Path.Combine("TempLoaded", $"{name}-{Guid.NewGuid()}{ext}"));
        EnsureAssetFoldersExist(Path.GetDirectoryName(path));
        using (var fileWriter = File.OpenWrite(path))
        {
            stream.CopyTo(fileWriter);
            fileWriter.Flush();
        }

        path = Path.Combine("Assets", Path.GetRelativePath(Application.dataPath, path));
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
}