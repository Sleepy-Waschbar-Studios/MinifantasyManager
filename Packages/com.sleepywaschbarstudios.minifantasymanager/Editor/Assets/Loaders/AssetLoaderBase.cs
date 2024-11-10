using MinifantasyManager.Runtime.Assets;
using MinifantasyManager.Runtime.Assets.Temporary;

namespace MinifantasyManager.Editor.Assets.Loaders
{
    public abstract class AssetLoaderBase
    {
        public abstract bool TryLoad(TemporaryLoadedDetails details, ManagerMetadata currentMetadata, TemporaryAsset asset);
    }
}