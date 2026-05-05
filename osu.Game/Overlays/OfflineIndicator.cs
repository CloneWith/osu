// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays
{
    public partial class OfflineIndicator : VisibilityContainer
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.BottomRight;
            Origin = Anchor.Centre;
            Position = new Vector2(-120, -120);

            Alpha = 0;

            Add(new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.Solid.Unlink,
                Size = new Vector2(32),
            });
        }

        protected override void PopIn()
        {
            this.FadeTo(0.7f, 3000, Easing.OutQuint)
                .Then()
                .FadeTo(0.5f, 3000, Easing.OutQuint)
                .Loop(1500);
        }

        protected override void PopOut()
        {
            this.FadeOut(500, Easing.OutQuint);
        }
    }
}
