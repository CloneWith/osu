// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class EXBoardBeatmapPanel : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        private readonly string index;
        private readonly string mod;

        public const float WIDTH = 650;
        public const float HEIGHT = 75;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private Box flash = null!;
        private Box backgroundAddition = null!;

        public EXBoardBeatmapPanel(IBeatmapInfo? beatmap, string mod = "", string index = "")
        {
            Beatmap = beatmap;
            this.index = index;
            this.mod = mod;

            Width = WIDTH;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            var displayTitle = Beatmap?.GetDisplayTitleRomanisable(false, false)
                                      .ToString().ExtractSongTitleFromMetadata().Trim().TruncateWithEllipsis(39) ?? (LocalisableString)@"unknown";

            string displayDifficulty = Beatmap?.DifficultyName.TruncateWithEllipsis(25) ?? "unknown";

            Masking = true;
            CornerRadius = 10;

            InternalChildren = new Drawable[]
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
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Padding = new MarginPadding(15),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = displayTitle,
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 32),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new osuTK.Vector2(5),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Text = displayDifficulty,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 23)
                                },
                                new StarRatingDisplay(starDifficulty: new StarDifficulty(Beatmap?.StarRating ?? 0, 0), animated: true),
                            }
                        }
                    },
                },
                new TournamentModIcon(index.IsNull() ? mod : mod + index)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 20, Top = 90 },
                    Size = new osuTK.Vector2(96),
                    RelativeSizeAxes = Axes.Y,
                },
                flash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            };
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
                match.OldValue.EXPicks.CollectionChanged -= picksBansOnCollectionChanged;
            if (match.NewValue != null)
                match.NewValue.EXPicks.CollectionChanged += picksBansOnCollectionChanged;

            Scheduler.AddOnce(updateState);
        }

        private void picksBansOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(updateState);

        private BeatmapChoice? choice;

        private void updateState()
        {
            if (currentMatch.Value == null)
            {
                return;
            }

            var newChoice = currentMatch.Value.EXPicks.FirstOrDefault(p => p.BeatmapID == Beatmap?.OnlineID);

            bool shouldFlash = newChoice != choice;

            if (newChoice != null)
            {
                if (shouldFlash)
                    flash.FadeOutFromOne(duration: 900, easing: Easing.OutSine).Loop(0, 3);

                BorderThickness = 6;

                BorderColour = TournamentGame.GetTeamColour(newChoice.Team);

                switch (newChoice.Type)
                {
                    case ChoiceType.Pick:
                        backgroundAddition.Colour = Color4.White;
                        backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                        break;

                    case ChoiceType.RedWin:
                        backgroundAddition.Colour = new OsuColour().Red;
                        backgroundAddition.FadeTo(newAlpha: 0.35f, duration: 100, easing: Easing.InCubic);
                        break;

                    case ChoiceType.BlueWin:
                        backgroundAddition.Colour = new OsuColour().Sky;
                        backgroundAddition.FadeTo(newAlpha: 0.4f, duration: 100, easing: Easing.InCubic);
                        break;

                    default:
                        backgroundAddition.Colour = Color4.White;
                        backgroundAddition.FadeTo(newAlpha: 0, duration: 150, easing: Easing.InCubic);
                        break;
                }
            }
            else
            {
                flash.ClearTransforms();
                backgroundAddition.ClearTransforms();
                backgroundAddition.FadeOut(duration: 100, easing: Easing.OutCubic);
                backgroundAddition.Colour = Color4.White;
                BorderThickness = 0;
                Alpha = 1;
                flash.Alpha = 0;
            }

            choice = newChoice;
        }
    }
}
