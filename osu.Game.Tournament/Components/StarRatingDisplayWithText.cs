// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A star rating display with additional text for comment purposes.
    /// </summary>
    /// <remarks>
    /// Currently the colour of the text wouldn't change with the star icon and rating text, might be implemented on demand.
    /// </remarks>
    public partial class StarRatingDisplayWithText : StarRatingDisplay
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider? colourProvider { get; set; }

        private readonly OsuSpriteText textSprite;

        public StarRatingDisplayWithText(string additionalText, StarDifficulty starDifficulty, StarRatingDisplaySize size = StarRatingDisplaySize.Regular, bool animated = false)
            : base(starDifficulty, size, animated)
        {
            TextContainer.Add(textSprite = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Margin = new MarginPadding { Bottom = 1.5f },
                Text = additionalText,
                // todo: this should be size: 12f, but to match up with the design, it needs to be 14.4f
                // see https://github.com/ppy/osu-framework/issues/3271.
                Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                Shadow = false,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            textSprite.Colour = Current.Value.Stars >= 6.5 ? colours.Orange1 : colourProvider?.Background5 ?? Color4.Black.Opacity(0.75f);
        }
    }
}
