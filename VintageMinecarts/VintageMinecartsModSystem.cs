using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

using VintageMinecarts.ModBlock;
using VintageMinecarts.ModEntity;
using VintageMinecarts.ModNetwork;

namespace VintageMinecarts
{
    public class VintageMinecartsModSystem : ModSystem
    {
        private IServerNetworkChannel serverChannel;
        private ICoreAPI api;

        public override void StartPre(ICoreAPI api)
        {
            string cfgFileName = "VintageMinecarts.json";

            try 
            {
                VintageMinecartsConfig cfgFromDisk;
                if ((cfgFromDisk = api.LoadModConfig<VintageMinecartsConfig>(cfgFileName)) == null)
                {
                    api.StoreModConfig(VintageMinecartsConfig.Loaded, cfgFileName);
                }
                else
                {
                    VintageMinecartsConfig.Loaded = cfgFromDisk;
                }
            } 
            catch 
            {
                api.StoreModConfig(VintageMinecartsConfig.Loaded, cfgFileName);
            }

            base.StartPre(api);
        }

        public override void Start(ICoreAPI api)
        {
            this.api = api;
            base.Start(api);

            api.Logger.Notification("Loaded Vintage Minecarts!");

            api.RegisterBlockClass("BlockMinecartRails", typeof(BlockMinecartRails));
            api.RegisterEntity("EntityMinecart", typeof(EntityMinecart));
            api.RegisterMountable("EntityMinecartSeat", EntityMinecartSeat.GetMountable);
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            // Send connecting players config settings
            this.serverChannel.SendPacket(
                new SyncConfigClientPacket {
                    MaxCartSpeedMetersPerSecond = VintageMinecartsConfig.Loaded.MaxCartSpeedMetersPerSecond
                }, player);
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            sapi.Event.PlayerJoin += this.OnPlayerJoin; 
            
            // Create server channel for config data sync
            this.serverChannel = sapi.Network.RegisterChannel("vintageminecarts")
                .RegisterMessageType<SyncConfigClientPacket>()
                .SetMessageHandler<SyncConfigClientPacket>((player, packet) => {});
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            // Sync config settings with clients
            capi.Network.RegisterChannel("vintageminecarts")
                .RegisterMessageType<SyncConfigClientPacket>()
                .SetMessageHandler<SyncConfigClientPacket>(p => {
                    this.Mod.Logger.Event("Received config settings from server");
                    VintageMinecartsConfig.Loaded.MaxCartSpeedMetersPerSecond = p.MaxCartSpeedMetersPerSecond;
                });
        }
        
        public override void Dispose()
        {
            if (this.api is ICoreServerAPI sapi)
            {
                sapi.Event.PlayerJoin -= this.OnPlayerJoin;
            }
        }
    }
}