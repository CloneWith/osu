// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class FumoChessBoard : CompositeDrawable
    {
        /// <summary>
        /// The regular size of the board.
        /// </summary>
        public const float BOARD_SIZE = 570;

        public bool Interactive { get; set; } = true;

        /// <summary>
        /// All <see cref="DrawableBoardBlock"/> on the board.
        /// </summary>
        public List<DrawableBoardBlock> Blocks { get; private set; } = new List<DrawableBoardBlock>();

        /// <summary>
        /// All <see cref="DrawableBoardBlock"/>s selected.
        /// </summary>
        public List<DrawableBoardBlock> SelectedBlocks { get; } = new List<DrawableBoardBlock>();

        /// <summary>
        /// All <see cref="FumoChessPiece"/>s placed on the board.
        /// </summary>
        public List<FumoChessPiece> ChessPieces { get; } = new List<FumoChessPiece>();

        private readonly bool lazyInitialization;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private FillFlowContainer<DrawableBoardBlock> boardBlockArea = null!;

        private Sample? placeChessSample;

        [Resolved]
        private LadderInfo ladderInfo { get; set; } = null!;

        public FumoChessBoard(bool lazyInitialization = false)
        {
            Name = @"Chess board";
            Width = BOARD_SIZE;
            Height = BOARD_SIZE;

            this.lazyInitialization = lazyInitialization;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, AudioManager audio)
        {
            var boardTexture = textures.Get("Board/board");

            placeChessSample = audio.Samples.Get("Board/place");

            InternalChild = new Container
            {
                Name = "Board container",
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                RelativePositionAxes = Axes.Both,
                Height = BOARD_SIZE,
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
                            Colour = Color4Extensions.FromHex("#454545"), Alpha = 0.74f, RelativeSizeAxes = Axes.Both,
                        },
                    boardBlockArea = new FillFlowContainer<DrawableBoardBlock>
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = FillDirection.Full,
                        Width = ladderInfo.MainBoardSize.Value,
                        Height = ladderInfo.MainBoardSize.Value,
                        ChildrenEnumerable = Blocks =
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
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ladderInfo.MainBoardSize.BindValueChanged(e =>
                boardBlockArea.ResizeTo(new Vector2(e.NewValue), 300, Easing.OutQuint));

            currentMatch.BindTo(ladderInfo.CurrentMatch);
            currentMatch.BindValueChanged(_ => initialize(), !lazyInitialization);
        }

        private void initialize()
        {
            if (!IsLoaded)
                return;

            ChessPieces.Clear();
            boardBlockArea.Children.ForEach(b => b.ChessLayer.Clear());

            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    var placement = currentMatch.Value?.ChessPlacements.LastOrDefault(p =>
                        p.BoardRow == i && p.BoardColumn == j);

                    if (placement != null)
                        AddSingleChess(placement.BeatmapID, i, j, placement.OwnerTeam, placement.CurrentType, true);
                }
            }
        }

        /// <summary>
        /// Remove all chess pieces from the board.
        /// </summary>
        public void Reset()
        {
            ChessPieces.Clear();
            boardBlockArea.Children.ForEach(b => b.ChessLayer.Clear());
        }

        /// <summary>
        /// Whether the board's block area can receive input of a specified position.
        /// </summary>
        /// <remarks>This is a wrapper for <see cref="Drawable.ReceivePositionalInputAt"/>.</remarks>
        /// <param name="screenSpacePos">The position relative to the screen space.</param>
        /// <returns><c>true</c> if the block area can receive input, otherwise <c>false</c>.</returns>
        public bool CanBlockAreaReceivesInput(Vector2 screenSpacePos)
            => boardBlockArea.ReceivePositionalInputAt(screenSpacePos);

        /// <summary>
        /// Add a <see cref="FumoChessPiece"/> to the specified position on the board.
        /// </summary>
        /// <remarks>The board won't verify if the placement is valid. Please do this elsewhere beforehand.</remarks>
        /// <param name="beatmapId">The ID of the beatmap corresponding to the placement.</param>
        /// <param name="row">The vertical position of the chess piece.</param>
        /// <param name="column">The horizontal position of the chess piece.</param>
        /// <param name="ownerTeam">The <see cref="TeamColour"/> placing the chess piece.</param>
        /// <param name="choiceType">The type of the placement.</param>
        /// <param name="omitSound">If <c>true</c>, the chess placement sound won't be played.</param>
        public void AddSingleChess(int beatmapId, int row, int column,
                                   TeamColour ownerTeam = TeamColour.None, ChoiceType choiceType = ChoiceType.Neutral,
                                   bool omitSound = false)
        {
            var block = Blocks.FirstOrDefault(b => b.BoardRow == row && b.BoardColumn == column);

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
            ChessPieces.Add(newPiece);

            newPiece.FadeIn(500, Easing.OutQuint);
            newPiece.ScaleTo(1.25f).Then().ScaleTo(1f, 900, Easing.OutQuint);

            if (!omitSound)
                placeChessSample?.Play();
        }
    }
}
