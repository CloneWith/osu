// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Components.Shapes;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// A rounded box with <see cref="CustomRoundedBoxBase"/> as its background.
    /// </summary>
    /// <remarks>
    /// Due to the height of base not considered, the layout with other components would be really chaotic.
    /// This is going to be fixed in the future.
    /// </remarks>
    public partial class CustomRoundedBox : Container
    {
        private readonly CustomRoundedBoxBase background = new CustomRoundedBoxBase();

        public Color4 BackgroundColour
        {
            get => background.BackgroundColour;
            set
            {
                if (background.BackgroundColour != value)
                    background.TransformTo(nameof(background.BackgroundColour), value, 300, Easing.OutQuint);
            }
        }

        public CustomRoundedBox()
        {
            AutoSizeAxes = Axes.Both;
            AutoSizeEasing = Easing.OutQuint;
            AutoSizeDuration = 200;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Set a margin for children to maintain a basically sane layout.
            Child.Margin = new MarginPadding { Horizontal = 10, Vertical = 2 };

            background.Anchor = Anchor.CentreLeft;
            background.Origin = Anchor.CentreLeft;
            background.Colour = BackgroundColour;
            background.Depth = float.MaxValue;

            AddInternal(background);
        }
    }
}
