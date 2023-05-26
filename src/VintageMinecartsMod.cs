using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

using VintageMinecarts.ModBlock;
using VintageMinecarts.ModEntity;

namespace VintageMinecarts
{
    public class VintageMinecartsMod : ModSystem
    {
        private Harmony _harmony;

        public static VintageMinecartsMod Instance { get; private set; }

        public ICoreClientAPI CApi { get; private set; }
        public ICoreServerAPI SApi { get; private set; }
        public ICoreAPI Api { get; private set; }

        public bool Debug { get; private set; }

        public override void StartClientSide(ICoreClientAPI api)
        {
             CApi = api;

            RegisterHotkeys();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            SApi = api;

            PatchGame();
        }

        public override void StartPre(ICoreAPI api)
        {
            SetConfigDefaults();

            Instance = this;
           
            Debug = Environment.GetEnvironmentVariable("VINTAGEMINECARTS_DEBUG").ToBool();
            if (Debug)
            {
                Mod.Logger.Event("Debugging activated");
            }
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            Api = api;

            VintageMinecartsMod.Instance.Api.RegisterBlockClass("BlockMinecartRails", typeof(BlockMinecartRails));
            VintageMinecartsMod.Instance.Api.RegisterEntity("EntityMinecart", typeof(EntityMinecart));
            VintageMinecartsMod.Instance.Api.RegisterMountable("EntityMinecartSeat", EntityMinecartSeat.GetMountable);
        }

        private void RegisterHotkeys()
        {

        }

        private void PatchGame()
        {
            Mod.Logger.Event("Loading harmony for patching...");
            Harmony.DEBUG = Debug;
            _harmony = new Harmony("com.zach2039.vintageminecarts");
            _harmony.PatchAll();

            var myOriginalMethods = _harmony.GetPatchedMethods();
            foreach (var method in myOriginalMethods)
            {
                Mod.Logger.Event("Patched " + method.FullDescription());
            }
        }

        private static void SetConfigDefaults()
        {
            
        }

        public override void Dispose()
        {
            if (Api == null) return;

            _harmony?.UnpatchAll();

            Instance = null;
        }
    }
}