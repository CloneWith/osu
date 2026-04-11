// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.IO.Serialization;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Rulesets;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.Models;
using osu.Game.TournamentIpc;

namespace osu.Game.Tournament.IPC
{
    public partial class MatchIPCInfo : Component
    {
        public Bindable<TournamentBeatmap?> Beatmap { get; } = new Bindable<TournamentBeatmap?>();

        public BindableList<APIMod> Mods { get; } = new BindableList<APIMod>();

        public Bindable<TourneyState> State { get; } = new Bindable<TourneyState>();

        public BindableList<Message> ChatMessages { get; } = new BindableList<Message>();
        public BindableLong Score1 { get; } = new BindableLong();
        public BindableLong Score2 { get; } = new BindableLong();
    }

    public partial class FileBasedIPC : MatchIPCInfo
    {
        public Storage IPCStorage { get; private set; } = null!;

        [Resolved]
        protected IAPIProvider API { get; private set; } = null!;

        [Resolved]
        protected IRulesetStore Rulesets { get; private set; } = null!;

        // [Resolved]
        // private GameHost host { get; set; } = null!;

        [Resolved]
        private LadderInfo ladder { get; set; } = null!;

        private int lastBeatmapId;
        private string lastMods = string.Empty;

        // ReSharper disable once NotAccessedField.Local
        private ScheduledDelegate? scheduled;
        private ScheduledDelegate? refreshBackgroundDelegate;

        private GetBeatmapRequest? beatmapLookupRequest;

        [BackgroundDependencyLoader]
        private void load(TournamentStorage tournamentStorage)
        {
            IPCStorage = tournamentStorage.AllTournaments;
            Logger.Log($"ipc storage path: {IPCStorage.GetFullPath(string.Empty)}");
            Logger.Log(IPCStorage.Exists("ipc.txt") ? "file ipc.txt found in game storage yay" : "no ipc.txt found in game storage, uh oh", LoggingTarget.Runtime, LogLevel.Debug);

            if (IPCStorage.Exists("ipc.txt") && ladder.UseLazerIpc.Value)
            {
                scheduled = Scheduler.AddDelayed(delegate
                {
                    // beatmap
                    try
                    {
                        int beatmapId;
                        string requiredMods;

                        using (var stream = IPCStorage.GetStream(IpcFiles.BEATMAP))
                        using (var sr = new StreamReader(stream))
                        {
                            beatmapId = int.Parse(sr.ReadLine().AsNonNull());
                            requiredMods = sr.ReadLine().AsNonNull();
                        }

                        if (lastBeatmapId != beatmapId || lastMods != requiredMods)
                        {
                            Logger.Log($"hello! parsed mods: {requiredMods}");

                            lastBeatmapId = beatmapId;
                            lastMods = requiredMods;

                            // try to load the mods
                            try
                            {
                                var parsedMods = JsonConvert.DeserializeObject<APIMod[]>(requiredMods);

                                if (parsedMods != null)
                                {
                                    Logger.Log($"deserialized mods into array of apimods: {parsedMods.Length} mods");
                                    Mods.Clear();
                                    Mods.AddRange(parsedMods);
                                }
                                else
                                {
                                    Logger.Log($"Couldn't parse mods into apimods?");
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Log($"Couldn't parse mods string: {requiredMods} ({e.Message})");
                                Mods.Clear();
                            }

                            // id of -1: unsubmitted map
                            if (beatmapId != -1)
                            {
                                beatmapLookupRequest?.Cancel();
                                var existing = ladder
                                               .CurrentMatch.Value
                                               ?.Round.Value
                                               ?.Beatmaps
                                               .FirstOrDefault(b => b.ID == beatmapId);

                                if (existing != null)
                                    Beatmap.Value = existing.Beatmap;
                                else
                                {
                                    beatmapLookupRequest = new GetBeatmapRequest(new APIBeatmap { OnlineID = beatmapId });
                                    beatmapLookupRequest.Success += b =>
                                    {
                                        if (lastBeatmapId == beatmapId)
                                            Beatmap.Value = new TournamentBeatmap(b);
                                    };
                                    // beatmapLookupRequest.Failure += _ =>
                                    // {
                                    //     if (lastBeatmapId == beatmapId)
                                    //         Beatmap.Value = null;
                                    // };
                                    API.Queue(beatmapLookupRequest);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // file might be in use
                    }

                    // beatmap metadata (lookup via MD5 instead of API)
                    parseBeatmapMetadata();

                    // chat
                    try
                    {
                        using (var stream = IPCStorage.GetStream(IpcFiles.CHAT))
                        using (var sr = new StreamReader(stream))
                        {
                            if (sr.Peek() == -1)
                            {
                                ChatMessages.Clear();
                            }

                            bool isFirstLine = true;

                            while (sr.ReadLine() is { } line)
                            {
                                string[] parts = line.Split(',');
                                if (parts.Length < 4) continue;

                                bool parseOk = long.TryParse(parts[0], out long ts);
                                parseOk &= int.TryParse(parts[2], out int uid);
                                if (!parseOk) continue;

                                if (isFirstLine)
                                {
                                    isFirstLine = false;
                                    ChatMessages.RemoveAll(msg => msg.Timestamp.ToUnixTimeMilliseconds() < ts);
                                }

                                if ((ChatMessages.LastOrDefault()?.Timestamp.ToUnixTimeMilliseconds() ?? 0) >= ts) continue;

                                Logger.Log($"added chat message {line}", LoggingTarget.Runtime, LogLevel.Debug);
                                ChatMessages.Add(new Message
                                {
                                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ts),
                                    Sender = new APIUser
                                    {
                                        Id = uid,
                                        Username = parts[1]
                                    },
                                    Content = string.Join(",", parts.Skip(3))
                                });
                            }
                        }
                    }
                    catch
                    {
                    }

                    // scores
                    try
                    {
                        using var stream = IPCStorage.GetStream(IpcFiles.SCORES);
                        using var sr = new StreamReader(stream);

                        // we expect exactly two values in this file, always
                        Score1.Value = long.Parse(sr.ReadLine().AsNonNull());
                        Score2.Value = long.Parse(sr.ReadLine().AsNonNull());
                    }
                    catch
                    {
                        // file might be busy
                    }

                    // state
                    try
                    {
                        using var stream = IPCStorage.GetStream(IpcFiles.STATE);
                        using var sr = new StreamReader(stream);

                        State.Value = Enum.Parse<TourneyState>(sr.ReadLine().AsNonNull());
                    }
                    catch
                    {
                        // file might be busy
                    }
                }, 250, true);
            }
        }

        private void parseBeatmapMetadata()
        {
            try
            {
                if (lastBeatmapId != -1)
                    return;

                BeatmapInfo beatmapInfo;

                using (var stream = IPCStorage.GetStream(IpcFiles.BEATMAP_METADATA))
                using (var sr = new StreamReader(stream))
                {
                    beatmapInfo = sr.ReadToEnd().Deserialize<BeatmapInfo>();
                }

                if (beatmapInfo == null || Beatmap.Value?.MD5Hash == beatmapInfo.MD5Hash)
                    return;

                bool bgFileReady = IPCStorage.Exists(IpcFiles.BEATMAP_BACKGROUND);
                Beatmap.Value = new TournamentBeatmap(beatmapInfo, new BeatmapSetOnlineCovers { Cover = bgFileReady ? IpcFiles.BEATMAP_BACKGROUND : "" });

                if (bgFileReady)
                    return;

                // force an update on Beatmap.Value so the background gets reloaded
                refreshBackgroundDelegate?.Cancel();

                refreshBackgroundDelegate = Scheduler.AddDelayed(() =>
                {
                    if (Beatmap.Value.MD5Hash != beatmapInfo.MD5Hash)
                    {
                        refreshBackgroundDelegate?.Cancel();
                        return;
                    }

                    if (!IPCStorage.Exists(IpcFiles.BEATMAP_BACKGROUND))
                        return;

                    // file now exists, we can stop polling for it
                    refreshBackgroundDelegate?.Cancel();
                    Beatmap.Value = new TournamentBeatmap(beatmapInfo, new BeatmapSetOnlineCovers { Cover = IpcFiles.BEATMAP_BACKGROUND });
                }, 200, true);
            }
            catch
            {
                // ignore, the file might be in use
            }
        }
    }
}
