// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
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

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly List<FumoChessPiece> boardMapList = new List<FumoChessPiece>();
        private List<DrawableBoardBlock> blocks = new List<DrawableBoardBlock>();

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private TeamColour pickTeam;
        private RoundStep pickType;

        private Container mainContainer = null!;
        private Container informationContainer = null!;
        private Container chatContainer = null!;
        private Container boardContainer = null!;
        private ChessMapPool mapPool = null!;
        private InstructionDisplay instructionDisplay = null!;
        private FillFlowContainer<DrawableBoardBlock> boardBlockArea = null!;

        private OsuButton buttonRedBan = null!;
        private OsuButton buttonBlueBan = null!;
        private OsuButton buttonRedPick = null!;
        private OsuButton buttonBluePick = null!;

        private OsuButton buttonRedWin = null!;
        private OsuButton buttonBlueWin = null!;
        private OsuButton buttonRedShiro = null!;
        private OsuButton buttonBlueShiro = null!;

        private OsuButton buttonIndicator = null!;

        private DialogOverlay dialogOverlay = null!;

        private ScheduledDelegate? scheduledScreenChange;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(LadderInfo.CurrentMatch);

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
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedShiro = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Shiro",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, RoundStep.Shiro),
                                    },
                                    buttonBlueShiro = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Shiro",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, RoundStep.Shiro),
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
                            BackgroundColour = Color4.DeepPink,
                            Action = () =>
                            {
                                dialogOverlay.Push(new ResetBoardDialog(reset));
                            },
                        },
                    },
                },
                dialogOverlay = new DialogOverlay(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LadderInfo.MainBoardSize.BindValueChanged(e =>
                boardBlockArea.ResizeTo(new Vector2(e.NewValue), 300, Easing.OutQuint));
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            ResetSelectStatus();
            // TODO: Add more relevant actions.
        }

        private void setMode(TeamColour colour, RoundStep stepType)
        {
            pickTeam = colour;
            pickType = stepType;

            instructionDisplay.Team = colour;
            instructionDisplay.Step = stepType;

            buttonRedBan.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Ban);
            buttonBlueBan.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Ban);
            buttonRedPick.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Pick);
            buttonBluePick.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Pick);
            buttonRedWin.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Win);
            buttonBlueWin.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Win);
            buttonRedShiro.Colour = setColour(pickTeam == TeamColour.Red && pickType == RoundStep.Shiro);
            buttonBlueShiro.Colour = setColour(pickTeam == TeamColour.Blue && pickType == RoundStep.Shiro);
            return;

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var map = boardMapList.FirstOrDefault(m => m.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));
            var block = blocks.FirstOrDefault(b => b.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));

            if (map == null)
            {
                showFail(block);
                return base.OnMouseDown(e);
            }

            switch (e.Button)
            {
                case MouseButton.Left when map.BeatmapID > 0:
                {
                    // Handle updating status to Red/Blue Win
                    if (pickType == RoundStep.Win)
                    {
                        addWinPlacement(map.BeatmapID, block);
                    }
                    else
                    {
                        addPlacement(map.BeatmapID, block);
                    }

                    break;
                }

                case MouseButton.Right:
                {
                    var placement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p => p.BeatmapID == map.BeatmapID);

                    if (placement == null)
                        return true;

                    {
                        CurrentMatch.Value?.ChessPlacements.Remove(placement);

                        var chessPiece = boardMapList.LastOrDefault(c => c.BeatmapID == map.BeatmapID);

                        if (chessPiece == null)
                            return true;

                        placement = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p => p.BeatmapID == map.BeatmapID);

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
                    break;
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
            CurrentMatch.Value?.Round.Value?.IsFinalStage.BindTo(new BindableBool());

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

        private void addWinPlacement(int beatmapId, DrawableBoardBlock? block)
        {
            var existing = CurrentMatch.Value?.ChessPlacements.LastOrDefault(p =>
                p.BeatmapID == beatmapId && p.CurrentType is ChoiceType.RedWin or ChoiceType.BlueWin);

            // Updating winning status without existing placement entries is not allowed now.
            if (existing == null)
            {
                showFail(block);
                dialogOverlay.Push(new ActionNotPermittedDialog(BoardStrings.PickBansUnavailable));
                return;
            }

            CurrentMatch.Value?.ChessPlacements.Add(existing.CreateUpdate(pickTeam,
                pickTeam == TeamColour.Red ? ChoiceType.RedWin : ChoiceType.BlueWin));
        }

        private void addPlacement(int beatmapId, DrawableBoardBlock? block)
        {
            bool isCommonType = pickType is RoundStep.Pick or RoundStep.Ban or RoundStep.Win or RoundStep.Shiro;

            if (pickType == RoundStep.Default || pickTeam == TeamColour.None || CurrentMatch.Value?.Round.Value == null)
            {
                showFail(block);
                return;
            }

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap?.OnlineID != beatmapId))
                // don't attempt to add if the beatmap isn't in our pool
                return;

            if (pickType != RoundStep.Win
                && CurrentMatch.Value.ChessPlacements.Any(p => p.BeatmapID == beatmapId
                                                               && p.CurrentType is ChoiceType.Ban or ChoiceType.RedWin or ChoiceType.BlueWin))
                // don't attempt to add if already banned / won, and it's not a win type.
                return;

            if (pickType == RoundStep.Pick)
            {
                var introMap = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.Beatmap?.OnlineID == beatmapId);

                if (introMap != null)
                    sceneManager?.ShowMapIntro(introMap, pickTeam);
            }

            if (isCommonType
                && block != null
                && !CurrentMatch.Value.ChessPlacements.Any(p => p.BeatmapID == beatmapId && isSameStep(p.CurrentType, pickType)))
            {
                CurrentMatch.Value.ChessPlacements.Add(new ChessPlacement(block.BoardRow, block.BoardColumn,
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
        }

        private bool isSameStep(ChoiceType choiceType, RoundStep step)
            => step switch
            {
                RoundStep.Pick => choiceType == ChoiceType.Pick,
                RoundStep.Ban => choiceType == ChoiceType.Ban,
                RoundStep.Win => choiceType is ChoiceType.RedWin or ChoiceType.BlueWin,
                _ => false,
            };

        public override void Hide()
        {
            scheduledScreenChange?.Cancel();
            base.Hide();
        }
    }
}
