// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// Displays the data received over IPC in real time, along with an indication of how recently it was updated.
    /// </summary>
    public partial class IPCStatusDisplay : FillFlowContainer
    {
        /// <summary>
        /// The duration for which IPC data is considered up to date.
        /// </summary>
        public static readonly TimeSpan STALE_THRESHOLD = TimeSpan.FromSeconds(60);

        /// <summary>
        /// How often the displayed data is refreshed, in milliseconds.
        /// </summary>
        private const double refresh_interval = 500;

        [Resolved]
        private MatchIPCInfo ipc { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private OsuSpriteText beatmapValue = null!;
        private OsuSpriteText modsValue = null!;
        private OsuSpriteText stateValue = null!;
        private OsuSpriteText scoreValue = null!;
        private OsuSpriteText channelValue = null!;
        private OsuSpriteText lastUpdateValue = null!;
        private SettingsNote staleNote = null!;

        public IPCStatusDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(2);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                createRow(IPCStatusStrings.Beatmap, beatmapValue = createValueText()),
                createRow(IPCStatusStrings.Mods, modsValue = createValueText()),
                createRow(IPCStatusStrings.State, stateValue = createValueText()),
                createRow(IPCStatusStrings.Score, scoreValue = createValueText()),
                createRow(IPCStatusStrings.Channel, channelValue = createValueText()),
                createRow(IPCStatusStrings.LastUpdate, lastUpdateValue = createValueText()),
                staleNote = new SettingsNote
                {
                    RelativeSizeAxes = Axes.X,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ipc.Beatmap.BindValueChanged(_ => updateValues(), true);
            ipc.Mods.BindValueChanged(_ => updateValues());
            ipc.State.BindValueChanged(_ => updateValues());
            ipc.Score1.BindValueChanged(_ => updateValues());
            ipc.Score2.BindValueChanged(_ => updateValues());
            ipc.ChatChannel.BindValueChanged(_ => updateValues());

            Scheduler.AddDelayed(updateLastUpdate, refresh_interval, true);
        }

        private void updateValues()
        {
            TournamentBeatmap? beatmap = ipc.Beatmap.Value;

            beatmapValue.Text = beatmap == null
                ? IPCStatusStrings.None
                : IPCStatusStrings.BeatmapValue(beatmap.Metadata.Title, beatmap.DifficultyName, beatmap.OnlineID);

            LegacyMods mods = ipc.Mods.Value;
            modsValue.Text = mods == LegacyMods.None ? IPCStatusStrings.None : mods.ToString();

            stateValue.Text = ipc.State.Value.ToString();

            scoreValue.Text = IPCStatusStrings.ScoreValue(ipc.Score1.Value, ipc.Score2.Value);

            string channel = ipc.ChatChannel.Value;
            channelValue.Text = string.IsNullOrEmpty(channel) ? IPCStatusStrings.None : channel;
        }

        private void updateLastUpdate()
        {
            DateTime? lastUpdate = (ipc as FileBasedIPC)?.GetLastIPCUpdateTime();

            if (lastUpdate == null)
            {
                lastUpdateValue.Text = IPCStatusStrings.Unknown;
                staleNote.Current.Value = null;
                return;
            }

            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - lastUpdate.Value;

            if (elapsed < TimeSpan.Zero)
                elapsed = TimeSpan.Zero;

            lastUpdateValue.Text = IPCStatusStrings.LastUpdateTime(lastUpdate.Value.ToLocalTime().ToString(@"HH\:mm\:ss"), formatElapsed(elapsed));

            staleNote.Current.Value = IsStale(lastUpdate, now)
                ? new SettingsNote.Data(IPCStatusStrings.StaleHint(formatElapsed(elapsed)), SettingsNote.Type.Warning)
                : null;
        }

        /// <summary>
        /// Whether the supplied update time is old enough to indicate that no data is being received.
        /// </summary>
        /// <param name="lastUpdate">The last update time to check, or <c>null</c> if nothing has been received yet.</param>
        /// <param name="now">The current time.</param>
        public static bool IsStale(DateTime? lastUpdate, DateTime now) => lastUpdate != null && now - lastUpdate.Value > STALE_THRESHOLD;

        private static string formatElapsed(TimeSpan elapsed) => elapsed.ToHumanizedString(false);

        private FillFlowContainer createRow(LocalisableString label, OsuSpriteText value) => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(6, 0),
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = label,
                    Colour = colours.GrayA,
                    Font = OsuFont.Style.Caption1,
                },
                value,
            },
        };

        private static OsuSpriteText createValueText() => new OsuSpriteText
        {
            Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
            AllowMultiline = true,
        };
    }
}
