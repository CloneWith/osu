// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTournamentHeaderText : CompositeDrawable
    {
        private readonly TournamentSpriteText nameText;
        private readonly Sprite headerSprite;

        public DrawableTournamentHeaderText(bool center = true)
        {
            InternalChildren = new Drawable[]
            {
                nameText = new TournamentSpriteText
                {
                    Anchor = center ? Anchor.Centre : Anchor.TopLeft,
                    Origin = center ? Anchor.Centre : Anchor.TopLeft,
                    Font = OsuFont.Torus.With(size: 22, weight: FontWeight.Bold),
                },
                headerSprite = new Sprite
                {
                    Anchor = center ? Anchor.Centre : Anchor.TopLeft,
                    Origin = center ? Anchor.Centre : Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                },
            };

            Height = 22;
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, TextureStore textures)
        {
            var headerTexture = textures.Get("header-text");

            headerSprite.Texture = headerTexture;
            nameText.Alpha = headerTexture != null ? 0 : 1;
            headerSprite.Alpha = headerTexture != null ? 1 : 0;

            ladder.FullName.BindValueChanged(e => nameText.Text = e.NewValue, true);
        }
    }
}
