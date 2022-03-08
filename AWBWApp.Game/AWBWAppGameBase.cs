using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AWBWApp.Game.API;
using AWBWApp.Game.Game.Building;
using AWBWApp.Game.Game.COs;
using AWBWApp.Game.Game.Country;
using AWBWApp.Game.Game.Tile;
using AWBWApp.Game.Game.Units;
using AWBWApp.Game.Helpers;
using AWBWApp.Game.IO;
using AWBWApp.Game.UI;
using AWBWApp.Resources;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;

namespace AWBWApp.Game
{
    public class AWBWAppGameBase : osu.Framework.Game
    {
        // Anything in this class is shared between the test browser and the game implementation.
        // It allows for caching global dependencies that should be accessible to tests, or changing
        // the screen scaling for all components including the test browser and framework overlays.

        protected override Container<Drawable> Content { get; }

        protected AWBWConfigManager LocalConfig { get; set; }

        protected Storage HostStorage { get; set; }

        private NearestNeighbourTextureStore unfilteredTextures;
        private DependencyContainer dependencies;
        private ResourceStore<byte[]> fileStorage;
        private ReplayManager replayStorage;
        private MapFileStorage mapStorage;

        private TerrainTileStorage terrainTileStorage;
        private BuildingStorage buildingStorage;
        private UnitStorage unitStorage;
        private COStorage coStorage;
        private CountryStorage countryStorage;

        private InterruptDialogueOverlay interruptOverlay;
        private AWBWSessionHandler sessionHandler;

        protected AWBWAppGameBase()
        {
            // Ensure game and tests scale with window size and screen DPI.
            base.Content.Add(Content = new DrawSizePreservingFillContainer
            {
                // You may want to change TargetDrawSize to your "default" resolution, which will decide how things scale and position when using absolute coordinates.
                TargetDrawSize = new Vector2(1366, 768)
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Resources.AddStore(new DllResourceStore(typeof(AWBWAppResources).Assembly));

            dependencies.CacheAs(this);
            dependencies.Cache(HostStorage);
            dependencies.Cache(LocalConfig);

            unfilteredTextures = new NearestNeighbourTextureStore(Textures);
            dependencies.Cache(unfilteredTextures);

            fileStorage = new ResourceStore<byte[]>(Resources);
            fileStorage.AddExtension(".json");
            dependencies.Cache(fileStorage);

            replayStorage = new ReplayManager(HostStorage);
            dependencies.Cache(replayStorage);

            mapStorage = new MapFileStorage(HostStorage);
            dependencies.Cache(mapStorage);

            var tilesJson = fileStorage.GetStream("Json/Tiles");
            terrainTileStorage = new TerrainTileStorage();
            terrainTileStorage.LoadStream(tilesJson);
            dependencies.Cache(terrainTileStorage);

            var buildingsJson = fileStorage.GetStream("Json/Buildings");
            buildingStorage = new BuildingStorage();
            buildingStorage.LoadStream(buildingsJson);
            dependencies.Cache(buildingStorage);

            var unitsJson = fileStorage.GetStream("Json/Units");
            unitStorage = new UnitStorage();
            unitStorage.LoadStream(unitsJson);
            dependencies.Cache(unitStorage);

            var cosJson = fileStorage.GetStream("Json/COs");
            coStorage = new COStorage();
            coStorage.LoadStream(cosJson);
            dependencies.Cache(coStorage);

            var countriesJson = fileStorage.GetStream("Json/Countries");
            countryStorage = new CountryStorage();
            countryStorage.LoadStream(countriesJson);
            dependencies.Cache(countryStorage);

            Add(interruptOverlay = new InterruptDialogueOverlay());
            dependencies.Cache(interruptOverlay);

            sessionHandler = new AWBWSessionHandler();
            dependencies.Cache(sessionHandler);
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            HostStorage ??= host.Storage;

            LocalConfig ??= new AWBWConfigManager(HostStorage);
        }

        public virtual Version AssemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

        public bool IsDeployedBuild => AssemblyVersion.Major > 0 || AssemblyVersion.Minor > 0;

        public virtual string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return @"local " + (DebugUtils.IsDebugBuild ? @"debug" : @"release");

                var version = AssemblyVersion;
                return $@"{version.Major}.{version.Minor}.{version.Build}-lazer";
            }
        }

        public async Task Import(params string[] paths)
        {
            if (paths.Length == 0)
                return;

            //Todo: Are we going to have any other extensions?

            foreach (var path in paths)
            {
                if (Path.GetExtension(path) != ".zip")
                    continue;

                await replayStorage.ParseAndStoreReplay(path);
            }
        }

        protected override UserInputManager CreateUserInputManager() => new AWBWAppUserInputManager();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
    }
}
