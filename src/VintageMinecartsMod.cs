using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace VintageMinecarts
{
    public class VintageMinecartsMod : ModSystem
    {
        private Harmony _harmony;

        public static VintageMinecartsMod Instance { get; private set; }

        public ICoreClientAPI CApi { get; private set; }
        public bool Debug { get; private set; }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            PatchGame();
            RegisterHotkeys();
        }

        public override void StartPre(ICoreAPI api)
        {
            if (!(api is ICoreClientAPI clientApi)) return;

            SetConfigDefaults();

            Instance = this;
            CApi = clientApi;
            
            Debug = Environment.GetEnvironmentVariable("VINTAGEMINECARTS_DEBUG").ToBool();
            if (Debug)
            {
                Mod.Logger.Event("Debugging activated");
            }
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
            if (CApi == null) return;

            _harmony?.UnpatchAll();

            Instance = null;
        }
    }
}