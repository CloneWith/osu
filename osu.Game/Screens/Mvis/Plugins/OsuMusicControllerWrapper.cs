using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Game.Overlays;
using osu.Game.Screens.Mvis.Plugins.Types;
using osu.Game.Screens.Mvis.Skinning;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class OsuMusicControllerWrapper : MvisPlugin, IProvideAudioControlPlugin
    {
        [Resolved]
        private MusicController controller { get; set; }

        public void NextTrack() => controller.NextTrack();

        public void PrevTrack() => controller.PreviousTrack();

        public void TogglePause() => controller.TogglePause();

        public void Seek(double position) => controller.SeekTo(position);

        public DrawableTrack GetCurrentTrack() => controller.CurrentTrack;

        public bool IsCurrent { get; set; }

        protected override Drawable CreateContent() => new PlaceHolder();

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit() => true;

        public override int Version => 1;

        public OsuMusicControllerWrapper()
        {
            Name = "osu!";
        }
    }
}
