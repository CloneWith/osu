// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.OpenGL;

namespace osu.Game.Tournament.Components
{

    public partial class SimpleBoardBeatmapPanel : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        public readonly string Index;

        public readonly string Mod;

        public const float HEIGHT = 150;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private Box backgroundAddition = null!;
        private Box flash = null!;

        private SpriteIcon icon = null!;
        private SpriteIcon swapIcon = null!;
        private SpriteIcon protectIcon = null!;
        private SpriteIcon trapIcon = null!;

        // Real X and Y positions on the board, distinct from RoundBeatmap.BoardX and BoardY.
        public int RealX;
        public int RealY;

        public SimpleBoardBeatmapPanel(IBeatmapInfo? beatmap, string mod = "", string index = "", int initX = 1, int initY = 1, float scale = 1.0f)
        {
            Beatmap = beatmap;
            Index = index;
            Mod = mod;
            RealX = initX;
            RealY = initY;

            Width = HEIGHT * scale;
            Height = HEIGHT * scale;
        }


        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindTo(ladder.CurrentMatch);

            Masking = true;
            Alpha = 0;

            AddRangeInternal(new Drawable[]
            {
                new NoUnloadBeatmapSetCover
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.5f),
                    OnlineInfo = (Beatmap as IBeatmapSetOnlineInfo),
                },
                backgroundAddition = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.1f
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Size = new osuTK.Vector2(0.4f),
                    Alpha = 0,
                },
                swapIcon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Yellow,
                    Size = new osuTK.Vector2(0.4f),
                    Icon = FontAwesome.Solid.ExchangeAlt,
                    Alpha = 0,
                },
                protectIcon = new SpriteIcon
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Icon = FontAwesome.Solid.Lock,
                    Colour = Color4.White,
                    Size = new osuTK.Vector2(24),
                    Position = new osuTK.Vector2(5, -5),
                    Alpha = 0,
                },
                trapIcon = new SpriteIcon
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Icon = FontAwesome.Solid.ExclamationCircle,
                    Colour = Color4.White,
                    Size = new osuTK.Vector2(24),
                    Position = new osuTK.Vector2(-5, -5),
                    Alpha = 0,
                },
                flash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            });

            if (!string.IsNullOrEmpty(Mod))
            {
                AddInternal(new TournamentModIcon(Index.IsNull() ? Mod : Mod + Index)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 60,
                    Size = new osuTK.Vector2(80),
                    RelativeSizeAxes = Axes.Y,
                });
            }
        }

        public void Flash(int count = 1)
        {
            if (count <= 0) return;
            if (count == 1) flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine);
            else flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine).Loop(0, count);
        }

        public void InitAnimate(int initDelay = 0)
        {
            Scale = new Vector2(0.7f);
            using (BeginDelayedSequence(initDelay))
            {
                this.FadeInFromZero(400, Easing.OutCubic);
                this.ScaleTo(1.0f, 800, Easing.OutQuint);
            }
        }

        public void AddStateAnimate(int initDelay = 0)
        {
            using (BeginDelayedSequence(initDelay))
            {
                var relatedBPChoices = currentMatch.Value.PicksBans.Where(p => p.BeatmapID == Beatmap.OnlineID);
                var relatedTrapChoices = currentMatch.Value.Traps.Where(p => p.BeatmapID == Beatmap.OnlineID);
                var relatedProtectChoices = currentMatch.Value.Protects.Where(p => p.BeatmapID == Beatmap.OnlineID);
                bool isBothTrapped = relatedTrapChoices.Any(p => p.Team == TeamColour.Red) && relatedTrapChoices.Any(p => p.Team == TeamColour.Blue);

                for (int i = 0; i < relatedProtectChoices.Count(); i++)
                {
                    var protect = relatedProtectChoices.ElementAt(i);
                    using (BeginDelayedSequence(i * 250))
                    {
                        backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                        protectIcon.FadeIn(duration: 150, easing: Easing.InCubic);
                        protectIcon.Colour = protect.Team == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky;
                    }
                }

                for (int i = 0; i < relatedTrapChoices.Count(); i++)
                {
                    var trap = relatedTrapChoices.ElementAt(i);
                    using (BeginDelayedSequence((i + relatedProtectChoices.Count()) * 250))
                    {
                        backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                        trapIcon.FadeIn(duration: 150, easing: Easing.InCubic);
                        trapIcon.Colour = isBothTrapped ? Color4.White : (trap.Team == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky);
                    }
                }

                for (int i = 0; i < relatedBPChoices.Count(); i++)
                {
                    var choice = relatedBPChoices.ElementAt(i);
                    using (BeginDelayedSequence((i + relatedProtectChoices.Count() + relatedTrapChoices.Count()) * 250))
                    {
                        Flash();

                        switch (choice.Type)
                        {
                            case ChoiceType.Pick:
                                Colour = Color4.White;
                                // Alpha = 1f;
                                backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                                icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                                BorderColour = TournamentGame.GetTeamColour(choice.Team);
                                BorderThickness = 4;
                                break;

                            // Ban: All darker
                            case ChoiceType.Ban:
                                backgroundAddition.Colour = Color4.Black;
                                backgroundAddition.FadeTo(newAlpha: 0.7f, duration: 150, easing: Easing.InCubic);
                                icon.Icon = FontAwesome.Solid.Ban;
                                icon.Colour = choice.Team == TeamColour.Red ? new OsuColour().TeamColourRed : new OsuColour().Sky;
                                icon.FadeTo(newAlpha: 0.6f, duration: 200, easing: Easing.InCubic);
                                BorderThickness = 0;
                                break;

                            // Win: Background colour
                            case ChoiceType.RedWin:
                                backgroundAddition.Colour = Color4.Red;
                                backgroundAddition.FadeTo(newAlpha: 0.4f, duration: 150, easing: Easing.InCubic);
                                icon.FadeIn(duration: 150, easing: Easing.InCubic);
                                icon.Icon = FontAwesome.Solid.Trophy;
                                icon.Colour = new OsuColour().Red;
                                BorderThickness = 4;
                                break;

                            case ChoiceType.BlueWin:
                                backgroundAddition.Colour = new OsuColour().Sky;
                                backgroundAddition.FadeTo(newAlpha: 0.5f, duration: 150, easing: Easing.InCubic);
                                icon.FadeIn(duration: 150, easing: Easing.InCubic);
                                icon.Icon = FontAwesome.Solid.Trophy;
                                icon.Colour = new OsuColour().Blue;
                                BorderThickness = 4;
                                break;

                            default:
                                icon.FadeOut(duration: 100, easing: Easing.OutCubic);
                                BorderThickness = 0;
                                break;
                        }
                    }
                }
            }
        }

        private partial class NoUnloadBeatmapSetCover : UpdateableOnlineBeatmapSetCover
        {
            // As covers are displayed on stream, we want them to load as soon as possible.
            protected override double LoadDelay => 0;

            // Use DelayedLoadWrapper to avoid content unloading when switching away to another screen.
            protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
                => new DelayedLoadWrapper(createContentFunc(), timeBeforeLoad);
        }
    }
}
