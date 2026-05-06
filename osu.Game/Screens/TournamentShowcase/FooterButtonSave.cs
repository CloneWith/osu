// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class FooterButtonSave : ScreenFooterButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Icon = FontAwesome.Solid.Save;
            Text = TournamentShowcaseStrings.SaveAction;
            AccentColour = colour.Blue1;
        }
    }
}
