// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Game.Graphics;

namespace osu.Game.Users
{
    public partial class UserListPanel : ExtendedUserPanel
    {
        public ColourInfo BackgroundColour = ColourInfo.GradientHorizontal(Color4.White.Opacity(1), Color4.White.Opacity(0.3f));

        private readonly ListDisplayMode displayMode;
        private readonly int absoluteHeight;

        public UserListPanel(APIUser user, int height = 40, int cornerRadius = 6, ListDisplayMode mode = ListDisplayMode.Status)
            : base(user)
        {
            RelativeSizeAxes = Axes.X;
            CornerRadius = cornerRadius;
            absoluteHeight = height;

            displayMode = mode;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = RelativeSizeAxes is Axes.Y or Axes.Both ? 1f : absoluteHeight;
            Background.Width = 0.5f;
            Background.Origin = Anchor.CentreRight;
            Background.Anchor = Anchor.CentreRight;
            Background.Colour = BackgroundColour;
        }

        protected OsuSpriteText CreateUserRank()
        {
            // Assuming statistics is a property of APIUser and contains the necessary rank information
            var globalRank = User.Statistics?.GlobalRank?.ToLocalisableString("\\##,##0") ?? "-";

            return new OsuSpriteText
            {
                Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                Shadow = false,
                Text = globalRank
            };
        }

        protected OsuSpriteText CreateUserPP()
        {
            string performance = User.Statistics?.PP?.ToString("N0") ?? "-";

            return new OsuSpriteText
            {
                Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                Shadow = false,
                Text = performance + "pp"
            };
        }

        protected override Drawable CreateLayout()
        {
            FillFlowContainer details;

            var layout = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    details = new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            CreateAvatar().With(avatar =>
                            {
                                avatar.Anchor = Anchor.CentreLeft;
                                avatar.Origin = Anchor.CentreLeft;
                                avatar.Size = new Vector2(absoluteHeight);
                            }),
                            CreateFlag().With(flag =>
                            {
                                flag.Anchor = Anchor.CentreLeft;
                                flag.Origin = Anchor.CentreLeft;
                            }),
                            CreateUsername().With(username =>
                            {
                                username.Anchor = Anchor.CentreLeft;
                                username.Origin = Anchor.CentreLeft;
                                username.UseFullGlyphHeight = false;
                            })
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Margin = new MarginPadding { Right = 10 },
                        Children = new Drawable[]
                        {
                            CreateUserPP().With(pp =>
                            {
                                pp.Anchor = Anchor.CentreRight;
                                pp.Origin = Anchor.CentreRight;
                                pp.Alpha = displayMode != ListDisplayMode.Status ? 1f : 0f;
                            }),
                            CreateUserRank().With(rank =>
                            {
                                rank.Anchor = Anchor.CentreRight;
                                rank.Origin = Anchor.CentreRight;
                                rank.Alpha = displayMode != ListDisplayMode.Status ? 1f : 0f;
                            }),
                            // Disable these two function will cause a strange exception, using Alpha = 0f; instead
                            CreateStatusMessage(true).With(message =>
                            {
                                message.Anchor = Anchor.CentreRight;
                                message.Origin = Anchor.CentreRight;
                                message.Alpha = displayMode != ListDisplayMode.Statistics ? 1f : 0f;
                            }),
                            CreateStatusIcon().With(icon =>
                            {
                                icon.Anchor = Anchor.CentreRight;
                                icon.Origin = Anchor.CentreRight;
                                icon.Alpha = displayMode != ListDisplayMode.Statistics ? 1f : 0f;
                            })
                        }
                    }
                }
            };

            if (User.Groups != null)
            {
                details.Add(new GroupBadgeFlow
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    User = { Value = User }
                });
            }

            if (User.IsSupporter)
            {
                details.Add(new SupporterIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Height = 16,
                    SupportLevel = User.SupportLevel
                });
            }

            return layout;
        }
    }

    public enum ListDisplayMode
    {
        Status,
        Statistics,
        Both
    }
}
