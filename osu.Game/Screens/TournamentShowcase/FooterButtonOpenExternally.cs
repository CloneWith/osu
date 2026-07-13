using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class FooterButtonOpenExternally : ScreenFooterButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Icon = FontAwesome.Solid.FolderOpen;
            Text = EditorStrings.EditExternally;
            AccentColour = colour.Orange1;
        }
    }
}
