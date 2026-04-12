// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class ChromaBox : Box
    {
        private readonly Color4 chromaGreen = new Color4(0, 255, 0, 255);
        private readonly Color4 chromaBlue = new Color4(0, 0, 255, 255);

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            ladder.UseBlueChroma.BindValueChanged(e =>
                this.FadeColour(e.NewValue ? chromaBlue : chromaGreen, 300, Easing.OutQuint), true);
        }
    }
}
