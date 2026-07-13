using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Screens.Footer;
using osu.Game.Tournament.Localisation;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class FooterButtonRefresh : ScreenFooterButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Icon = FontAwesome.Solid.Sync;
            Text = BaseStrings.Refresh;
            AccentColour = colour.Lime1;
        }
    }
}
