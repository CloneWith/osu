// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.IO.Serialization;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Game.TournamentIpc
{
    public static class IpcFiles
    {
        public const string BEATMAP = @"ipc.txt";
        public const string STATE = @"ipc-state.txt";
        public const string SCORES = @"ipc-scores.txt";
        public const string CHAT = @"ipc-chat.txt";
        public const string BEATMAP_METADATA = @"ipc-meta.txt";
        public const string BEATMAP_BACKGROUND = @"ipc-bg.png";
    }

    // am I being paranoid with the locks? Not familiar with threading model in C#
    public partial class TournamentFileBasedIPC : Component
    {
        private Storage tournamentStorage = null!;

        private Task? beatmapWriteOperation;
        private Task? beatmapMetaWriteOperation;
        private Task? beatmapBackgroundWriteOperation;
        private Task? scoresWriteOperation;
        private Task? chatMessagesWriteOperation;

        private readonly List<NotifyCollectionChangedEventArgs> newMessagesEventsQueue = new List<NotifyCollectionChangedEventArgs>();
        private readonly HashSet<string> seenMessages = new HashSet<string>();

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        private string playlistItemSerializedMods = string.Empty;

        private MultiplayerClient? multiplayerClient;

        public Bindable<TourneyState> TourneyState { get; } = new Bindable<TourneyState>();
        // private MultiplayerRoomState? lastRoomState = null;

        private readonly BindableList<Message> chatMessages = new BindableList<Message>();

        private long[] pendingScores = [];

        private ScheduledDelegate? flushScoresDelegate;
        private ScheduledDelegate? flushChatMessagesDelegate;

        private void updateChatMessages()
        {
            if (chatMessagesWriteOperation?.IsCompleted == false)
                return;

            chatMessagesWriteOperation = Task.Run(chatMessagesWriter);
        }

        private void chatMessagesWriter()
        {
            try
            {
                // else append to file normally
                using var chatAppendIpcStream = tournamentStorage.GetStream(IpcFiles.CHAT, FileAccess.Write, FileMode.Append);
                using var chatIpcStreamWriter = new StreamWriter(chatAppendIpcStream);

                int msgWritten = 0;

                long lastMsgTimestamp = 0;
                int msgDisambiguator = 0;

                foreach (Message message in newMessagesEventsQueue.SelectMany(newMessageEvent => newMessageEvent.NewItems?.OfType<Message>() ?? Array.Empty<Message>()))
                {
                    long msgTimestamp = message.Timestamp.ToUnixTimeMilliseconds();

                    if (msgTimestamp != lastMsgTimestamp)
                        msgDisambiguator = 0;

                    msgTimestamp += msgDisambiguator++;

                    string ipcMsgContent = $"{msgTimestamp},{message.Sender.Username},{message.Sender.Id},{message.Content}\n";

                    if (!seenMessages.Contains(ipcMsgContent))
                    {
                        chatIpcStreamWriter.Write(ipcMsgContent);
                        seenMessages.Add(ipcMsgContent);
                    }

                    msgWritten++;
                    lastMsgTimestamp = msgTimestamp;
                }

                newMessagesEventsQueue.Clear();

                if (msgWritten > 0)
                    Logger.Log($@"[FileIPC] Wrote {msgWritten} message(s) to file");
            }
            catch
            {
                Logger.Log("failed writing chat messages to ipc file");
            }
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage, IBindable<WorkingBeatmap> workingBeatmap)
        {
            tournamentStorage = storage.GetStorageForDirectory(@"tournaments");

            // clear chat file on game launch
            using (var chatIpcStream = tournamentStorage.CreateFileSafely(IpcFiles.CHAT))
                chatIpcStream.SetLength(0);

            chatMessages.BindCollectionChanged((_, messages) => newMessagesEventsQueue.Add(messages), true);

            Logger.Log(@"started watching for tourney state changes");

            TourneyState.BindValueChanged(vce =>
            {
                try
                {
                    using var mainIpc = tournamentStorage.CreateFileSafely(IpcFiles.STATE);
                    using var mainIpcStreamWriter = new StreamWriter(mainIpc);

                    Logger.Log($"tourney state changed to: {vce.NewValue}");
                    mainIpcStreamWriter.Write($"{(int)vce.NewValue}\n");
                }
                catch
                {
                    Logger.Log("failed writing tourney state to ipc file, trying again in 50ms");
                    Scheduler.AddDelayed(() => TourneyState.TriggerChange(), 50);
                }
            }, true);

            flushScoresDelegate?.Cancel();
            flushScoresDelegate = Scheduler.AddDelayed(flushPendingScoresToDisk, 200, true);

            flushChatMessagesDelegate?.Cancel();
            flushChatMessagesDelegate = Scheduler.AddDelayed(updateChatMessages, 500, true);
        }

        public void AddChatMessage(Message message)
        {
            chatMessages.Add(message);
        }

        public void ClearChatMessages()
        {
            chatMessages.Clear();
            using (var chatIpcStream = tournamentStorage.CreateFileSafely(IpcFiles.CHAT))
                chatIpcStream.SetLength(0);

            seenMessages.Clear();
        }

        public void UpdateTeamScores(long[] scores)
        {
            pendingScores = scores;
        }

        private void beatmapIdWriter(int beatmapId, string modsJson)
        {
            try
            {
                using var mainIpc = tournamentStorage.CreateFileSafely(IpcFiles.BEATMAP);
                using var mainIpcStreamWriter = new StreamWriter(mainIpc);

                mainIpcStreamWriter.Write($"{beatmapId}\n");
                mainIpcStreamWriter.Write($"{modsJson}\n");
            }
            catch
            {
                Logger.Log("failed writing updated beatmap id to ipc file, trying again in 50ms");
                Scheduler.AddDelayed(() => updateActiveBeatmapID(beatmapId, modsJson), 50);
            }
        }

        private void updateActiveBeatmapID(int beatmapId, string modsJson)
        {
            Logger.Log($"new active beatmap: {beatmapId}");

            beatmapWriteOperation = beatmapWriteOperation?.ContinueWith(_ => { beatmapIdWriter(beatmapId, modsJson); })
                                    ?? Task.Run(() => beatmapIdWriter(beatmapId, modsJson));
        }

        private void beatmapMetaWriter(BeatmapInfo beatmapMetadata)
        {
            try
            {
                using var metadataIpc = tournamentStorage.CreateFileSafely(IpcFiles.BEATMAP_METADATA);
                using var metadataIpcStreamWriter = new StreamWriter(metadataIpc);

                metadataIpcStreamWriter.Write(beatmapMetadata.Serialize());
            }
            catch
            {
                Logger.Log("failed writing updated beatmap metadata to ipc file, trying again in 50ms");
                Scheduler.AddDelayed(() => updateActiveBeatmapMetadata(beatmapMetadata), 50);
            }
        }

        private void updateActiveBeatmapMetadata(BeatmapInfo beatmapMetadata)
        {
            // write beatmap metadata
            beatmapMetaWriteOperation = beatmapMetaWriteOperation?.ContinueWith(_ => { beatmapMetaWriter(beatmapMetadata); })
                                        ?? Task.Run(() => beatmapMetaWriter(beatmapMetadata));
        }

        private void beatmapBackgroundWriter(WorkingBeatmap beatmap)
        {
            string? backgroundImageStorePath = beatmap.BeatmapSetInfo.GetPathForFile(beatmap.Metadata.BackgroundFile);

            if (backgroundImageStorePath == null)
                return;

            Logger.Log($@"Got background image path: {backgroundImageStorePath}");

            try
            {
                tournamentStorage.Delete(IpcFiles.BEATMAP_BACKGROUND);
                using Stream backgroundIpc = tournamentStorage.CreateFileSafely(IpcFiles.BEATMAP_BACKGROUND);
                using Stream input = beatmap.GetStream(backgroundImageStorePath);

                using Image<Rgba32> img = Image.Load<Rgba32>(input);

                img.Save(backgroundIpc, new PngEncoder());
            }
            catch (Exception ex)
            {
                const int retry_delay = 200;
                Logger.Log($@"caught unexpected exception while trying to flush background image, retrying in {retry_delay}ms: {ex.Message}");
                Scheduler.AddDelayed(() => updateActiveBeatmapBackgroundImage(beatmap), retry_delay);
            }
        }

        private void updateActiveBeatmapBackgroundImage(WorkingBeatmap beatmap)
        {
            beatmapBackgroundWriteOperation = beatmapBackgroundWriteOperation?.ContinueWith(_ => { beatmapBackgroundWriter(beatmap); })
                                              ?? Task.Run(() => beatmapBackgroundWriter(beatmap));
        }

        public void RegisterMultiplayerRoomClient(MultiplayerClient multiplayerClient)
        {
            if (this.multiplayerClient != null)
                this.multiplayerClient.RoomUpdated -= onRoomUpdated;

            this.multiplayerClient = multiplayerClient;
            this.multiplayerClient.RoomUpdated += onRoomUpdated;
            onRoomUpdated();
        }

        private void onRoomUpdated()
        {
            var nextMapInPlaylist = multiplayerClient?.Room?.Playlist.LastOrDefault(i => i.BeatmapID == workingBeatmap.Value.Beatmap.BeatmapInfo.OnlineID);

            if (nextMapInPlaylist != null)
            {
                string newModsStr = JsonConvert.SerializeObject(nextMapInPlaylist.RequiredMods);

                if (newModsStr != playlistItemSerializedMods)
                {
                    playlistItemSerializedMods = newModsStr;
                    updateActiveBeatmapID(workingBeatmap.Value.Beatmap.BeatmapInfo.OnlineID, newModsStr);
                }
            }

            var newRoomState = multiplayerClient?.Room?.State ?? MultiplayerRoomState.Closed;

            switch (newRoomState)
            {
                // don't transition to Playing state while waiting for players to load
                case MultiplayerRoomState.WaitingForLoad:
                    break;

                case MultiplayerRoomState.Playing:
                    TourneyState.Value = TournamentIpc.TourneyState.Playing;
                    break;

                default:
                    // there is at least one user in results screen
                    if (TourneyState.Value != TournamentIpc.TourneyState.Lobby)
                    {
                        Logger.Log($"(room updated) tourney state changed to: {TournamentIpc.TourneyState.Ranking}");
                        TourneyState.Value = TournamentIpc.TourneyState.Ranking;
                        break;
                    }

                    TourneyState.Value = TournamentIpc.TourneyState.Lobby;
                    break;
            }
        }

        protected override void LoadComplete()
        {
            workingBeatmap.BindValueChanged(beatmapChangedEvent =>
            {
                Logger.Log($@"working beatmap changed to {beatmapChangedEvent.NewValue.Beatmap.BeatmapInfo.OnlineID}");
                updateActiveBeatmapID(beatmapChangedEvent.NewValue.Beatmap.BeatmapInfo.OnlineID, playlistItemSerializedMods);
                updateActiveBeatmapMetadata(beatmapChangedEvent.NewValue.Beatmap.BeatmapInfo);
                updateActiveBeatmapBackgroundImage(beatmapChangedEvent.NewValue);
            });
        }

        private void pendingScoresWriter(List<long> scoresToWrite)
        {
            // ensure there is always at least 2 scores to write
            if (scoresToWrite.Count == 1)
                scoresToWrite.Add(0);

            try
            {
                using (var scoresIpc = tournamentStorage.CreateFileSafely(IpcFiles.SCORES))
                using (var scoresIpcWriter = new StreamWriter(scoresIpc))
                {
                    foreach (long score in scoresToWrite)
                    {
                        scoresIpcWriter.Write($"{score}\n");
                    }
                }
            }
            catch
            {
                // file might be busy
            }
        }

        private void flushPendingScoresToDisk()
        {
            if (scoresWriteOperation?.IsCompleted == false) return;

            if (pendingScores.Length == 0)
                return;

            var scoresToWrite = pendingScores.ToList();
            scoresWriteOperation = Task.Run(() => pendingScoresWriter(scoresToWrite)).ContinueWith(_ => Schedule(() => { pendingScores = []; }));
        }
    }
}
