// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentMatchChatDisplay : StandAloneChatDisplay
    {
        private readonly Bindable<string> channelName = new Bindable<string>();

        private ChannelManager? manager;

        private MatchIPCInfo? ipc;
        private IAPIProvider? api;

        private int channelId;

        [Resolved]
        private LadderInfo ladderInfo { get; set; } = null!;

        public TournamentMatchChatDisplay(float cornerRadius = 0, bool autoSizeY = false, bool relativeSizeY = false, float bkgAlpha = 0)
        {
            AutoSizeAxes = autoSizeY ? Axes.Y : Axes.None;
            RelativeSizeAxes = relativeSizeY ? Axes.Both : Axes.X;
            if (!autoSizeY) Height = relativeSizeY ? 1f : 144;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Background.Alpha = bkgAlpha;

            CornerRadius = cornerRadius;
        }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc, IAPIProvider api)
        {
            this.ipc = ipc;
            this.api = api;

            AddInternal(manager = new ChannelManager(api));
            Channel.BindTo(manager.CurrentChannel);

            channelName.BindTo(ipc.ChatChannel);
            channelName.BindValueChanged(c =>
            {
                if (int.TryParse(c.OldValue, out int oldChannelId) && oldChannelId > 0)
                {
                    var joinedChannel = manager.JoinedChannels.SingleOrDefault(ch => ch.Id == oldChannelId);
                    if (joinedChannel != null)
                        manager.LeaveChannel(joinedChannel);
                }

                if (int.TryParse(c.NewValue, out int newChannelId) && newChannelId > 0)
                {
                    var channel = new Channel
                    {
                        Id = newChannelId,
                        Type = ChannelType.Public
                    };

                    manager.JoinChannel(channel);
                    manager.CurrentChannel.Value = channel;
                }
            }, true);
        }

        public void ReloadChannel()
        {
            if (ipc == null) return;

            if (string.IsNullOrWhiteSpace(channelName.Value)) return;

            channelId = int.Parse(channelName.Value);

            if (manager == null)
            {
                AddInternal(manager = new ChannelManager(api));
                Channel.BindTo(manager.CurrentChannel);
            }

            Channel? channel = manager.JoinedChannels.FirstOrDefault(p => p.Id == channelId);

            if (channel == null)
            {
                channel = new Channel
                {
                    Id = channelId,
                    Type = ChannelType.Public,
                };
                manager.JoinChannel(channel);
            }

            manager.CurrentChannel.Value = channel;
        }

        public void ChangeRadius(int radius)
        {
            CornerRadius = radius;
        }

        protected override ChatLine? CreateMessage(Message message)
        {
            if (message.Content.StartsWith("!mp", StringComparison.Ordinal))
                return null;

            var currentMatch = ladderInfo.CurrentMatch;
            bool isRef = currentMatch.Value?.Round.Value?.Referees.Any(p => p.OnlineID == message.SenderId) ?? false;

            return new MatchMessage(message, ladderInfo)
            {
                IsBackgroundInverted = isRef,
            };
        }

        protected override StandAloneDrawableChannel CreateDrawableChannel(Channel channel) => new MatchChannel(channel);

        public partial class MatchChannel : StandAloneDrawableChannel
        {
            public MatchChannel(Channel channel)
                : base(channel)
            {
                ScrollbarVisible = false;
            }
        }

        protected partial class MatchMessage : StandAloneMessage
        {
            public MatchMessage(Message message, LadderInfo info)
                : base(message)
            {
                // Disable line background alternating, see https://github.com/ppy/osu/pull/29137
                EnableBackgroundAlternating = false;

                if (info.CurrentMatch.Value is not TournamentMatch match) return;

                if (match.Team1.Value?.Players.Any(u => u.OnlineID == Message.Sender.OnlineID) == true)
                    UsernameColour = TournamentGame.COLOUR_RED;
                else if (match.Team2.Value?.Players.Any(u => u.OnlineID == Message.Sender.OnlineID) == true)
                    UsernameColour = TournamentGame.COLOUR_BLUE;
            }
        }
    }
}
