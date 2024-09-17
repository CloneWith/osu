
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Tournament.Components
{
    public partial class ShortDollyWidget : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Logger.Log("Dajiahaoa");
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = textures.Get("dolly"),
                            // When the size is not set, the texture will be displayed in its original size
                            // Size = new osuTK.Vector2(100),
                            // Font = OsuFont.GetFont(size: 20),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Short Dolly is here",
                            Font = OsuFont.GetFont(size: 45),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "欧内的手，好汉",
                            Font = OsuFont.GetFont(size: 50),
                        },
                    },
                },
            };
        }
    }
}
