// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Ladder
{
    public partial class LadderScreen : TournamentScreen
    {
        protected Container<DrawableTournamentMatch> MatchesContainer = null!;
        private Container<Path> paths = null!;
        private Container headings = null!;

        protected LadderDragContainer ScrollContent = null!;

        protected Container Content = null!;

        private OsuCheckbox matchCompleteOverride = null!;
        private TournamentSpriteText bracketPosition = null!;
        private TournamentSpriteText bracketScale = null!;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        [BackgroundDependencyLoader]
        private void load()
        {
            normalPathColour = Color4Extensions.FromHex("#66D1FF");
            losersPathColour = Color4Extensions.FromHex("#FFC700");

            RelativeSizeAxes = Axes.Both;

            InternalChild = Content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    new TourneyVideo("ladder")
                    {
                        RelativeSizeAxes = Axes.Both,
                        Loop = true,
                    },
                    new DrawableTournamentHeaderText
                    {
                        Y = 100,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                    ScrollContent = new LadderDragContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            paths = new Container<Path> { RelativeSizeAxes = Axes.Both },
                            headings = new Container { RelativeSizeAxes = Axes.Both },
                            MatchesContainer = new Container<DrawableTournamentMatch>
                            {
                                AutoSizeAxes = Axes.Both
                            },
                        }
                    },
                }
            };

            AddInternal(new ControlPanel
            {
                Children = new Drawable[]
                {
                    matchCompleteOverride = new OsuCheckbox
                    {
                        LabelText = "match complete?",
                    },
                    bracketPosition = new TournamentSpriteText
                    {
                        Text = $"Position (X:Y) {ScrollContent.TargetPosition.X:000}:{ScrollContent.TargetPosition.Y:000}"
                    },
                    bracketScale = new TournamentSpriteText
                    {
                        Text = $"Scale {ScrollContent.TargetScale:000}"
                    },
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Reset position",
                        Action = () => updateTranslate(BracketViewTransformMode.Absolute, new Vector2(0, 0))
                    },
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Reset zoom",
                        Action = () => updateScale(BracketViewTransformMode.Absolute, 1.0f)
                    },
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Zoom in",
                        Action = () => updateScale(BracketViewTransformMode.Relative, 0.1f)
                    },
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Zoom out",
                        Action = () => updateScale(BracketViewTransformMode.Relative, -0.1f)
                    },
                    new TournamentSpriteText
                    {
                        Text = "----"
                    },
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "(lga) winners",
                        Action = () =>
                        {
                            updateScale(BracketViewTransformMode.Absolute, 0.6f);
                            Schedule(() => ScrollContent.SetPosition(new Vector2(154, 128), duration: 1500f, easing: Easing.InOutQuart));
                        }
                    },
                    new TourneyButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "(lga) losers",
                        Action = () =>
                        {
                            updateScale(BracketViewTransformMode.Absolute, 0.6f);
                            Schedule(() => ScrollContent.SetPosition(new Vector2(154, -727), duration: 1500f, easing: Easing.InOutQuart));
                        }
                    },
                }
            });

            void addMatch(TournamentMatch match) =>
                MatchesContainer.Add(new DrawableTournamentMatch(match, this is LadderEditorScreen)
                {
                    Changed = () => layout.Invalidate()
                });

            foreach (var match in LadderInfo.Matches)
                addMatch(match);

            LadderInfo.Rounds.CollectionChanged += (_, _) => layout.Invalidate();
            LadderInfo.Matches.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Debug.Assert(args.NewItems != null);

                        foreach (var p in args.NewItems.Cast<TournamentMatch>())
                            addMatch(p);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(args.OldItems != null);

                        foreach (var p in args.OldItems.Cast<TournamentMatch>())
                        {
                            foreach (var d in MatchesContainer.Where(d => d.Match == p))
                                d.Expire();
                        }

                        break;
                }

                layout.Invalidate();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentMatch.BindTo(ladder.CurrentMatch);
            currentMatch.BindValueChanged(vce =>
            {
                if (vce.OldValue != null)
                    matchCompleteOverride.Current.UnbindFrom(vce.OldValue.Completed);

                if (vce.NewValue != null)
                {
                    matchCompleteOverride.Current.BindTo(vce.NewValue.Completed);
                }
            }, true);

            ScrollContent.TargetChanged += () => bracketPosition.Text = $"Position (X:Y) {ScrollContent.TargetPosition.X:000}:{ScrollContent.TargetPosition.Y:000}";
            ScrollContent.ScaleChanged += () => bracketScale.Text = $"Scale {ScrollContent.TargetScale:.00}";
        }

        private void updateScale(BracketViewTransformMode transformMode, float scaleAdjustFactor)
        {
            switch (transformMode)
            {
                case BracketViewTransformMode.Absolute:
                    ScrollContent.SetScale(scaleAdjustFactor);
                    break;

                case BracketViewTransformMode.Relative:
                    ScrollContent.AdjustScale(scaleAdjustFactor);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(transformMode), transformMode, null);
            }
        }

        private void updateTranslate(BracketViewTransformMode transformMode, Vector2 transformVector)
        {
            switch (transformMode)
            {
                case BracketViewTransformMode.Absolute:
                    ScrollContent.SetPosition(transformVector);
                    break;

                case BracketViewTransformMode.Relative:
                    ScrollContent.AdjustPosition(transformVector);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(transformMode), transformMode, null);
            }
        }

        private readonly Cached layout = new Cached();

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
                UpdateLayout();
        }

        private Color4 normalPathColour;
        private Color4 losersPathColour;

        protected virtual bool DrawLoserPaths => false;

        protected virtual void UpdateLayout()
        {
            paths.Clear();
            headings.Clear();

            int id = 1;

            foreach (var match in MatchesContainer.OrderBy(d => d.Y).ThenBy(d => d.X))
            {
                match.Match.ID = id++;

                if (match.Match.Progression.Value != null)
                {
                    var dest = MatchesContainer.FirstOrDefault(p => p.Match == match.Match.Progression.Value);

                    if (dest == null)
                        // clean up outdated progressions.
                        match.Match.Progression.Value = null;
                    else
                        paths.Add(new ProgressionPath(match, dest) { Colour = match.Match.Losers.Value ? losersPathColour : normalPathColour });
                }

                if (DrawLoserPaths)
                {
                    if (match.Match.LosersProgression.Value != null)
                    {
                        var dest = MatchesContainer.FirstOrDefault(p => p.Match == match.Match.LosersProgression.Value);

                        if (dest == null)
                            // clean up outdated progressions.
                            match.Match.LosersProgression.Value = null;
                        else
                            paths.Add(new ProgressionPath(match, dest) { Colour = losersPathColour.Opacity(0.1f) });
                    }
                }
            }

            foreach (var round in LadderInfo.Rounds)
            {
                var topMatch = MatchesContainer.Where(p => !p.Match.Losers.Value && p.Match.Round.Value == round).MinBy(p => p.Y);

                if (topMatch == null) continue;

                headings.Add(new DrawableTournamentRound(round)
                {
                    Position = headings.ToLocalSpace((topMatch.ScreenSpaceDrawQuad.TopLeft + topMatch.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            foreach (var round in LadderInfo.Rounds)
            {
                var topMatch = MatchesContainer.Where(p => p.Match.Losers.Value && p.Match.Round.Value == round).MinBy(p => p.Y);

                if (topMatch == null) continue;

                headings.Add(new DrawableTournamentRound(round, true)
                {
                    Position = headings.ToLocalSpace((topMatch.ScreenSpaceDrawQuad.TopLeft + topMatch.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            layout.Validate();
        }
    }

    internal enum BracketViewTransformMode
    {
        Absolute,
        Relative
    }
}
