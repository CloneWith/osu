// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Threading;
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
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Board
{
    public partial class BoardScreen : TournamentMatchScreen
    {
        private const float board_size = 570;

        private readonly List<FumoChessPiece> boardMapList = new List<FumoChessPiece>();
        private List<DrawableBoardBlock> blocks = new List<DrawableBoardBlock>();
        private readonly List<DrawableBoardBlock> selectedBlocks = new List<DrawableBoardBlock>();

        private readonly BindableBool shiroModeActivated = new BindableBool();

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private TeamColour pickTeam;
        private RoundStep pickType;

        private BindableInt currentRoundIndex = new BindableInt(1)
        {
            MinValue = 1,
            MaxValue = 32,
        };

        private Container mainContainer = null!;
        private Container informationContainer = null!;
        private Container chatContainer = null!;
        private Container boardContainer = null!;
        private ChessMapPool mapPool = null!;
        private InstructionDisplay instructionDisplay = null!;
        private FillFlowContainer<DrawableBoardBlock> boardBlockArea = null!;

        private OsuNumberBox roundNumberBox = null!;
        private GrayButton roundMinusButton = null!;
        private GrayButton roundPlusButton = null!;

        private OsuButton buttonRedBan = null!;
        private OsuButton buttonBlueBan = null!;
        private OsuButton buttonRedPick = null!;
        private OsuButton buttonBluePick = null!;
        private OsuButton buttonRedWin = null!;
        private OsuButton buttonBlueWin = null!;

        private OsuButton buttonIndicator = null!;

        private TournamentSpriteText actionStateText = null!;

        private DialogOverlay dialogOverlay = null!;

        private ScheduledDelegate? scheduledScreenChange;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            var boardTexture = textures.Get("Board/board");

            InternalChildren = new Drawable[]
            {
                new TourneyBackground(BackgroundType.Board)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new FumoMatchHeader(),

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
                                    Child = new EmptyBox(10)
                                    {
                                        Colour = Color4Extensions.FromHex("#454545"),
                                        Alpha = 0.74f,
                                        RelativeSizeAxes = Axes.Both,
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
                            Width = board_size,
                            Children = new Drawable[]
                            {
                                boardContainer = new Container
                                {
                                    Name = "Board container",
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    RelativePositionAxes = Axes.Both,
                                    Height = board_size,
                                    Children = new Drawable[]
                                    {
                                        boardTexture != null
                                            ? new Sprite
                                            {
                                                Name = @"Board texture",
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                FillMode = FillMode.Fit,
                                                Texture = textures.Get(@"Board/board"),
                                            }
                                            : new EmptyBox(10)
                                            {
                                                Colour = Color4Extensions.FromHex("#454545"),
                                                Alpha = 0.74f,
                                                RelativeSizeAxes = Axes.Both,
                                            },
                                        boardBlockArea = new FillFlowContainer<DrawableBoardBlock>
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Direction = FillDirection.Full,
                                            Width = LadderInfo.MainBoardSize.Value,
                                            Height = LadderInfo.MainBoardSize.Value,
                                            ChildrenEnumerable = blocks =
                                                (from row in Enumerable.Range(1, 4)
                                                 from column in Enumerable.Range(1, 4)
                                                 select new DrawableBoardBlock(row, column)
                                                 {
                                                     Anchor = Anchor.Centre,
                                                     Origin = Anchor.Centre,
                                                     RelativeSizeAxes = Axes.Both,
                                                     Width = 0.25f,
                                                     Height = 0.25f,
                                                 })
                                                .ToList(),
                                        },
                                    },
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
                                    roundMinusButton = new GrayButton(FontAwesome.Solid.Minus)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Action = () => currentRoundIndex.Value--,
                                        Padding = new MarginPadding { Right = 5 },
                                    },
                                    roundNumberBox = new OsuNumberBox
                                    {
                                        RelativeSizeAxes = Axes.X,
                                    },
                                    roundPlusButton = new GrayButton(FontAwesome.Solid.Plus)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Action = () => currentRoundIndex.Value++,
                                        Padding = new MarginPadding { Left = 5 },
                                    },
                                }
                            },
                        },
                        new SectionHeader(BoardStrings.CurrentMode),
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
                        new ControlPanel.Spacer(),
                        buttonIndicator = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "TB Indicator",
                            BackgroundColour = Color4.Purple,
                            Colour = Color4.Gray,
                            Action = () => setMode(TeamColour.Neutral, RoundStep.Default),
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
                    },
                },
                dialogOverlay = new DialogOverlay(),
            };

            animationQueue.BindCollectionChanged((_, arg) =>
            {
                if (!animationQueue.Any())
                    return;

                if (currentAnimation == null || currentAnimation.Status == AnimationStatus.Complete)
                {
                    var animation = animationQueue.First();
                    startAnimation(animation);
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            initializeBoard();

            currentRoundIndex.BindTo(CurrentMatch.Value?.CurrentRoundIndex);
            CurrentMatch.BindValueChanged(matchChanged);

            LadderInfo.MainBoardSize.BindValueChanged(e =>
                boardBlockArea.ResizeTo(new Vector2(e.NewValue), 300, Easing.OutQuint));

            shiroModeActivated.BindValueChanged(e =>
            {
                // Only handle enabled cases to prevent infinite loops
                if (e.NewValue)
                {
                    setMode(TeamColour.Neutral, RoundStep.Shiro);
                }
                else
                {
                    pickType = RoundStep.Default;
                    clearShiroSelection();
                }
            });

            currentRoundIndex.BindValueChanged(e =>
            {
                roundMinusButton.Enabled.Value = e.NewValue > currentRoundIndex.MinValue;
                roundPlusButton.Enabled.Value = e.NewValue < currentRoundIndex.MaxValue;
                roundNumberBox.Text = e.NewValue.ToString();
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

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            currentRoundIndex.BindTo(match.NewValue?.CurrentRoundIndex);

            ResetSelectStatus();
            initializeBoard();
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

            if (stepType != RoundStep.Shiro)
                shiroModeActivated.Value = false;

            buttonRedBan.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Ban);
            buttonBlueBan.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Ban);
            buttonRedPick.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Pick);
            buttonBluePick.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Pick);
            buttonRedWin.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Win);
            buttonBlueWin.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Win);

            return;

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;
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
            foreach (var b in selectedBlocks)
            {
                b.ChessLayer.Child.CurrentType = ChoiceType.Consumed;
                var record = CurrentMatch.Value?.ChessPlacements.Last(p => positionEquals(p, b));

                if (record != null)
                    CurrentMatch.Value?.ChessPlacements.Add(record.CreateUpdate(null, ChoiceType.Consumed));
            }
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

            var chessPieces = selectedBlocks.Select(b => b.ChessLayer.Child);

            if (chessPieces.Count() != 2)
            {
                updateActionText(BoardStrings.ShiroActivationPrompt, true);
                return;
            }

            if (!checkSelected(chessPieces))
                return;

            setMode(chessPieces.Select(b => b.OwnerTeam).First(), RoundStep.Shiro);
            addWinPlacement(TournamentGame.RESERVED_BEATMAP_ID);
            consumeSelected();
            shiroModeActivated.Value = false;
        }

        private void updateWin()
        {
            if (CurrentMatch.Value == null || !CurrentMatch.Value.ChessPlacements.Any())
                return;

            var chessPieces = selectedBlocks.Select(b => b.ChessLayer.Child);
            var placements = chessPieces.Select(p => p.BeatmapID)
                                        .Select(id => CurrentMatch.Value.ChessPlacements.LastOrDefault(p => p.BeatmapID == id))
                                        .OfType<ChessPlacement>();

            if (!checkSelected(chessPieces))
                return;

            (int red, int blue) couplets = TournamentMatch.GetMaximumSuccessiveChess(placements);

            // Have checked in checkSelected, guaranteed to have exactly one group
            TeamColour targetTeam = placements.GroupBy(p => p.OwnerTeam).Single().Key;
            int coupletCount = targetTeam == TeamColour.Red ? couplets.red : couplets.blue;

            if (coupletCount == 3 || (coupletCount == 2 && placements.Count() - coupletCount == 2))
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

            if (couplets.red == 4 && couplets.blue == 4)
            {
                setMode(TeamColour.Neutral, RoundStep.TieBreaker);
            }
            else if (couplets.blue == 4 || couplets.red == 4)
            {
                setMode(couplets.red == 4 ? TeamColour.Red : TeamColour.Blue, RoundStep.FinalWin);
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var block = blocks.FirstOrDefault(b => b.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));
            var lastSelected = mapPool.MapPanels.FirstOrDefault(p => p.Selected);

            bool succeeded = false;

            // 1. Map pool interaction
            if (mapPool.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
            {
                var target = mapPool.MapPanels.FirstOrDefault(p => p.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));

                if (target == null)
                    return true;

                var existingPlacement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(c => c.BeatmapID == target.Beatmap.ID);
                var chessBlock = blocks.FirstOrDefault(b => positionEquals(existingPlacement, b));

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
            else if (boardBlockArea.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
            {
                // 2. Chess board interaction or no special handling needed
                var placement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p => positionEquals(p, block));
                var target = boardMapList.FirstOrDefault(c => c.BeatmapID == placement?.BeatmapID);

                switch (e.Button)
                {
                    case MouseButton.Left:
                    {
                        switch (pickType)
                        {
                            case RoundStep.UpdateOwner:
                                if (target != null)
                                {
                                    var chessPieces = selectedBlocks.Select(b => b.ChessLayer.Child);

                                    if (chessPieces.Contains(target))
                                        break;

                                    succeeded |= addWinPlacement(target.BeatmapID);

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
                                            succeeded |= addPlacement(TournamentGame.RESERVED_BEATMAP_ID, block);

                                            if (succeeded)
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
                                            bool exists = selectedBlocks.Contains(block);

                                            block.FadeBackgroundColour(!exists ? FumoColours.SeaBlue.Regular : null);

                                            if (exists)
                                            {
                                                selectedBlocks.Remove(block);
                                            }
                                            else
                                            {
                                                selectedBlocks.Add(block);
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

            switch (succeeded)
            {
                case true:
                    detectWin();
                    if (lastSelected != null)
                        lastSelected.Selected = false;
                    break;

                case false:
                    showFail(block);
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

            selectedBlocks.ForEach(b => b.FadeBackgroundColour());
            selectedBlocks.Clear();
        }

        private bool removeLatestPlacement(int beatmapId)
        {
            var placement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p => p.BeatmapID == beatmapId);

            if (placement == null)
                return false;

            CurrentMatch.Value?.ChessPlacements.Remove(placement);

            var chessPiece = boardMapList.LastOrDefault(c => c.BeatmapID == beatmapId);

            if (chessPiece == null)
                return true;

            placement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p => p.BeatmapID == beatmapId);

            if (placement != null)
            {
                chessPiece.OwnerTeam = placement.OwnerTeam;
                chessPiece.CurrentType = placement.CurrentType;
            }
            else
            {
                chessPiece.Remove();
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
            boardContainer.MoveToY(1.5f);
            instructionDisplay.MoveToY(1.75f);
            mapPool.MoveToY(1.5f);

            // All containers start moving into the screen in order.
            using (BeginDelayedSequence(1000))
            {
                boardContainer.MoveToY(0, 900, Easing.OutQuint);

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
            CurrentMatch.Value?.Round.Value?.IsFinalStage.BindTo(new BindableBool());

            boardMapList.Clear();
            boardBlockArea.Children.ForEach(b => b.ChessLayer.Clear());

            if (CurrentMatch.Value != null)
            {
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

        private void showFail(DrawableBoardBlock? flashBlock)
        {
            flashBlock?.FlashIcon(FontAwesome.Solid.Times);
            flashBlock?.FlashColour(FumoColours.FlandreRed.Regular);
        }

        private bool addWinPlacement(int beatmapId)
        {
            var existing = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p =>
                p.BeatmapID == beatmapId && p.CurrentType is not ChoiceType.Neutral);
            var chess = boardMapList.LastOrDefault(c => c.BeatmapID == beatmapId);

            // Updating winning status without existing placement entries is not allowed now.
            if (existing == null)
            {
                updateActionText(BoardStrings.PicksUnavailable, true);
                return false;
            }

            if (existing.CurrentType is ChoiceType.Ban or ChoiceType.Consumed)
                return false;

            CurrentMatch.Value?.ChessPlacements.Add(existing.CreateUpdate(pickTeam,
                pickTeam == TeamColour.Red ? ChoiceType.RedWin : ChoiceType.BlueWin));

            if (chess != null)
            {
                chess.OwnerTeam = pickTeam;
                chess.CurrentType = TournamentGame.ToChoiceType(pickType, pickTeam);
            }

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

                addSingleChess(beatmapId, block.BoardRow, block.BoardColumn, pickTeam, ChoiceType.Pick);

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
                // Multiple pick records are not allowed
                if (CurrentMatch.Value.ChessPlacements.Any(p => p.BeatmapID == beatmapId && p.CurrentType == ChoiceType.Pick))
                    return false;

                var introMap = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.Beatmap?.OnlineID == beatmapId);

                if (introMap != null)
                    ShowMapIntro(introMap, pickTeam);
            }

            if (isCommonType
                && !CurrentMatch.Value.ChessPlacements.Any(p => p.BeatmapID == beatmapId && isSameStep(p.CurrentType, pickType)))
            {
                if (block != null)
                {
                    addSingleChess(beatmapId, block.BoardRow, block.BoardColumn);
                }

                CurrentMatch.Value.ChessPlacements.Add(new ChessPlacement(block?.BoardRow, block?.BoardColumn,
                    pickTeam, TournamentGame.ToChoiceType(pickType, pickTeam), beatmapId));
            }

            // setNextMode(); // Uncomment if you still want to automatically set the next mode

            if (LadderInfo.AutoProgressScreens.Value)
            {
                if (pickType == RoundStep.Pick && CurrentMatch.Value.PicksBans.Any(i => i.Type == ChoiceType.Pick))
                {
                    scheduledScreenChange?.Cancel();
                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(GameplayScreen)); }, 10000);
                }
            }

            return true;
        }

        private void addSingleChess(int beatmapId, int row, int column,
                                    TeamColour ownerTeam = TeamColour.None, ChoiceType choiceType = ChoiceType.Neutral)
        {
            var block = blocks.FirstOrDefault(b => b.BoardRow == row && b.BoardColumn == column);

            // Add chess piece only when a block exists
            if (block == null)
                return;

            var newPiece = new FumoChessPiece(beatmapId)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Width = 1,
                Height = 1,
                Alpha = 0,
                OwnerTeam = ownerTeam,
                CurrentType = choiceType,
            };

            block.ChessLayer.Add(newPiece);
            boardMapList.Add(newPiece);

            newPiece.FadeIn(500, Easing.OutQuint);
            newPiece.ScaleTo(1.25f).Then().ScaleTo(1f, 900, Easing.OutQuint);
        }

        private void initializeBoard()
        {
            if (!IsLoaded)
                return;

            boardMapList.Clear();
            boardBlockArea.Children.ForEach(b => b.ChessLayer.Clear());

            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    var placement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p =>
                        p.BoardRow == i && p.BoardColumn == j);

                    if (placement != null)
                        addSingleChess(placement.BeatmapID, i, j, placement.OwnerTeam, placement.CurrentType);
                }
            }
        }

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
            sceneManager?.ProxyChatToContainer(chatContainer);
            base.Show();
        }

        public override void Hide()
        {
            sceneManager?.ReturnProxyChat();
            scheduledScreenChange?.Cancel();
            base.Hide();
        }

        #region Animation

        private readonly BindableList<IAnimation> animationQueue = new BindableList<IAnimation>();

        private IAnimation? currentAnimation;

        public void ShowMapIntro(RoundBeatmap map, TeamColour colour = TeamColour.Neutral) => queueAnimation(new TournamentIntro(map, colour)
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
