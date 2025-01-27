#nullable enable

using MinifantasyManager.Editor.Assets;
using MinifantasyManager.Runtime.Assets.Temporary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class DungeonPackageDefinition : PackageDefinition
{
    public DungeonPackageDefinition() : base("Dungeon", "v2.2")
    {
    }

    public override IEnumerable<LoadedAsset> Classify(FileTree assetGroup)
    {
        return
            DefineTileset(assetGroup.Find(AssetTypes.Tileset), "Tileset", "Shadows", "DungeonTileset")
            .BeginClassify(8, 8, subAsset =>
            {
                subAsset.ClassifyNext("")
            }).Load();
    }

    public TilesetBuilder DefineTileset(FileTree group, string tileset, string shadow, string name)
    {
        return new TilesetBuilder(group, tileset, shadow, name);
    }

    public IEnumerable<LoadedAsset> DefineProps(FileTree group, string tileset, string shadow)
    {
        yield break;
    }
}

public abstract class AssetBuilder
{
    public abstract IEnumerable<LoadedAsset> Load();
}

public class PropsBuilder : AssetBuilder
{
    public override IEnumerable<LoadedAsset> Load()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// The responsibility of this class is to define the sub assets
/// that exist within each spritesheet.  This is used by both props & tileset.
/// </summary>
public class SubAssetBuilder
{
    private readonly string name;
    private readonly TilesetLoadedAsset tileset;
    private readonly Vector2Int size;
    private readonly Texture2D baseImageAsset;
    private readonly Texture2D? shadowsAsset;
    private ISpriteEditorDataProvider data;
    private List<Rect> spriteRects;
    private List<SpriteRect> resultantRects;
    private Texture2D tex;

    public SubAssetBuilder(ScriptableObject baseObject, string name, TilesetLoadedAsset tileset, Vector2Int size)
    {
        this.name = name;
        this.tileset = tileset;
        this.size = size;

        // Let's create the asset!
        this.baseImageAsset = InitBaseAsset(this.tileset.Tileset, "Tileset.png");

        //if (this.tileset.ShadowTileset != null)
        //{
        //    this.shadowsAsset = InitBaseAsset(this.tileset.ShadowTileset, "Tileset_Shadows.png");
        //}
    }

    public class RuleTileBuilder
    {
        public SubAssetBuilder Parent;

        public RuleTileBuilder(SubAssetBuilder parent)
        {
            Parent = parent;
        }

        public void Complete()
        {

        }
    }

    public SubAssetBuilder DefineRuleTile(Action<RuleTileBuilder> handler)
    {
        var builder = new RuleTileBuilder(this);
        handler(builder);
        builder.Complete();

        return this;
    }

    public SubAssetBuilder Skip(int n)
    {
        if (spriteRects.Count < n) throw new Exception("Not enough assets to pop");
        spriteRects.RemoveRange(0, n);
        return this;
    }

    public SubAssetBuilder ClassifyNext(string name)
    {
        if (spriteRects.Count == 0) throw new Exception("Didn't expect any more assets.");

        var next = spriteRects.First();
        spriteRects.RemoveAt(0);



        return this;
    }

    private Texture2D InitBaseAsset(TemporaryImageAsset asset, string assetName)
    {
        PackageUtils.PathsToHandle.Remove(asset.FullPath);
        tex = this.tileset.Tileset.CreateAsset();
        // Let's then add this texture to our main "resource"
        PackageUtils.MoveAsset(tex, Path.Combine(name, assetName));

        var path = AssetDatabase.GetAssetPath(tex);
        // Set basic set of settings
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.spritePixelsPerUnit = 8;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.textureType = TextureImporterType.Sprite;
        importer.filterMode = FilterMode.Point;
        importer.spriteImportMode = SpriteImportMode.Multiple;

        var importerSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(importerSettings);
        importerSettings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(importerSettings);
        importer.SaveAndReimport();

        var pixelData = tex.GetPixels() ?? throw new Exception("No pixel data");

        // Let's now generate the set of sprite rects
        spriteRects = new List<Rect>();
        for (int y = 0; y < tex.height; y += size.y)
        {
            for (int x = 0; x < tex.width; x += size.x)
            {
                var baseRect = new Rect(x, y, size.x, size.y);
                // See if the cell is empty or not
                if (IsCellEmpty(baseRect)) continue;

                spriteRects.Add(baseRect);
            }
        }

        bool IsCellEmpty(Rect rect)
        {
            for (int y = (int)rect.y; y <= (int)(rect.y + size.y); y++)
            {
                for (int x = (int)rect.x; x <= (int)(rect.x + size.x); x++)
                {
                    if (pixelData![y * size.x + x].a > 0) return false;
                }
            }
            return true;
        }

        return tex;
    }

    public void Complete()
    {
        var factory = new SpriteDataProviderFactories();
        factory.Init();

        data = factory.GetSpriteEditorDataProviderFromObject(tex);
        data.InitSpriteEditorDataProvider();

        data.SetSpriteRects(resultantRects.ToArray());

        var spriteNameFileIdDataProvider = data.GetDataProvider<ISpriteNameFileIdDataProvider>();
        var nameFileIdPairs = resultantRects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList();
        spriteNameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);

        data.Apply();
        ((AssetImporter)data.targetObject).SaveAndReimport();
    }
}

public class TilesetBuilder : AssetBuilder
{
    private string name;
    private TilesetLoadedMetadata baseObject;
    private FileTree group;
    private string tileset;
    private string shadow;
    private TilesetLoadedAsset tilesetAsset;

    public TilesetBuilder(FileTree group, string tileset, string shadow, string name)
    {
        this.group = group;
        this.tileset = tileset;
        this.shadow = shadow;
        this.tilesetAsset = new TilesetLoadedAsset(group.FindAsset<TemporaryImageAsset>(tileset), group.FindAsset<TemporaryImageAsset>(shadow));
        this.name = name;
        this.baseObject = ScriptableObject.CreateInstance<TilesetLoadedMetadata>();
        PackageUtils.CreateMainAsset(this.baseObject, name + ".asset");
    }

    // This is where you can "classify" / contextualise parts of the tileset, i.e. specify things are walls or floors for example.
    // We can also support animations and so on.
    public TilesetBuilder BeginClassify(int sizeW, int sizeH, Action<SubAssetBuilder> handler)
    {
        SubAssetBuilder subAsset = new SubAssetBuilder(baseObject, name, tilesetAsset, new(sizeW, sizeH));
        handler(subAsset);
        subAsset.Complete();
        
        return this;
    }

    public override IEnumerable<LoadedAsset> Load()
    {
        yield return tilesetAsset;
    }
}
