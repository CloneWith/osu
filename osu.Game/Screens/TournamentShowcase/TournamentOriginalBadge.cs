// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class TournamentOriginalBadge : CompositeDrawable
    {
        private const int width = 200;
        private const int height = 50;

        private readonly RotatingDisplayContainer flashTextContainer;
        private readonly GridContainer contentContainer;
        private readonly Sprite badgeIcon;
        private readonly FillFlowContainer textFlow;

        public TournamentOriginalBadge()
        {
            Width = width;
            Height = height;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = @"Background container",
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(1),
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.3f),
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = FumoColours.SeaBlue.Light.Opacity(0.75f),
                        },
                        new TrianglesV2
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScaleAdjust = 0.4f,
                            Alpha = 0.5f,
                        },
                    },
                },
                flashTextContainer = new RotatingDisplayContainer
                {
                    Name = @"Flash text container",
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Looped = false,
                    DisplayLength = 1000,
                    TransformLength = 800,
                    CrossAnimation = true,
                    InAnimation = (t, duration) =>
                    {
                        t.ScaleTo(1.5f);
                        t.FadeOut();

                        t.ScaleTo(1f, duration, Easing.OutQuint);
                        t.FadeIn(duration, Easing.OutQuint);
                    },
                    OutAnimation = (t, duration) =>
                    {
                        t.ScaleTo(0.3f, duration, Easing.InQuint);
                        t.Delay(duration * 0.1f).FadeOut(duration * 0.75f, Easing.OutQuint);
                    },
                },
                contentContainer = new GridContainer
                {
                    Name = @"Content container (long running)",
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Padding = new MarginPadding { Horizontal = 5, Bottom = 5 },
                    ColumnDimensions = [
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            badgeIcon = new Sprite
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Alpha = 0,
                            },
                            textFlow = new FillFlowContainer
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Alpha = 0,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Text = @"赛事定制",
                                        Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 20),
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Text = @"Tournament Original",
                                        Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 14),
                                    },
                                },
                            },
                        },
                    },
                },
            };

            // Add two line of texts to text container
            flashTextContainer.AddLayers(new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = @"赛事定制",
                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 24),
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = @"Tournament Original",
                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 20),
                },
            });
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(TextureStore? textures, ShowcaseConfig? config)
        {
            if (textures != null && config != null)
                badgeIcon.Texture = textures.Get(@$"{config.TournamentName}/original-badge");
        }

        public void Animate()
        {
            contentContainer.FadeOut();
            badgeIcon.FadeOut();
            textFlow.FadeOut();

            flashTextContainer.ResetCompleteTrigger();
            if (flashTextContainer.Playing)
                flashTextContainer.Pause();

            flashTextContainer.OnComplete += () =>
            {
                flashTextContainer.FadeOut(500, Easing.OutQuint);

                using (BeginDelayedSequence(250))
                {
                    contentContainer.FadeIn(500, Easing.OutQuint);
                    textFlow.MoveToOffset(new Vector2(50, 0));

                    using (BeginDelayedSequence(250))
                    {
                        badgeIcon.FadeIn(750, Easing.OutQuint);
                        textFlow.MoveToOffset(new Vector2(-50, 0), 750, Easing.OutQuint);
                        textFlow.FadeIn(1000, Easing.OutQuint);
                    }
                }
            };

            flashTextContainer.Start(true);
        }
    }
}
