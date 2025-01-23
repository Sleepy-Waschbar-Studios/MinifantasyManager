#nullable enable

using MinifantasyManager.Runtime.Assets.Temporary;

public abstract class LoadedAsset
{

}

public class TilesetLoadedAsset : LoadedAsset
{
    public TilesetLoadedAsset(TemporaryImageAsset tileset, TemporaryImageAsset shadowTileset)
    {
        Tileset = tileset;
        ShadowTileset = shadowTileset;
    }

    public TemporaryImageAsset Tileset { get; set; }
    public TemporaryImageAsset ShadowTileset { get; set; }
}