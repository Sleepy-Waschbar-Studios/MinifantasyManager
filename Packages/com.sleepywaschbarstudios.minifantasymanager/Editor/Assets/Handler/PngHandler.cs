#nullable enable

using System;
using System.IO;
using MinifantasyManager.Runtime.Assets.Temporary;
using Unity.SharpZipLib.Zip;
using UnityEngine;

namespace MinifantasyManager.Editor.Assets.Handler
{
    public class IgnoreHandler : IHandler
    {
        public TemporaryAsset? HandleFile(string path, ZipFile file, ZipEntry entry)
        {
            return null;
        }
    }

    public class PngHandler : IHandler
    {
        public TemporaryAsset? HandleFile(string path, ZipFile file, ZipEntry entry)
        {
            //using var ms = new MemoryStream();
            //using var stream = file.GetInputStream(entry);
            //stream.CopyTo(ms);
            //ms.Seek(0, SeekOrigin.Begin);
            //var texAsset = new Texture2D(1, 1);
            //if (!texAsset.LoadImage(ms.GetBuffer()))
            //{
            //    throw new BadImageFormatException($"{path} is not a valid .png file.");
            //}
            return new TemporaryImageAsset(path, () => file.GetInputStream(entry));
        }
    }

    public class TextHandler : IHandler
    {
        public TemporaryAsset? HandleFile(string path, ZipFile file, ZipEntry entry)
        {
            using var stream = file.GetInputStream(entry);
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            return new TemporaryTextAsset(path, text);
        }
    }

    public interface IHandler
    {
        public TemporaryAsset? HandleFile(string path, ZipFile file, ZipEntry entry);
    }
}