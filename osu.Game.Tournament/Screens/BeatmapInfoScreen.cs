// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Online.API;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens
{
    public abstract partial class BeatmapInfoScreen : TournamentMatchScreen
    {
        protected readonly SongBar SongBar;
        private readonly IBindable<TournamentBeatmap?> beatmap = new Bindable<TournamentBeatmap?>();
        private readonly Bindable<LegacyMods> legacyMods = new Bindable<LegacyMods>();
        private readonly BindableList<APIMod> lazerMods = new BindableList<APIMod>();

        protected BeatmapInfoScreen()
        {
            AddInternal(SongBar = new SongBar
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Depth = float.MinValue,
            });
        }

        [BackgroundDependencyLoader]
        private void load(LegacyMatchIPCInfo legacyIpc, MatchIPCInfo lazerIpc, LadderInfo ladder)
        {
            ladder.UseLazerIpc.BindValueChanged(
                vce =>
                {
                    beatmap.UnbindAll();
                    beatmap.BindTo(vce.NewValue ? lazerIpc.Beatmap : legacyIpc.Beatmap);

                    lazerMods.UnbindBindings();
                    legacyMods.UnbindBindings();

                    if (vce.NewValue)
                    {
                        lazerMods.BindTo(lazerIpc.Mods);
                        lazerMods.BindCollectionChanged(lazerModsChanged, true);
                    }
                    else
                    {
                        legacyMods.BindTo(legacyIpc.Mods);
                        legacyMods.BindValueChanged(modsChanged, true);
                    }
                },
                true);

            beatmap.BindValueChanged(beatmapChanged, true);
        }

        private void lazerModsChanged(object? _, NotifyCollectionChangedEventArgs e)
        {
            var modsList = lazerMods.ToList();
            Logger.Log($"Got mods for song bar: {string.Join(", ", modsList.Select(JsonConvert.SerializeObject))}");
            SongBar.LazerMods = modsList;
        }

        private void modsChanged(ValueChangedEvent<LegacyMods> mods)
        {
            SongBar.Mods = mods.NewValue;
        }

        private void beatmapChanged(ValueChangedEvent<TournamentBeatmap?> beatmap)
        {
            SongBar.Beatmap = beatmap.NewValue;
        }
    }
}
