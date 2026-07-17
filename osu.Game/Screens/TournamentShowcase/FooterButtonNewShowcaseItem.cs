using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class FooterButtonNewShowcaseItem : ScreenFooterButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Icon = FontAwesome.Solid.Plus;
            Text = TournamentShowcaseStrings.AddBeatmap;
            AccentColour = colour.Blue1;
        }
    }
}
