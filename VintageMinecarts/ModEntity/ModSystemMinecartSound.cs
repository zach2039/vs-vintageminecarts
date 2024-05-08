using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace VintageMinecarts.ModEntity
{
	public class ModSystemMinecartSound : ModSystem
	{
		public override bool ShouldLoad(EnumAppSide forSide)
		{
			return forSide == EnumAppSide.Client;
		}
		public override void StartClientSide(ICoreClientAPI api)
		{
			this.capi = api;
			this.capi.Event.LevelFinalize += this.Event_LevelFinalize;
		}

		private void Event_LevelFinalize()
		{
			this.travelSound = this.capi.World.LoadSound(new SoundParams
			{
				Location = new AssetLocation("sounds/raft-moving.ogg"),
				ShouldLoop = true,
				RelativePosition = false,
				DisposeOnFinish = false,
				Volume = 0f
			});
			this.idleSound = this.capi.World.LoadSound(new SoundParams
			{
				Location = new AssetLocation("sounds/raft-idle.ogg"),
				ShouldLoop = true,
				RelativePosition = false,
				DisposeOnFinish = false,
				Volume = 0.35f
			});
		}

		public void NowInMotion(float velocity)
		{
			if (!this.soundsActive)
			{
				this.idleSound.Start();
				this.soundsActive = true;
			}
			if (velocity > 0f)
			{
				if (!this.travelSound.IsPlaying)
				{
					this.travelSound.Start();
				}
				float volume = GameMath.Clamp((velocity - 0.025f) * 7f, 0f, 1f);
				this.travelSound.FadeTo((double)volume, 0.5f, null);
				return;
			}
			if (this.travelSound.IsPlaying)
			{
				this.travelSound.Stop();
			}
		}

		public override void Dispose()
		{
			ILoadedSound loadedSound = this.travelSound;
			if (loadedSound != null)
			{
				loadedSound.Dispose();
			}
			ILoadedSound loadedSound2 = this.idleSound;
			if (loadedSound2 == null)
			{
				return;
			}
			loadedSound2.Dispose();
		}

		public void NotMounted()
		{
			if (this.soundsActive)
			{
				this.idleSound.Stop();
				this.travelSound.SetVolume(0f);
				this.travelSound.Stop();
			}
			this.soundsActive = false;
		}

		// Token: 0x04000CF1 RID: 3313
		public ILoadedSound travelSound;

		// Token: 0x04000CF2 RID: 3314
		public ILoadedSound idleSound;

		// Token: 0x04000CF3 RID: 3315
		private ICoreClientAPI capi;

		// Token: 0x04000CF4 RID: 3316
		private bool soundsActive;
	}
}
