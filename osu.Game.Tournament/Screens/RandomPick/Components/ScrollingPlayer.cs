// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.RandomPick.Components
{
    /// <summary>
    /// One player in the scrolling strip, shown as their avatar.
    /// </summary>
    public partial class ScrollingPlayer : Container
    {
        public readonly TournamentUser Player;

        public const float WIDTH = 64;
        public const float HEIGHT = 64;

        private readonly Box outline;

        public bool Selected
        {
            get;
            set
            {
                field = value;
                outline.FadeTo(value ? 0.6f : 0f, 100);
            }
        }

        public ScrollingPlayer(TournamentUser player)
        {
            Player = player;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            Size = new Vector2(WIDTH, HEIGHT);
            Masking = true;
            CornerRadius = 8f;

            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(4),
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        Child = new TournamentAvatar(player)
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
                outline = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0,
                },
            };
        }
    }
}
