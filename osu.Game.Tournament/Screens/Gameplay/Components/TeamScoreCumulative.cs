// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class TeamScoreCumulative : CommaSeparatedScoreCounter<long>
    {
        private OsuSpriteText displayedSpriteText = null!;
        private const int font_size = 50;
        private Bindable<bool> useCumulativeScore = null!;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private readonly BindableDictionary<string,Tuple<long,long>> mapScores = new BindableDictionary<string, Tuple<long, long>>();
        private readonly IBindable<TournamentBeatmap?> beatmap = new Bindable<TournamentBeatmap?>();
        private readonly TeamColour teamColour;

        public TeamScoreCumulative(TeamColour colour)
        {
            teamColour = colour;
        }

        [BackgroundDependencyLoader]
        private void load(LegacyMatchIPCInfo legacyIpc, MatchIPCInfo lazerIpc)
        {
            ladder.UseLazerIpc.BindValueChanged(
                vce =>
                {
                    beatmap.UnbindAll();
                    beatmap.BindTo(vce.NewValue ? lazerIpc.Beatmap : legacyIpc.Beatmap);
                },
                true);

            // delay cumulative score update until sets are updated
            beatmap.BindValueChanged(_ => Schedule(updateCumulativeScore), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            useCumulativeScore = ladder.CumulativeScore.GetBoundCopy();
            useCumulativeScore.BindValueChanged(v => displayedSpriteText.Alpha = v.NewValue ? 1 : 0, true);

            ladder.CurrentMatch.BindValueChanged(vce =>
            {
                mapScores.UnbindBindings();

                if (vce.NewValue == null)
                    return;

                mapScores.BindTo(vce.NewValue.MapScores);
            }, true);

            mapScores.BindCollectionChanged((_, e) =>
            {
                if (e.NewItems != null)
                    Logger.Log($"mapScores changed: {string.Join(",", e.NewItems.Select(i => i.Key))}");

                updateCumulativeScore();
            }, true);
        }

        private void updateCumulativeScore()
        {
            // if (ladder.CurrentMatch.Value?.Round.Value == null)
            //     return;
            //
            // if (beatmap.Value == null)
            //     return;
            //
            // // get the current set
            // var currentSet = ladder.CurrentMatch.Value.Sets.FirstOrDefault(s => s.Map1Id.Value == beatmap.Value.OnlineID || s.Map2Id.Value == beatmap.Value.OnlineID);
            //
            // if (currentSet == null)
            //     return;
            //
            // // lookup roundbeatmap
            // var map1RoundBeatmap = ladder.CurrentMatch.Value.Round.Value?.Beatmaps.FirstOrDefault(poolMap => poolMap.ID == currentSet.Map1Id.Value);
            // var map2RoundBeatmap = ladder.CurrentMatch.Value.Round.Value?.Beatmaps.FirstOrDefault(poolMap => poolMap.ID == currentSet.Map2Id.Value);
            //
            // if (map1RoundBeatmap == null && map2RoundBeatmap == null)
            //     return;
            //
            // // get scores from both maps
            // long score = 0;
            //
            // if (ladder.CurrentMatch.Value.MapScores.TryGetValue(map1RoundBeatmap?.SlotName ?? "INVALID_SLOT", out var map1Score))
            //     score += teamColour == TeamColour.Red ? map1Score.Item1 : map1Score.Item2;
            //
            // if (ladder.CurrentMatch.Value.MapScores.TryGetValue(map2RoundBeatmap?.SlotName ?? "INVALID_SLOT", out var map2Score))
            //     score += teamColour == TeamColour.Red ? map2Score.Item1 : map2Score.Item2;

            if (ladder.CurrentMatch.Value?.Round.Value == null)
                return;

            if (beatmap.Value == null)
                return;

            var scores = MatchSet.GetSetScores(ladder.CurrentMatch.Value, beatmap.Value.OnlineID);

            if (scores == null)
            {
                Current.Value = 0;
                return;
            }

            Current.Value = teamColour == TeamColour.Red ? scores.Item1 : scores.Item2;
        }

        protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(s =>
        {
            displayedSpriteText = s;
            displayedSpriteText.Spacing = new Vector2(-6);
            displayedSpriteText.Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: font_size, fixedWidth: true);
        });
    }
}
