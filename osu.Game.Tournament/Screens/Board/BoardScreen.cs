// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Components.Animations;
using osu.Game.Tournament.Components.Dialogs;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.Tournament.Screens.TeamWin;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Board
{
    public partial class BoardScreen : TournamentMatchScreen
    {
        private readonly BindableBool preparationMode = new BindableBool(true);
        private readonly BindableBool shiroModeActivated = new BindableBool();
        private readonly BindableBool tiebreakerOverride = new BindableBool();

        private readonly BindableBool enableIntroAnimation = new BindableBool(true);

        private TeamColour pickTeam;
        private RoundStep pickType;

        private readonly BindableInt currentRoundIndex = new BindableInt(-3)
        {
            MinValue = -3,
            MaxValue = 17,
        };

        private Container mainContainer = null!;
        private Container informationContainer = null!;
        private Container chatContainer = null!;
        private FumoChessBoard chessBoard = null!;
        private ChessMapPool mapPool = null!;
        private InstructionDisplay instructionDisplay = null!;

        private OsuNumberBox roundNumberBox = null!;
        private GrayButton roundMinusButton = null!;
        private GrayButton roundPlusButton = null!;

        private OsuButton buttonRedBan = null!;
        private OsuButton buttonBlueBan = null!;
        private OsuButton buttonRedPick = null!;
        private OsuButton buttonBluePick = null!;
        private OsuButton buttonRedWin = null!;
        private OsuButton buttonBlueWin = null!;

        private ClickTwiceButton buttonEnterTiebreaker = null!;
        private ClickTwiceButton buttonClearSpecialState = null!;
        private OsuButton buttonIndicator = null!;
        private OsuButton buttonTiebreakerRedWin = null!;
        private OsuButton buttonTiebreakerBlueWin = null!;

        private TournamentSpriteText actionStateText = null!;

        private DialogOverlay dialogOverlay = null!;

        private Sample? updateOwnerSample;
        private Sample? unavailableSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            updateOwnerSample = audio.Samples.Get("Board/update");
            unavailableSample = audio.Samples.Get("unavailable");

            InternalChildren = new Drawable[]
            {
                new TourneyBackground(BackgroundType.Board)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new FumoMatchHeader(false),

                mainContainer = new Container
                {
                    Name = "Main container", // without header
                    Padding = new MarginPadding { Top = 100, Left = 30, Bottom = 10, Right = 30 },
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Left side",
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Width = 350,
                            RelativeSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                informationContainer = new Container
                                {
                                    Name = "Top-left information area",
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,
                                    Height = 0.7f,
                                    Padding = new MarginPadding { Bottom = 5f },
                                    Children = new Drawable[]
                                    {
                                        new EmptyBox(10)
                                        {
                                            Colour = Color4Extensions.FromHex("#454545"),
                                            Alpha = 0.74f,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        new GridContainer
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            RowDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize),
                                                new Dimension(),
                                                new Dimension(GridSizeMode.AutoSize),
                                            },
                                            Content = new Drawable[][]
                                            {
                                                [
                                                    new MatchRoundDisplay
                                                    {
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        // Weird margin layout...
                                                        Margin = new MarginPadding { Vertical = 10 },
                                                        Scale = new Vector2(0.45f),
                                                    },
                                                ],
                                                [
                                                    new HistoryDisplay
                                                    {
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        RelativeSizeAxes = Axes.Both,
                                                        Padding = new MarginPadding(5),
                                                    },
                                                ],
                                                [
                                                    new RoundCounterLine
                                                    {
                                                        Anchor = Anchor.BottomCentre,
                                                        Origin = Anchor.BottomCentre,
                                                        RelativeSizeAxes = Axes.X,
                                                        Margin = new MarginPadding { Vertical = 5 },
                                                    },
                                                ],
                                            },
                                        },
                                    },
                                },
                                chatContainer = new Container
                                {
                                    Name = "Chat area",
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,
                                    Height = 0.3f,
                                    Padding = new MarginPadding { Top = 5f },
                                    Child = new EmptyBox(10)
                                    {
                                        Colour = Color4Extensions.FromHex("#454545"),
                                        Alpha = 0.74f,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                },
                            },
                        },
                        new Container
                        {
                            Name = "Centre",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Y,
                            Width = FumoChessBoard.BOARD_SIZE,
                            Children = new Drawable[]
                            {
                                chessBoard = new FumoChessBoard
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativePositionAxes = Axes.Both,
                                },
                                instructionDisplay = new InstructionDisplay
                                {
                                    Name = @"Instruction area",
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    RelativeSizeAxes = Axes.X,
                                    RelativePositionAxes = Axes.Both,
                                    Width = 1,
                                    Height = 80,
                                    InnerPadding = new MarginPadding { Horizontal = 20 },
                                },
                            },
                        },
                        mapPool = new ChessMapPool
                        {
                            Name = @"Chess piece pool",
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            RelativePositionAxes = Axes.Both,
                            Width = 350,
                        },
                    },
                },
                new ControlPanel(true)
                {
                    Children = new Drawable[]
                    {
                        new SectionHeader(BoardStrings.RoundCounter),
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            ColumnDimensions =
                            [
                                new Dimension(GridSizeMode.Absolute, 40),
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 40),
                            ],
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    roundMinusButton = new GrayButton(FontAwesome.Solid.Minus, HoverSampleSet.ButtonSidebar)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Action = () => currentRoundIndex.Value--,
                                        Padding = new MarginPadding { Right = 5 },
                                    },
                                    roundNumberBox = new OsuNumberBox(allowNegative: true)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                    },
                                    roundPlusButton = new GrayButton(FontAwesome.Solid.Plus, HoverSampleSet.ButtonSidebar)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Action = () => currentRoundIndex.Value++,
                                        Padding = new MarginPadding { Left = 5 },
                                    },
                                }
                            },
                        },
                        new LabelledSwitchButton
                        {
                            Label = BoardStrings.PreparationMode,
                            Current = preparationMode,
                        },
                        new SectionHeader(BoardStrings.CurrentMode),
                        buttonIndicator = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BoardStrings.TiebreakerIndicator,
                            BackgroundColour = FumoColours.DeepPurple.Regular,
                            Action = () => setMode(TeamColour.Neutral, RoundStep.Default),
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedBan = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Ban",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, RoundStep.Ban),
                                    },
                                    buttonBlueBan = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Ban",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, RoundStep.Ban),
                                    },
                                },
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedPick = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Pick",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, RoundStep.Pick),
                                    },
                                    buttonBluePick = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Pick",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, RoundStep.Pick),
                                    },
                                },
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedWin = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Win",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, RoundStep.Win),
                                    },
                                    buttonBlueWin = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Win",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, RoundStep.Win),
                                    },
                                },
                            },
                        },
                        new FormSliderBar<int>
                        {
                            Current = LadderInfo.MainBoardSize,
                            Caption = BoardStrings.MainBoardAreaSize,
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BaseStrings.Reset,
                            BackgroundColour = FumoColours.FlandreRed.Regular,
                            Action = () =>
                            {
                                dialogOverlay.Push(new ResetBoardDialog(reset));
                            },
                        },
                        new SectionHeader(BoardStrings.ShiroDeployment),
                        actionStateText = new TournamentSpriteText
                        {
                            Name = @"Action information display",
                            RelativeSizeAxes = Axes.X,
                            AllowMultiline = true,
                            Text = BoardStrings.ActionPlaceholder,
                            Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                            Padding = new MarginPadding { Horizontal = 5 },
                        },
                        new LabelledSwitchButton
                        {
                            Label = BoardStrings.EnableDeployment,
                            Current = shiroModeActivated,
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BoardStrings.PlaceShiro,
                            BackgroundColour = FumoColours.DeepPurple.Regular,
                            Action = () =>
                            {
                                if (CurrentMatch.Value?.ChessPlacements.Any(p => p.BeatmapID == TournamentGame.RESERVED_BEATMAP_ID) != false)
                                {
                                    updateActionText(BoardStrings.ShiroExistsPrompt, true);
                                    return;
                                }

                                setMode(TeamColour.None, RoundStep.Shiro);
                            },
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BoardStrings.ActivateShiro,
                            BackgroundColour = FumoColours.SeaBlue.Regular,
                            Action = activateShiro,
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BoardStrings.UpdateShiroOwner,
                            BackgroundColour = FumoColours.SunshineYellow.Darker,
                            Action = updateWin,
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = BoardStrings.ClearSelection,
                            BackgroundColour = FumoColours.FlandreRed.Regular,
                            Action = clearShiroSelection,
                        },
                        new SectionHeader(BoardStrings.TiebreakerControl),
                        new LabelledSwitchButton
                        {
                            Label = BoardStrings.OverrideTiebreakerControl,
                            Current = tiebreakerOverride,
                        },
                        buttonEnterTiebreaker = new ClickTwiceButton(sampleSet: null)
                        {
                            AutoSizeAxes = Axes.None,
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            IdleIcon = FontAwesome.Solid.ArrowRight,
                            Text = BoardStrings.EnterTiebreaker,
                            Action = () => setMode(TeamColour.Neutral, RoundStep.TieBreaker),
                            Enabled = { Value = pickType is not (RoundStep.TieBreaker or RoundStep.FinalWin) },
                        },
                        buttonClearSpecialState = new ClickTwiceButton(sampleSet: null)
                        {
                            AutoSizeAxes = Axes.None,
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            IdleIcon = FontAwesome.Regular.TrashAlt,
                            Text = BoardStrings.ClearSpecialState,
                            Action = clearWin,
                            Enabled = { Value = pickType is RoundStep.TieBreaker or RoundStep.FinalWin },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonTiebreakerRedWin = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Win",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setWin(TeamColour.Red),
                                        Enabled = { Value = false },
                                    },
                                    buttonTiebreakerBlueWin = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Win",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setWin(TeamColour.Blue),
                                        Enabled = { Value = false },
                                    },
                                },
                            },
                        },
                        new SectionHeader(SetupStrings.AutomationHeader),
                        new LabelledSwitchButton
                        {
                            Label = BoardStrings.AutoAdvanceRounds,
                            Current = LadderInfo.AutoProgressRound,
                        },
                        new LabelledSwitchButton
                        {
                            Label = BoardStrings.AutoAdvanceScreens,
                            Current = LadderInfo.AutoProgressScreens,
                        },
                        new SectionHeader(BaseStrings.DebugSettings),
                        new LabelledSwitchButton
                        {
                            Label = BoardStrings.EnableIntroAnimation,
                            Current = enableIntroAnimation,
                        },
                    },
                },
                dialogOverlay = new DialogOverlay(),
            };

            animationQueue.BindCollectionChanged((_, arg) =>
            {
                if (animationQueue.Count == 0)
                    return;

                if (currentAnimation != null && currentAnimation.Status != AnimationStatus.Complete)
                    return;

                var animation = animationQueue.First();
                startAnimation(animation);
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentMatch.BindValueChanged(matchChanged, true);

            shiroModeActivated.BindValueChanged(e =>
            {
                if (e.NewValue)
                    setMode(TeamColour.Neutral, RoundStep.Shiro);
                else
                    pickType = RoundStep.Default;
            });

            tiebreakerOverride.BindValueChanged(e =>
            {
                buttonTiebreakerRedWin.Enabled.Value = buttonTiebreakerBlueWin.Enabled.Value = e.NewValue;
                buttonTiebreakerRedWin.Colour = buttonTiebreakerBlueWin.Colour = setColour(e.NewValue);
            });

            currentRoundIndex.BindValueChanged(e =>
            {
                roundMinusButton.Enabled.Value = e.NewValue > currentRoundIndex.MinValue;
                roundPlusButton.Enabled.Value = e.NewValue < currentRoundIndex.MaxValue;
                roundNumberBox.Text = e.NewValue.ToString();

                if (e.NewValue == 17)
                    setMode(TeamColour.Neutral, RoundStep.TieBreaker);
            }, true);

            roundNumberBox.OnCommit += (_, newText) =>
            {
                if (!newText)
                    return;

                if (int.TryParse(roundNumberBox.Text, out int newIndex))
                {
                    currentRoundIndex.Value = newIndex;
                }
                else
                {
                    roundNumberBox.Text = currentRoundIndex.Value.ToString();
                }
            };
        }

        private (string? mod, string? index) getBeatmapMod(int beatmapId)
        {
            if (CurrentMatch.Value?.Round.Value?.Beatmaps == null)
                return (null, null);

            var fetched = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.ID == beatmapId);
            return (fetched?.Mods, fetched?.ModIndex);
        }

        private static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
            {
                currentRoundIndex.UnbindFrom(match.OldValue.CurrentRoundIndex);
                preparationMode.UnbindFrom(match.OldValue.PreparationMode);
            }

            if (match.NewValue != null)
            {
                currentRoundIndex.BindTo(match.NewValue.CurrentRoundIndex);
                currentRoundIndex.MinValue = match.NewValue.CurrentRoundIndex.MinValue;
                currentRoundIndex.MaxValue = match.NewValue.CurrentRoundIndex.MaxValue;
                preparationMode.BindTo(match.NewValue.PreparationMode);
            }

            ResetSelectStatus();
            detectWin();
        }

        private void updateActionText(LocalisableString text, bool failing = false)
        {
            actionStateText.Text = text;
            actionStateText.Colour = failing ? FumoColours.SunshineYellow.Regular : FumoColours.SeaBlue.Regular;
            actionStateText.FlashColour(Color4.White, 900, Easing.OutQuint);
        }

        private void setMode(TeamColour colour, RoundStep stepType)
        {
            pickTeam = colour;
            pickType = stepType;

            if (instructionDisplay.Team != colour || instructionDisplay.Step != stepType)
            {
                instructionDisplay.Team = colour;
                instructionDisplay.Step = stepType;
            }

            if (CurrentMatch.Value != null)
                CurrentMatch.Value.IsFinalStage.Value = pickType is RoundStep.TieBreaker;

            if (stepType != RoundStep.Shiro)
                shiroModeActivated.Value = false;

            if (pickType is RoundStep.TieBreaker)
            {
                var tieBreakerMap = CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(b => b.Mods.Equals(@"TB", StringComparison.OrdinalIgnoreCase));

                if (tieBreakerMap != null && enableIntroAnimation.Value)
                    ShowMapIntro(tieBreakerMap);
            }

            buttonEnterTiebreaker.Enabled.Value = pickType is not (RoundStep.TieBreaker or RoundStep.FinalWin);
            buttonClearSpecialState.Enabled.Value = pickType is RoundStep.TieBreaker or RoundStep.FinalWin;
            buttonTiebreakerRedWin.Enabled.Value = tiebreakerOverride.Value || pickType is RoundStep.TieBreaker or RoundStep.FinalWin;
            buttonTiebreakerBlueWin.Enabled.Value = tiebreakerOverride.Value || pickType is RoundStep.TieBreaker or RoundStep.FinalWin;

            buttonRedBan.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Ban);
            buttonBlueBan.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Ban);
            buttonRedPick.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Pick);
            buttonBluePick.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Pick);
            buttonRedWin.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Win);
            buttonBlueWin.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Win);

            if (pickType != RoundStep.TieBreaker)
            {
                buttonTiebreakerRedWin.Colour = setColour(colour == TeamColour.Red && pickType == RoundStep.FinalWin);
                buttonTiebreakerBlueWin.Colour = setColour(colour == TeamColour.Blue && pickType == RoundStep.FinalWin);
            }
        }

        private bool checkSelected(IEnumerable<FumoChessPiece> source)
        {
            if (source.GroupBy(b => b.OwnerTeam).Count() != 1)
            {
                updateActionText(BoardStrings.SingleColourPrompt, true);
                return false;
            }

            return true;
        }

        private void consumeSelected()
        {
            foreach (var b in chessBoard.SelectedBlocks)
            {
                b.ChessLayer.Child.CurrentType = ChoiceType.Consumed;
                var record = CurrentMatch.Value?.ChessPlacements.Last(p => positionEquals(p, b));

                if (record != null)
                    CurrentMatch.Value?.ChessPlacements.Add(record.CreateUpdate(null, ChoiceType.Consumed));
            }

            clearShiroSelection();
        }

        private void activateShiro()
        {
            var shiro = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p => p.BeatmapID == TournamentGame.RESERVED_BEATMAP_ID);

            // Don't activate if a Shiro is not found or already in a Win state.
            if (shiro == null)
            {
                updateActionText(BoardStrings.ShiroMissingPrompt, true);
                return;
            }

            if (shiro.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin)
            {
                updateActionText(BoardStrings.ShiroActivatedPrompt, true);
            }

            var chessPieces = chessBoard.SelectedBlocks.Select(b => b.ChessLayer.Child);

            if (chessPieces.Count() != 2)
            {
                updateActionText(BoardStrings.ShiroActivationPrompt, true);
                return;
            }

            if (!checkSelected(chessPieces))
                return;

            setMode(chessPieces.Select(b => b.OwnerTeam).First(), RoundStep.Shiro);
            addWinPlacement(TournamentGame.RESERVED_BEATMAP_ID, true);
            consumeSelected();

            // Only this function escapes the normal interaction route, needed to detect winner separately.
            detectWin();
            shiroModeActivated.Value = false;
        }

        private void updateWin()
        {
            if (CurrentMatch.Value == null || !CurrentMatch.Value.ChessPlacements.Any())
                return;

            var chessPieces = chessBoard.SelectedBlocks.Select(b => b.ChessLayer.Child);
            var placements = chessPieces.Select(p => p.BeatmapID)
                                        .Select(id => CurrentMatch.Value.ChessPlacements.LastOrDefault(p => p.BeatmapID == id))
                                        .OfType<ChessPlacement>();

            if (!checkSelected(chessPieces))
                return;

            (int red, int blue) couplets = TournamentMatch.GetMaximumSuccessiveChess(placements);

            // Have checked in checkSelected, guaranteed to have exactly one group
            TeamColour targetTeam = placements.GroupBy(p => p.OwnerTeam).Single().Key;
            int coupletCount = targetTeam == TeamColour.Red ? couplets.red : couplets.blue;
            (int remainRed, int remainBlue) = TournamentMatch.GetMaximumSuccessiveChess(placements.Skip(2));
            int remainCount = targetTeam == TeamColour.Red ? remainRed : remainBlue;

            if (coupletCount == 3 || (coupletCount == 2 && remainCount == 2))
            {
                pickTeam = targetTeam;
                pickType = RoundStep.UpdateOwner;

                instructionDisplay.Team = targetTeam;
                instructionDisplay.Step = RoundStep.UpdateOwner;
            }
            else
            {
                updateActionText(BoardStrings.ShiroOwnerUpdatePrompt, true);
            }
        }

        private void detectWin()
        {
            if (CurrentMatch.Value == null)
                return;

            (int red, int blue) couplets = CurrentMatch.Value.GetMaximumSuccessiveChess();

            if (couplets.red == 4 ^ couplets.blue == 4)
            {
                // Winner detected: Set winner and completion
                setWin(couplets.red == 4 ? TeamColour.Red : TeamColour.Blue);
            }
            else
            {
                if ((couplets.red == 4 && couplets.blue == 4))
                {
                    // TieBreaker detected
                    setMode(TeamColour.Neutral, RoundStep.TieBreaker);
                }

                // TieBreaker detection might not be so accurate.
                // The final decision should be made by referees and streamers.
                if (isTieBreaker())
                {
                    buttonIndicator.BackgroundColour = FumoColours.SunshineYellow.Lighter;
                    buttonIndicator.FlashColour(Color4.White, 1000, Easing.OutQuint);
                }
                else
                {
                    buttonIndicator.BackgroundColour = FumoColours.DeepPurple.Regular;
                }

                // No condition met: Reset status and clear scores
                CurrentMatch.Value.Completed.Value = false;
                CurrentMatch.Value.Team1Score.Value = couplets.red;
                CurrentMatch.Value.Team2Score.Value = couplets.blue;
            }
        }

        private void clearWin()
        {
            if (CurrentMatch.Value == null)
                return;

            detectWin();

            // If we still got the winner, it is valid and don't need further handling.
            // Otherwise, restore the original state.
            if (!CurrentMatch.Value.Completed.Value)
                setMode(CurrentMatch.Value.CurrentTeam, CurrentMatch.Value.CurrentRoundIndex.Value <= 0 ? RoundStep.Ban : RoundStep.Pick);

            SceneManager?.CancelScreenChange();
        }

        private bool isTieBreaker()
        {
            if (CurrentMatch.Value == null)
                return false;

            var occupiedBlocks = chessBoard.Blocks.Where(b => b.ChessLayer.Count == 1).ToList();

            // 1. Row / Column / Diagonal line chess check
            // Win chess pieces must have the same colour and not consumed
            for (int i = 1; i <= 4; i++)
            {
                // ReSharper disable AccessToModifiedClosure
                var rowWinPieces = takeWinPieces(occupiedBlocks.Where(b => b.BoardRow == i));
                var colWinPieces = takeWinPieces(occupiedBlocks.Where(b => b.BoardColumn == i));
                // ReSharper restore AccessToModifiedClosure

                if (isSameWinTeam(rowWinPieces) || isSameWinTeam(colWinPieces))
                    return false;
            }

            var mainDiagonalWinPieces = takeWinPieces(occupiedBlocks.Where(b => b.BoardRow == b.BoardColumn));
            var subDiagonalWinPieces = takeWinPieces(occupiedBlocks.Where(b => b.BoardRow + b.BoardColumn == 5));

            if (isSameWinTeam(mainDiagonalWinPieces) || isSameWinTeam(subDiagonalWinPieces))
                return false;

            // 2. Available space check per specific area
            bool stepsAvailable = false;

            foreach (string k in TournamentGame.MODS.Select(m => m.Key))
            {
                bool condition(DrawableBoardBlock b) => k switch
                {
                    @"HR" => b.BoardRow > 2 && b.BoardColumn > 2,
                    @"HD" => b.BoardRow <= 2 && b.BoardColumn > 2,
                    @"DT" => (b.BoardRow <= 2 && b.BoardColumn <= 2) || (b.BoardRow > 2 && b.BoardColumn > 2),
                    _ => true,
                };

                stepsAvailable |= chessBoard.Blocks.Any(b => condition(b) && b.ChessLayer.Count == 0);
            }

            return !stepsAvailable;

            List<FumoChessPiece> takeWinPieces(IEnumerable<DrawableBoardBlock> blk) =>
                blk.Select(b => b.ChessLayer.Child)
                   .Where(c => c.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin or ChoiceType.Consumed)
                   .ToList();

            bool isSameWinTeam(List<FumoChessPiece> cs) => cs.Count == 0
                                                           || !cs.Any(c => c.CurrentType is ChoiceType.Consumed)
                                                           && cs.GroupBy(c => c.CurrentType).Count() == 1;
        }

        private void setWin(TeamColour colour)
        {
            if (CurrentMatch.Value == null || colour is not (TeamColour.Blue or TeamColour.Red))
                return;

            int targetScore = CurrentMatch.Value.PointsToWin;

            setMode(colour, RoundStep.FinalWin);

            CurrentMatch.Value.Team1Score.Value = colour == TeamColour.Red ? targetScore : 0;
            CurrentMatch.Value.Team2Score.Value = colour == TeamColour.Blue ? targetScore : 0;

            if (LadderInfo.AutoProgressScreens.Value)
            {
                SceneManager?.ScheduleScreenChange(typeof(TeamWinScreen), 15000);
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var block = chessBoard.Blocks.FirstOrDefault(b => b.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));
            var lastSelected = mapPool.MapPanels.FirstOrDefault(p => p.Selected);

            bool succeeded = false;

            // 1. Map pool interaction
            if (mapPool.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
            {
                var target = mapPool.MapPanels.FirstOrDefault(p => p.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));

                if (target == null)
                    return true;

                var existingPlacement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(c => c.BeatmapID == target.Beatmap.ID);
                var chessBlock = chessBoard.Blocks.FirstOrDefault(b => positionEquals(existingPlacement, b));

                switch (e.Button)
                {
                    case MouseButton.Left:
                        switch (pickType)
                        {
                            case RoundStep.Ban:
                                succeeded |= addPlacement(target.Beatmap.ID, chessBlock);
                                break;

                            case RoundStep.Win:
                                succeeded |= addWinPlacement(target.Beatmap.ID);
                                break;

                            default:
                                // Unselect itself
                                if (lastSelected == target)
                                {
                                    target.Selected = false;
                                }
                                else
                                {
                                    if (lastSelected != null)
                                        lastSelected.Selected = false;
                                    target.Selected = true;
                                }

                                break;
                        }

                        break;

                    case MouseButton.Right:
                        removeLatestPlacement(target.Beatmap.ID);
                        break;
                }
            }
            else if (chessBoard.CanBlockAreaReceivesInput(e.ScreenSpaceMousePosition))
            {
                // 2. Chess board interaction or no special handling needed
                var placement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p => positionEquals(p, block));
                var target = chessBoard.ChessPieces.FirstOrDefault(c => c.BeatmapID == placement?.BeatmapID);

                switch (e.Button)
                {
                    case MouseButton.Left:
                    {
                        switch (pickType)
                        {
                            case RoundStep.UpdateOwner:
                                if (target != null)
                                {
                                    var chessPieces = chessBoard.SelectedBlocks.Select(b => b.ChessLayer.Child);

                                    if (chessPieces.Contains(target))
                                        break;

                                    succeeded |= addWinPlacement(target.BeatmapID, true, true);

                                    if (succeeded)
                                    {
                                        consumeSelected();
                                        shiroModeActivated.Value = false;
                                    }
                                }

                                break;

                            case RoundStep.Win:
                                if (target != null)
                                {
                                    // Disallow multiple win states
                                    if (CurrentMatch.Value?.ChessPlacements.Any(p => p.BeatmapID == target.BeatmapID
                                                                                     && p.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin)
                                        != false)
                                        break;

                                    succeeded |= addWinPlacement(target.BeatmapID);
                                }

                                break;

                            case RoundStep.Pick:
                                if (lastSelected == null)
                                {
                                    showFail(block);
                                    break;
                                }

                                succeeded |= addPlacement(lastSelected.Beatmap.ID, block);
                                break;

                            case RoundStep.Shiro:
                                if (block != null)
                                {
                                    switch (pickTeam)
                                    {
                                        case TeamColour.None:
                                            pickTeam = currentRoundIndex.Value % 2 == 1 ? TeamColour.Red : TeamColour.Blue;
                                            succeeded |= addPlacement(TournamentGame.RESERVED_BEATMAP_ID, block);

                                            // Reset active mode only when auto progressing is not enabled
                                            if (succeeded && !LadderInfo.AutoProgressRound.Value)
                                            {
                                                pickType = RoundStep.Default;
                                                instructionDisplay.Step = RoundStep.Default;
                                            }

                                            break;

                                        default:
                                            if (!shiroModeActivated.Value)
                                                break;

                                            var matches = CurrentMatch.Value?.ChessPlacements.Where(p => positionEquals(p, block));

                                            if (matches?.Any(p => p.CurrentType is ChoiceType.Consumed) == true
                                                || matches?.Any(p => p.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin) != true)
                                                break;

                                            succeeded = true;
                                            bool exists = chessBoard.SelectedBlocks.Contains(block);

                                            block.FadeBackgroundColour(!exists ? FumoColours.SeaBlue.Regular : null);

                                            if (exists)
                                            {
                                                chessBoard.SelectedBlocks.Remove(block);
                                            }
                                            else
                                            {
                                                chessBoard.SelectedBlocks.Add(block);
                                            }

                                            break;
                                    }
                                }

                                break;
                        }

                        break;
                    }

                    case MouseButton.Right:
                    {
                        if (target != null)
                            succeeded |= removeLatestPlacement(target.BeatmapID);
                        break;
                    }
                }
            }
            else
            {
                // Immediately return without playing samples when nothing notable is clicked
                return true;
            }

            switch (succeeded)
            {
                case true:
                    detectWin();
                    if (lastSelected != null)
                        lastSelected.Selected = false;
                    break;

                case false:
                    showFail(block);
                    unavailableSample?.Play();
                    break;
            }

            return true;
        }

        private bool positionEquals(ChessPlacement? placement, DrawableBoardBlock? block)
            => placement != null && block != null && placement.BoardRow == block.BoardRow && placement.BoardColumn == block.BoardColumn;

        private void clearShiroSelection()
        {
            // Get back to normal route to avoid accidentally adding win states.
            shiroModeActivated.Value = false;

            chessBoard.SelectedBlocks.ForEach(b => b.FadeBackgroundColour());
            chessBoard.SelectedBlocks.Clear();
        }

        private bool removeLatestPlacement(int beatmapId)
        {
            var placement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p => p.BeatmapID == beatmapId);
            var beatmap = CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(b => b.ID == beatmapId);
            var history = CurrentMatch.Value?.ChessHistory.LastOrDefault(h => (h.Mod, h.ModIndex) == (beatmap?.Mods, beatmap?.ModIndex));

            if (CurrentMatch.Value == null || placement == null)
                return false;

            if (placement.CurrentType is ChoiceType.Consumed)
            {
                if (beatmap != null)
                {
                    var lastConsumed = CurrentMatch.Value.ChessHistory.LastOrDefault(h =>
                        h.UsedPieces.Contains((beatmap.Mods, beatmap.ModIndex)));

                    if (lastConsumed != null)
                    {
                        if (lastConsumed.UsedPieces.Length == 1 || placement.CurrentType is not ChoiceType.Consumed)
                            CurrentMatch.Value.ChessHistory.Remove(lastConsumed);
                        else
                        {
                            var otherUsedPieces = lastConsumed.UsedPieces.Where(p => p != (beatmap.Mods, beatmap.ModIndex))
                                                              .ToArray();
                            int index = CurrentMatch.Value.ChessHistory.Count - 1;

                            while (index >= 0 && CurrentMatch.Value.ChessHistory[index] == lastConsumed)
                                index--;

                            CurrentMatch.Value.ChessHistory[index] = lastConsumed with
                            {
                                UsedPieces = otherUsedPieces
                            };
                        }
                    }
                }
            }
            else
            {
                if (history != null)
                    CurrentMatch.Value.ChessHistory.Remove(history);
            }

            CurrentMatch.Value.ChessPlacements.Remove(placement);

            // Decrement round when the revoked placement is a Shiro, or in a Win status.
            if (placement.BeatmapID == TournamentGame.RESERVED_BEATMAP_ID
                || placement.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin)
                setNextMode(undo: true);

            var chessPiece = chessBoard.ChessPieces.LastOrDefault(c => c.BeatmapID == beatmapId);

            if (chessPiece != null)
            {
                placement = CurrentMatch.Value.ChessPlacements.LastOrDefault(p => p.BeatmapID == beatmapId);

                if (placement != null)
                {
                    chessPiece.OwnerTeam = placement.OwnerTeam;
                    chessPiece.CurrentType = placement.CurrentType;
                }
                else
                {
                    chessPiece.Remove();
                }
            }

            return true;
        }

        protected override void OnFirstSelected()
        {
            base.OnFirstSelected();

            // Padding cannot be changed partially, moving the container instead.
            mainContainer.MoveToY(10);
            informationContainer.MoveToY(1.5f);
            chatContainer.MoveToY(1.75f);
            chessBoard.MoveToY(1.5f);
            instructionDisplay.MoveToY(1.75f);
            mapPool.MoveToY(1.5f);

            // All containers start moving into the screen in order.
            using (BeginDelayedSequence(1000))
            {
                chessBoard.MoveToY(0, 900, Easing.OutQuint);

                using (BeginDelayedSequence(300))
                {
                    informationContainer.MoveToY(0, 900, Easing.OutQuint);
                    chatContainer.Delay(100).MoveToY(0, 900, Easing.OutQuint);
                    mapPool.MoveToY(0, 900, Easing.OutQuint);
                    instructionDisplay.MoveToY(0, 900, Easing.OutQuint);
                }
            }

            using (BeginDelayedSequence(500))
            {
                mainContainer.MoveToY(0, 1000, Easing.OutQuint);
            }
        }

        private void reset()
        {
            // Clear map marking lists
            CurrentMatch.Value?.PicksBans.Clear();
            CurrentMatch.Value?.ChessPlacements.Clear();
            CurrentMatch.Value?.ChessHistory.Clear();

            chessBoard.Reset();

            if (CurrentMatch.Value != null)
            {
                CurrentMatch.Value.PreparationMode.Value = true;
                CurrentMatch.Value.CurrentRoundIndex.Value = -1;

                CurrentMatch.Value.IsFinalStage.Value = false;
                CurrentMatch.Value.Completed.Value = false;
                CurrentMatch.Value.Team1Score.Value = 0;
                CurrentMatch.Value.Team2Score.Value = 0;
            }

            // Reset button group
            buttonBlueBan.Colour = Color4.White;
            buttonBluePick.Colour = Color4.White;
            buttonBlueWin.Colour = Color4.White;
            buttonRedBan.Colour = Color4.White;
            buttonRedPick.Colour = Color4.White;
            buttonRedWin.Colour = Color4.White;
            buttonIndicator.Colour = Color4.Gray;

            pickTeam = TeamColour.None;
            pickType = RoundStep.Default;
        }

        private void setNextMode(bool undo = false)
        {
            if (CurrentMatch.Value == null || !LadderInfo.AutoProgressRound.Value)
                return;

            if (undo)
                CurrentMatch.Value.CurrentRoundIndex.Value--;
            else
                CurrentMatch.Value.CurrentRoundIndex.Value++;

            setMode(CurrentMatch.Value.CurrentTeam, CurrentMatch.Value.CurrentRoundIndex.Value <= 0 ? RoundStep.Ban : RoundStep.Pick);
        }

        private void showFail(DrawableBoardBlock? flashBlock)
        {
            flashBlock?.FlashIcon(FontAwesome.Solid.Times);
            flashBlock?.FlashColour(FumoColours.FlandreRed.Regular);
        }

        private bool addWinPlacement(int beatmapId, bool keepCurrentRound = false, bool updating = false)
        {
            var existing = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p =>
                p.BeatmapID == beatmapId && p.CurrentType is not ChoiceType.Neutral);
            var chess = chessBoard.ChessPieces.LastOrDefault(c => c.BeatmapID == beatmapId);

            // Updating winning status without existing placement entries is not allowed now.
            if (existing == null)
            {
                updateActionText(BoardStrings.PicksUnavailable, true);
                return false;
            }

            // Don't update if same as the last type
            if ((existing.CurrentType is ChoiceType.RedWin && pickTeam == TeamColour.Red)
                || (existing.CurrentType is ChoiceType.BlueWin && pickTeam == TeamColour.Blue))
            {
                return false;
            }

            if (existing.CurrentType is ChoiceType.Ban or ChoiceType.Consumed)
                return false;

            var winRecord = existing.CreateUpdate(pickTeam,
                pickTeam == TeamColour.Red ? ChoiceType.RedWin : ChoiceType.BlueWin);

            CurrentMatch.Value?.ChessPlacements.Add(winRecord);

            var converted = HistoryExtensions.Convert([winRecord], CurrentMatch.Value?.Round.Value?.Beatmaps.ToList());
            List<(string?, string?)> usedModDataList = chessBoard.SelectedBlocks.Select(b => CurrentMatch.Value?.ChessPlacements.Last(p => positionEquals(p, b))).OfType<ChessPlacement>().Select(record => getBeatmapMod(record.BeatmapID)).ToList();

            if (converted.Count == 1)
            {
                CurrentMatch.Value?.ChessHistory.Add(converted[0] with
                {
                    Type = updating ? HistoryType.OwnerUpdate : HistoryType.Normal,
                    UsedPieces = updating
                        ? usedModDataList.OfType<(string, string)>().ToArray()
                        : [],
                });
            }

            if (chess != null)
            {
                chess.OwnerTeam = pickTeam;
                chess.CurrentType = TournamentGame.ToChoiceType(pickType, pickTeam);
            }

            updateOwnerSample?.Play();

            if (!keepCurrentRound)
                setNextMode();

            return true;
        }

        private bool addPlacement(int beatmapId, DrawableBoardBlock? block)
        {
            if (pickType == RoundStep.Shiro)
            {
                if (block == null)
                    return false;

                CurrentMatch.Value?.ChessPlacements.Add(new ChessPlacement(block.BoardRow, block.BoardColumn,
                    pickTeam, ChoiceType.Pick));

                CurrentMatch.Value?.ChessHistory.Add(new History(HistoryType.ShiroPlacement, pickTeam, ChoiceType.Pick,
                    null, null, block.BoardRow, block.BoardColumn, []));

                chessBoard.AddSingleChess(beatmapId, block.BoardRow, block.BoardColumn, pickTeam, ChoiceType.Pick);
                setNextMode();
                return true;
            }

            bool isCommonType = pickType is RoundStep.Pick or RoundStep.Ban or RoundStep.Win;

            if (pickType == RoundStep.Default || pickTeam == TeamColour.None || CurrentMatch.Value?.Round.Value == null)
                return false;

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap?.OnlineID != beatmapId) && pickType != RoundStep.Shiro)
                // don't attempt to add if the beatmap isn't in our pool
                return false;

            if (pickType != RoundStep.Win
                && CurrentMatch.Value.ChessPlacements.Any(p => p.BeatmapID == beatmapId
                                                               && p.CurrentType is ChoiceType.Ban or ChoiceType.RedWin or ChoiceType.BlueWin))
                // don't attempt to add if already banned / won, and it's not a win type.
                return false;

            if (pickType == RoundStep.Pick)
            {
                if (block == null)
                    return false;

                // Pick records on existing ones are not allowed
                if (CurrentMatch.Value.ChessPlacements.Any(p => p.BeatmapID == beatmapId
                                                                || (p.BoardRow == block.BoardRow && p.BoardColumn == block.BoardColumn)))
                    return false;

                var introMap = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.Beatmap?.OnlineID == beatmapId);

                if (introMap == null || !isValidArea(introMap.Mods, block.BoardRow, block.BoardColumn))
                    return false;

                if (enableIntroAnimation.Value)
                    ShowMapIntro(introMap, pickTeam);
            }

            if (isCommonType
                && !CurrentMatch.Value.ChessPlacements.Any(p => p.BeatmapID == beatmapId && isSameStep(p.CurrentType, pickType)))
            {
                if (block != null)
                {
                    chessBoard.AddSingleChess(beatmapId, block.BoardRow, block.BoardColumn);
                }

                CurrentMatch.Value.ChessPlacements.Add(new ChessPlacement(block?.BoardRow, block?.BoardColumn,
                    pickTeam, TournamentGame.ToChoiceType(pickType, pickTeam), beatmapId));

                var fetched = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.ID == beatmapId);

                CurrentMatch.Value.ChessHistory.Add(new History(HistoryType.Normal, pickTeam, TournamentGame.ToChoiceType(pickType, pickTeam),
                    fetched?.Mods, fetched?.ModIndex, block?.BoardRow ?? -1, block?.BoardColumn ?? -1, []));
            }

            if (pickType is RoundStep.Ban)
                setNextMode();

            if (LadderInfo.AutoProgressScreens.Value)
            {
                if (pickType == RoundStep.Pick && CurrentMatch.Value.ChessPlacements.Any(i => i.CurrentType == ChoiceType.Pick))
                {
                    SceneManager?.ScheduleScreenChange(typeof(GameplayScreen), 20000);
                }
            }

            return true;
        }

        private bool isValidArea(string acronym, int row, int column) => acronym.ToUpperInvariant() switch
        {
            @"HR" => row > 2 && column <= 2,
            @"HD" => row <= 2 && column > 2,
            @"DT" => (row <= 2 && column <= 2) || (row > 2 && column > 2),
            _ => true,
        };

        private bool isSameStep(ChoiceType choiceType, RoundStep step)
            => step switch
            {
                RoundStep.Pick => choiceType == ChoiceType.Pick,
                RoundStep.Ban => choiceType == ChoiceType.Ban,
                RoundStep.Win => choiceType is ChoiceType.RedWin or ChoiceType.BlueWin,
                _ => false,
            };

        public override void Show()
        {
            SceneManager?.ProxyChatToContainer(chatContainer);
            base.Show();
        }

        #region Animation

        private readonly BindableList<IAnimation> animationQueue = new BindableList<IAnimation>();

        private IAnimation? currentAnimation;

        public void ShowMapIntro(RoundBeatmap map, TeamColour colour = TeamColour.Neutral) => queueAnimation(new BeatmapIntroAnimation(map, colour)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });

        public void ShowWinAnimation(TournamentTeam? team, TeamColour colour = TeamColour.Neutral) => queueAnimation(new RoundAnimation(team, colour)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });

        private void startAnimation(IAnimation animation)
        {
            AddInternal((Drawable)(currentAnimation = animation));

            animation.Fire();
            animation.OnAnimationComplete += () =>
            {
                animationQueue.Remove(animation);
            };
        }

        private void queueAnimation(IAnimation d)
        {
            animationQueue.Add(d);
        }

        #endregion
    }
}
