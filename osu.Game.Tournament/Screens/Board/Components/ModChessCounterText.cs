// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class ModChessCounterText : TournamentSpriteText
    {
        public readonly string ModAcronym;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private int? currentCount;

        public ModChessCounterText(string acronym)
        {
            ModAcronym = acronym;
            var colourScheme = ModColours.FromModString(acronym);

            Colour = colourScheme.Accent;
            Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 18);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ladder.CurrentMatch.BindValueChanged(matchChanged);
            ladder.CurrentMatch.Value?.Round.BindValueChanged(_ => updateCounter());
            ladder.CurrentMatch.Value?.Round.Value?.Beatmaps.BindCollectionChanged((_, _) => updateCounter());

            if (ladder.CurrentMatch.Value != null)
                ladder.CurrentMatch.Value.ChessPlacements.CollectionChanged += placementChanged;

            updateCounter();
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.ChessPlacements.CollectionChanged -= placementChanged;
            }

            if (e.NewValue != null)
            {
                e.NewValue.ChessPlacements.CollectionChanged += placementChanged;
                e.NewValue.Round.BindValueChanged(_ => updateCounter());
                e.NewValue.Round.Value?.Beatmaps.BindCollectionChanged((_, _) => updateCounter());
            }

            updateCounter();
        }

        private void placementChanged(object? _, NotifyCollectionChangedEventArgs __)
            => Scheduler.AddOnce(updateCounter);

        private void updateCounter()
        {
            int? mapCount = ladder.CurrentMatch.Value?.Round.Value?.Beatmaps
                                  .Count(b => b.Mods.Equals(ModAcronym, StringComparison.OrdinalIgnoreCase)
                                              && ladder.CurrentMatch.Value?.ChessPlacements.Any(p => p.BeatmapID == b.ID) != true);

            // Flash when availability changes or number updates.
            if (mapCount == null || mapCount != currentCount)
                this.FlashColour(Color4.White, 1000, Easing.OutQuint);

            Text = $@"{ModAcronym}x{(mapCount != null ? mapCount.ToString() : "?")}";
            currentCount = mapCount;
        }
    }
}
