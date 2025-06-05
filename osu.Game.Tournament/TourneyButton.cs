// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tournament
{
    public partial class TourneyButton : RoundedButton
    {
        public new Box Background => base.Background;

        public TourneyButton(HoverSampleSet? hoverSounds = HoverSampleSet.Default)
            : base(hoverSounds)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Padding = new MarginPadding();
        }
    }
}
