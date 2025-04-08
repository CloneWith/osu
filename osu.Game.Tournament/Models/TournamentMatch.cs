// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// A collection of two teams competing in a head-to-head match.
    /// </summary>
    [Serializable]
    public class TournamentMatch
    {
        public int ID;

        [JsonIgnore]
        public List<string> Acronyms
        {
            get
            {
                List<string> acronyms = new List<string>();
                if (Team1Acronym != null) acronyms.Add(Team1Acronym);
                if (Team2Acronym != null) acronyms.Add(Team2Acronym);
                return acronyms;
            }
        }

        [JsonIgnore]
        public readonly Bindable<TournamentTeam?> Team1 = new Bindable<TournamentTeam?>();

        public string? Team1Acronym;

        public readonly Bindable<int?> Team1Score = new Bindable<int?>();

        [JsonIgnore]
        public readonly Bindable<TournamentTeam?> Team2 = new Bindable<TournamentTeam?>();

        public string? Team2Acronym;

        public readonly Bindable<int?> Team2Score = new Bindable<int?>();

        public readonly Bindable<bool> Completed = new Bindable<bool>();

        public readonly Bindable<bool> Losers = new Bindable<bool>();

        public readonly ObservableCollection<BeatmapChoice> PicksBans = new ObservableCollection<BeatmapChoice>();

        public readonly ObservableCollection<ChessPlacement> ChessPlacements = new ObservableCollection<ChessPlacement>();

        [JsonIgnore]
        public readonly Bindable<TournamentRound?> Round = new Bindable<TournamentRound?>();

        [JsonIgnore]
        public readonly Bindable<TournamentMatch?> Progression = new Bindable<TournamentMatch?>();

        [JsonIgnore]
        public readonly Bindable<TournamentMatch?> LosersProgression = new Bindable<TournamentMatch?>();

        /// <summary>
        /// Should not be set directly. Use LadderInfo.CurrentMatch.Value = this instead.
        /// </summary>
        public readonly Bindable<bool> Current = new Bindable<bool>();

        public readonly Bindable<DateTimeOffset> Date = new Bindable<DateTimeOffset>(DateTimeOffset.Now);

        [JsonProperty]
        public readonly BindableList<ConditionalTournamentMatch> ConditionalMatches = new BindableList<ConditionalTournamentMatch>();

        public readonly Bindable<Point> Position = new Bindable<Point>();

        public TournamentMatch()
        {
            Team1.BindValueChanged(t => Team1Acronym = t.NewValue?.Acronym.Value, true);
            Team2.BindValueChanged(t => Team2Acronym = t.NewValue?.Acronym.Value, true);
        }

        public TournamentMatch(TournamentTeam? team1 = null, TournamentTeam? team2 = null)
            : this()
        {
            Team1.Value = team1;
            Team2.Value = team2;
        }

        [JsonIgnore]
        public TournamentTeam? Winner => !Completed.Value ? null : Team1Score.Value > Team2Score.Value ? Team1.Value : Team2.Value;

        [JsonIgnore]
        public TournamentTeam? Loser => !Completed.Value ? null : Team1Score.Value > Team2Score.Value ? Team2.Value : Team1.Value;

        public TeamColour WinnerColour => Winner == Team1.Value ? TeamColour.Red : TeamColour.Blue;

        public int PointsToWin => Round.Value?.BestOf.Value / 2 + 1 ?? 0;

        /// <summary>
        /// Remove scores from the match, in case of a false click or false start.
        /// </summary>
        public void CancelMatchStart()
        {
            Team1Score.Value = null;
            Team2Score.Value = null;
        }

        /// <summary>
        /// Initialise this match with zeroed scores. Will be a noop if either team is not present or if either of the scores are non-zero.
        /// </summary>
        public void StartMatch()
        {
            if (Team1.Value == null || Team2.Value == null)
                return;

            if (Team1Score.Value > 0 || Team2Score.Value > 0)
                return;

            Team1Score.Value = 0;
            Team2Score.Value = 0;
        }

        /// <summary>
        /// Search for the maximum successive chess pieces on the board for two teams.
        /// </summary>
        /// <returns>A tuple containing the number of successive chess for the red and blue team.</returns>
        public (int redNum, int blueNum) GetMaximumSuccessiveChess()
        {
            (int red, int blue) num = (0, 0);

            (int row, int column)[] directions = [(0, 1), (1, 0), (1, 1), (1, -1)];

            for (int i = 1; i <= 4; i++)
            {
                // The modification of i won't affect these lines.
                // ReSharper disable once AccessToModifiedClosure
                var rowChess = ChessPlacements.Where(c => c.BoardRow == i);

                foreach (var chess in rowChess)
                {
                    if (chess.CurrentType == ChoiceType.RedWin)
                    {
                        foreach (var d in directions)
                            num.red = Math.Max(num.red, progress(d.row, d.column, ChoiceType.RedWin, chess.BoardRow, chess.BoardColumn));
                    }
                    else if (chess.CurrentType == ChoiceType.BlueWin)
                    {
                        foreach (var d in directions)
                            num.blue = Math.Max(num.blue, progress(d.row, d.column, ChoiceType.BlueWin, chess.BoardRow, chess.BoardColumn));
                    }
                }
            }

            if (num.red == 1) num.red = 0;
            if (num.blue == 1) num.blue = 0;

            return num;

            int progress(int rowDelta, int columnDelta, ChoiceType targetType, int row, int column, int current = 0)
            {
                // Step 1: Boundary check
                if (row <= 0 || row > 4 || column <= 0 || column > 4)
                    goto EndRecursion;

                // Step 2: Find the next chess; Return if not found or not desired type
                var nextChess = ChessPlacements.FirstOrDefault(c => c.BoardRow == row && c.BoardColumn == column);

                if (nextChess == null || nextChess.CurrentType != targetType)
                    // Edge case: Dismiss dual diagonal matches
                    goto EndRecursion;

                // Step 3: Search forwards
                return progress(rowDelta, columnDelta, targetType, row + rowDelta, column + columnDelta, ++current);

#pragma warning disable format
                // This is EXACTLY the code format we expected.
                EndRecursion:
                return rowDelta == 1 && columnDelta != 0 && current <= 2 ? 0 : current;
#pragma warning restore format
            }
        }

        public void Reset()
        {
            CancelMatchStart();
            Team1.Value = null;
            Team2.Value = null;
            Completed.Value = false;
            PicksBans.Clear();
        }
    }
}
