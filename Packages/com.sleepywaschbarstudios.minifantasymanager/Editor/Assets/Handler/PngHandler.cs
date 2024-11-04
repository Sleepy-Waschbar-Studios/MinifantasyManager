#nullable enable

using System;
using System.IO;
using MinifantasyManager.Runtime.Assets.Temporary;
using UnityEngine;

namespace MinifantasyManager.Editor.Assets.Handler
{
    public class IgnoreHandler : IHandler
    {
        public TemporaryAsset? HandleFile(string path, Stream stream)
        {
            return null;
        }
    }

    public class PngHandler : IHandler
    {
        public TemporaryAsset? HandleFile(string path, Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var texAsset = new Texture2D(1, 1);
            if (!texAsset.LoadImage(ms.GetBuffer())) {
                throw new BadImageFormatException($"{path} is not a valid .png file.");
            }

            return new ImageAsset() {
                Name = path,
                Texture = texAsset,
            };
        }
    }

    public class TextHandler : IHandler
    {
        public TemporaryAsset? HandleFile(string path, Stream stream)
        {
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            return new Runtime.Assets.Temporary.TextAsset() {
                Name = path,
                Contents = text,
            };
        }
    }

    public interface IHandler
    {
        public TemporaryAsset? HandleFile(string path, Stream stream);
    }
}