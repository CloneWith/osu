// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Menu;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseLogo : BeatSyncedContainer
    {
        private readonly ShowcaseConfig config;
        private readonly Sprite logo;
        private readonly Container logoBeatContainer;
        private readonly Box flashLayer;
        private readonly Container colourAndTriangles;
        private readonly TrianglesV2 triangles;

        private const double early_activation = 60;
        private const float triangles_paused_velocity = 0.5f;

        public ShowcaseLogo(ShowcaseConfig config)
        {
            this.config = config;
            var colourProvider = new OverlayColourProvider(config.ColourScheme.Value);

            EarlyActivationMilliseconds = early_activation;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                logoBeatContainer = new Container
                {
                    Width = 512,
                    Height = 512,
                    Children = new Drawable[]
                    {
                        new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Scale = OsuLogo.SCALE_ADJUST,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                colourAndTriangles = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        triangles = new TrianglesV2
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Thickness = 0.009f,
                                            ScaleAdjust = 3,
                                            SpawnRatio = 1.4f,
                                            Colour = ColourInfo.GradientVertical(colourProvider.Colour1, colourProvider.Colour3),
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                    }
                                },
                                flashLayer = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Blending = BlendingParameters.Additive,
                                    Colour = Color4.White,
                                    Alpha = 0,
                                },
                            },
                        },
                        logo = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fill,
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture logoTexture = textures.Get($"{config.TournamentName}/logo") ?? textures.Get(@"Menu/logo");
            logo.Texture = logoTexture;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            double beatLength = timingPoint.BeatLength;

            float amplitudeAdjust = Math.Min(1, 0.4f + amplitudes.Maximum);

            if (beatIndex < 0) return;

            logoBeatContainer
                .ScaleTo(1 - 0.02f * amplitudeAdjust, early_activation, Easing.Out).Then()
                .ScaleTo(1, beatLength * 2, Easing.OutQuint);

            if (effectPoint.KiaiMode && flashLayer.Alpha < 0.4f)
            {
                flashLayer.ClearTransforms();
                flashLayer
                    .FadeTo(0.2f * amplitudeAdjust, early_activation, Easing.Out).Then()
                    .FadeOut(beatLength);
            }

            this.Delay(early_activation).Schedule(() =>
            {
                triangles.Velocity += amplitudeAdjust * (effectPoint.KiaiMode ? 6 : 3);
            });
        }

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        protected override void Update()
        {
            base.Update();

            if (musicController.CurrentTrack.IsRunning)
            {
                triangles.Velocity = (float)Interpolation.Damp(triangles.Velocity, triangles_paused_velocity * (IsKiaiTime ? 4 : 2), 0.995f, Time.Elapsed);
            }
            else
            {
                triangles.Velocity = (float)Interpolation.Damp(triangles.Velocity, triangles_paused_velocity, 0.9f, Time.Elapsed);
            }
        }
    }
}
