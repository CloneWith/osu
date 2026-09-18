// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Caching;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Draft.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Draft
{
    /// <summary>
    /// The drafting half of the player selection: the remaining players are placed into the groups which the
    /// random phase started, and the resulting rosters can be committed to the bracket teams.
    /// </summary>
    public partial class DraftScreen : TournamentScreen
    {
        private const float content_padding = 10;
        private const float column_height = 748;
        private const float column_spacing = 10;
        private const int groups_per_row = 2;

        /// <summary>How many player panels share a row in the pool.</summary>
        private const int pool_columns = 4;

        /// <summary>The gap between two player panels, horizontally and vertically.</summary>
        private const float pool_spacing = 5;

        /// <summary>
        /// The width of the pool column: a whole number of panels plus the scrollbar the listing keeps out of
        /// its content, with a couple of pixels to spare so that a rounding error cannot drop it to three panels
        /// a row.
        /// </summary>
        private const float pool_column_width = DraftUserPanel.WIDTH * pool_columns
                                                + pool_spacing * (pool_columns - 1)
                                                + OsuScrollContainer.SCROLL_BAR_WIDTH
                                                + 3;

        /// <summary>
        /// The width of the group column: whatever the pool does not take. The groups spend it on as few rows of
        /// two as it allows, which is what keeps them out of the pool's way.
        /// </summary>
        private const float group_column_width = TournamentExtensions.STREAM_AREA_WIDTH - content_padding * 2 - column_spacing - pool_column_width;

        [Resolved]
        private TournamentUserCache userCache { get; set; } = null!;

        [Resolved]
        private DraftSession session { get; set; } = null!;

        [Resolved]
        private TournamentGameBase? gameBase { get; set; }

        /// <summary>
        /// The draft state, shared with the random phase and mutated in place so that the bindings set up in
        /// <see cref="LoadComplete"/> stay attached across a reload.
        /// </summary>
        private DraftInfo draft => session.Draft;

        /// <summary>
        /// Whether the draft is currently being read back, by <see cref="initialiseDraft"/> or
        /// <see cref="reloadDraft"/>. Every change made while that happens comes from loading rather than from
        /// the operator, so none of them may trigger the reactive work (rebuilding the listing, reconciling the
        /// groups, writing the draft straight back out, reporting unsaved work).
        /// </summary>
        private bool restoring;

        private readonly Bindable<SettingsNote.Data?> status = new Bindable<SettingsNote.Data?>();

        /// <summary>
        /// The pool player the next placement will use, or <c>null</c> when nothing is selected.
        /// </summary>
        private readonly Bindable<DraftUserPanel?> selectedPanel = new Bindable<DraftUserPanel?>();

        private FillFlowContainer groupRowFlow = null!;
        private FillFlowContainer poolFlow = null!;
        private Container poolWarning = null!;

        /// <summary>
        /// What the pool placeholder currently says, so that a refresh which would say the same thing again is a no-op.
        /// </summary>
        private LocalisableString? poolWarningText;

        private Container warningContainer = null!;

        private readonly List<DraftTeamGroup> groups = new List<DraftTeamGroup>();
        private readonly Dictionary<TournamentUser, DraftUserPanel> panelByPlayer = new Dictionary<TournamentUser, DraftUserPanel>();

        /// <summary>
        /// The players this screen offers, i.e. everything past the front row.
        /// </summary>
        private IEnumerable<TournamentUser> poolCandidates => draft.Candidates.Where(p => p.Tier == DraftInfo.NO_TIER);

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new TourneyBackground(BackgroundType.Drawings)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new GridContainer
                {
                    Name = @"Content",
                    Position = new Vector2(content_padding),
                    Size = new Vector2(TournamentExtensions.STREAM_AREA_WIDTH - content_padding * 2, column_height),
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.Absolute, group_column_width),
                        new Dimension(GridSizeMode.Absolute, column_spacing),
                        new Dimension(GridSizeMode.Absolute, pool_column_width),
                    ],
                    Content = new Drawable[][]
                    {
                        [
                            createGroupColumn(),
                            Empty(),
                            createPoolColumn(),
                        ],
                    },
                },
                warningContainer = new Container
                {
                    Name = @"Warnings",
                    RelativeSizeAxes = Axes.Both,
                },
                new ControlPanel(needSaving: true, saveAction: saveDraftAndReport)
                {
                    Status =
                    {
                        BindTarget = status,
                    },
                    Children = new Drawable[]
                    {
                        new SectionHeader(ScreenStrings.Draft),
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new FormSliderBar<int>
                                {
                                    Name = @"Group count",
                                    Caption = DraftStrings.GroupCount,
                                    Current = draft.TeamCount,
                                },
                                new FormSliderBar<int>
                                {
                                    Name = @"Members per team",
                                    Caption = DraftStrings.MembersPerTeam,
                                    Current = draft.MembersPerTeam,
                                },
                            },
                        },
                        new TourneyButton
                        {
                            Name = @"Fetch players",
                            RelativeSizeAxes = Axes.X,
                            Text = DraftStrings.FetchPlayers,
                            Action = fetchPlayers,
                        },
                        new TourneyButton
                        {
                            Name = @"Write results",
                            RelativeSizeAxes = Axes.X,
                            Text = DraftStrings.WriteResults,
                            Action = writeResults,
                        },
                        new TourneyButton
                        {
                            Name = @"Reload",
                            RelativeSizeAxes = Axes.X,
                            Text = DraftStrings.Reload,
                            Action = reloadDraft,
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            draft.MembersPerTeam.BindValueChanged(_ => cleanupAfterModification());
            draft.TeamCount.BindValueChanged(_ => cleanupAfterModification());

            // The pool and the group boxes mirror the draft's collections, so nothing else has to remember to
            // redraw them. The group listing is driven by the group collection rather than by the group count,
            // because the count is only ever changed through ReconcileTeams which uses bulk collection
            // operations and therefore raises exactly one change.
            draft.Candidates.BindCollectionChanged(onCandidatesChanged);
            draft.Teams.BindCollectionChanged(onTeamsChanged);

            selectedPanel.BindValueChanged(onSelectionChanged);

            initialiseDraft();
        }

        public override void Show()
        {
            base.Show();

            // The bracket is where the groups' names come from, and it may have been read or edited since this
            // screen was last on. Nothing has to be rebuilt for that: the boxes follow their group's name.
            if (IsLoaded)
                session.SyncGroupNames(LadderInfo);
        }

        #region Layout

        private static SectionHeader createHeader(LocalisableString text) => new SectionHeader(text)
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Margin = new MarginPadding { Left = 5 },
        };

        private GridContainer createGroupColumn() => new GridContainer
        {
            Name = @"Draft groups",
            RelativeSizeAxes = Axes.Both,
            RowDimensions =
            [
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(),
            ],
            Content = new Drawable[][]
            {
                [createHeader(DraftStrings.GroupHeader)],
                [
                    // A large member limit makes the group boxes taller than the column, so it scrolls.
                    new OsuScrollContainer
                    {
                        Name = @"Draft groups scroll",
                        RelativeSizeAxes = Axes.Both,
                        ScrollbarVisible = false,
                        Child = groupRowFlow = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, column_spacing),
                        },
                    },
                ],
            },
        };

        private GridContainer createPoolColumn() => new GridContainer
        {
            Name = @"Draft pool",
            RelativeSizeAxes = Axes.Both,
            RowDimensions =
            [
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(),
            ],
            Content = new Drawable[][]
            {
                [
                    // TODO: Refactor
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 32,
                        ColumnDimensions =
                        [
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize),
                        ],
                        Content = new Drawable[][]
                        {
                            [
                                createHeader(DraftStrings.PoolHeader),
                            ],
                        },
                    },
                ],
                [
                    // The listing and the empty-state placeholder share the cell, so the placeholder sits where
                    // the player panels would have been. The pool is never filled just because the screen was
                    // opened — it takes the fetch button — so an empty one has to explain itself.
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            // The panels are a fixed size and the listing wraps them, so the column only has to
                            // be wide enough for the number of them a row should hold.
                            new OsuScrollContainer
                            {
                                Name = @"Draft pool scroll",
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarOverlapsContent = false,
                                Child = poolFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Full,
                                    Spacing = new Vector2(pool_spacing),
                                },
                            },
                            poolWarning = new Container
                            {
                                Name = @"Draft pool warning",
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    },
                ],
            },
        };

        #endregion

        #region Reactive plumbing

        /// <summary>
        /// Keep the pool in sync with <see cref="DraftInfo.Candidates"/> without rebuilding it from scratch:
        /// a panel holds an avatar, so recreating the whole list for every pick would restart every download
        /// and throw away the scroll position.
        /// </summary>
        private void onCandidatesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // A load replaces the collection in bulk and is followed by rebuildAll, which reads the result
            // outright; following each of the changes it is made of would only build the same listing again.
            if (restoring)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add when e.NewItems != null:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var player = (TournamentUser)e.NewItems[i]!;

                        if (player.Tier == DraftInfo.NO_TIER)
                            insertPoolPanel(player, i);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove when e.OldItems != null:
                    foreach (TournamentUser player in e.OldItems)
                        removePoolPanel(player);

                    break;

                // A clear arrives as a single Remove of every item and is handled above; this is only reached
                // for a wholesale replacement, which is cheap enough to redraw outright.
                default:
                    rebuildPool();
                    break;
            }

            // The placeholder depends on whether there is anything in the pool, so it is refreshed with it.
            refreshPoolWarning();
        }

        /// <summary>
        /// Rebuild the group boxes. <see cref="DraftInfo.ReconcileTeams"/> applies its changes in bulk, so this
        /// only runs once per group count change.
        /// </summary>
        private void onTeamsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // As for the pool: a load's changes are covered by the listing rebuild which follows it.
            if (restoring)
                return;

            rebuildGroups();

            // A group losing a member back to the pool also changes what the placeholder should say, because
            // whether anything has been drafted decides whether the empty pool is a finished draft or an
            // unfinished one.
            refreshPoolWarning();
        }

        private void onSelectionChanged(ValueChangedEvent<DraftUserPanel?> selection)
        {
            selection.OldValue?.Selected.Value = false;
            selection.NewValue?.Selected.Value = true;
        }

        private void insertPoolPanel(TournamentUser player, int index)
        {
            if (panelByPlayer.ContainsKey(player))
                return;

            var panel = new DraftUserPanel(player);
            panel.OnSelected += onPanelSelected;

            panelByPlayer.Add(player, panel);
            poolFlow.Insert(Math.Min(index, poolFlow.Count), panel);
        }

        private void removePoolPanel(TournamentUser player)
        {
            if (!panelByPlayer.Remove(player, out DraftUserPanel? panel))
                return;

            // Drop the selection before the panel goes away, so the selection handler never touches a
            // disposed drawable.
            if (selectedPanel.Value == panel)
                selectedPanel.Value = null;

            poolFlow.Remove(panel, true);
        }

        private void rebuildPool()
        {
            selectedPanel.Value = null;

            poolFlow.Clear();
            panelByPlayer.Clear();

            int index = 0;

            foreach (TournamentUser player in poolCandidates)
            {
                insertPoolPanel(player, index++);
            }
        }

        private void rebuildGroups()
        {
            groupRowFlow.Clear();
            groups.Clear();

            for (int row = 0; row * groups_per_row < draft.Teams.Count; row++)
            {
                var rowFlow = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(column_spacing),
                };

                for (int i = row * groups_per_row; i < Math.Min(draft.Teams.Count, (row + 1) * groups_per_row); i++)
                {
                    var group = new DraftTeamGroup(draft.Teams[i], draft.MembersPerTeam);

                    group.OnClicked += onGroupClicked;
                    group.OnMemberRemovalRequested += removeMember;

                    groups.Add(group);
                    rowFlow.Add(group);
                }

                groupRowFlow.Add(rowFlow);
            }
        }

        /// <summary>
        /// Build the whole listing from the draft as it stands: the group boxes, the pool, and the placeholder
        /// which speaks for an empty one.
        /// </summary>
        /// <remarks>
        /// The listings follow the draft's collections, but they are not built by the changes to them alone. The
        /// draft is shared with the random phase, and every screen is constructed as the client starts: whichever
        /// of the two screens comes up first is the one the draft is read for, and the other is handed a session
        /// which is already full, with no change ever reported for what is in it. Waiting for one would leave
        /// that screen with nothing but its headers until something else happened to touch the draft.
        /// </remarks>
        private void rebuildAll()
        {
            rebuildGroups();
            rebuildPool();
            refreshPoolWarning();
        }

        #endregion

        #region State

        /// <summary>
        /// Restore the persisted draft, or start a fresh one on top of the bracket, and make sure the groups
        /// match the settings.
        /// </summary>
        private void initialiseDraft()
        {
            restoring = true;

            try
            {
                session.EnsureLoaded(LadderInfo);
            }
            finally
            {
                restoring = false;
            }

            rebuildAll();
        }

        /// <summary>
        /// Write the draft to disk on demand. Bound to the control panel's "save" button, which persists the
        /// bracket alongside it.
        /// </summary>
        private void saveDraftAndReport()
        {
            session.Save();
            showStatus(DraftStrings.SavedInfo);
        }

        /// <summary>
        /// Report that the draft has changed since it was last saved.
        /// </summary>
        private void markDraftUnsaved()
        {
            if (!restoring)
                SaveChangesButton.TriggerEnableSaving();
        }

        #endregion

        #region Actions

        private void fetchPlayers()
        {
            switch (session.ReadPlayerIds(out List<int> ids))
            {
                case DraftSession.PlayerListReadResult.Missing:
                    showWarning(DraftStrings.PlayerListMissingWarning(DraftSession.PLAYER_LIST_FILENAME));
                    return;

                case DraftSession.PlayerListReadResult.Unreadable:
                    showWarning(DraftStrings.PlayerListUnreadableWarning(DraftSession.PLAYER_LIST_FILENAME));
                    return;

                case DraftSession.PlayerListReadResult.Empty:
                    showWarning(DraftStrings.PlayerListParseWarning(DraftSession.PLAYER_LIST_FILENAME));
                    return;
            }

            clearWarning();
            selectedPanel.Value = null;

            IReadOnlyList<TournamentUser> pool = session.FillPool(ids);

            session.Save();
            markDraftUnsaved();
            populateUsers(pool);
        }

        /// <summary>
        /// React to one of the draft's settings being changed from the control panel: bring the groups back in
        /// line with it, persist, and report the change.
        /// </summary>
        private void cleanupAfterModification()
        {
            if (restoring)
                return;

            draft.ReconcileTeams();

            // A raised group count materializes boxes the draft knows nothing about, so they are named here.
            session.SyncGroupNames(LadderInfo);

            session.Save();
            markDraftUnsaved();
        }

        private void reloadDraft()
        {
            clearWarning();
            clearStatus();

            restoring = true;

            try
            {
                session.Reload(LadderInfo);
            }
            finally
            {
                restoring = false;
            }

            rebuildAll();
        }

        private void writeResults()
        {
            int written = session.WriteToBracket(LadderInfo);

            session.Save();
            gameBase?.SaveChanges();

            showStatus(DraftStrings.WriteResultsInfo(written));
        }

        private void onGroupClicked(DraftTeamGroup group)
        {
            if (selectedPlayerFor(group) is not TournamentUser player)
                return;

            assignPlayer(group, player);
        }

        /// <summary>
        /// The player the next placement will use, or <c>null</c> after having reported that none is selected.
        /// </summary>
        private TournamentUser? selectedPlayerFor(DraftTeamGroup group)
        {
            DraftUserPanel? panel = selectedPanel.Value;

            if (panel != null)
                return panel.Player;

            showStatus(DraftStrings.NoSelectionWarning, true);
            return null;
        }

        /// <summary>
        /// Move <paramref name="player"/> out of the pool and into the next free member position of
        /// <paramref name="group"/>.
        /// </summary>
        private bool assignPlayer(DraftTeamGroup group, TournamentUser player)
        {
            if (!draft.Assign(player, group.Team))
            {
                showStatus(DraftStrings.GroupFullWarning, true);
                return false;
            }

            session.Save();
            markDraftUnsaved();

            showStatus(DraftStrings.AssignedInfo(displayNameFor(player), displayNameFor(group.Team)));
            return true;
        }

        private void removeMember(DraftTeamGroup group, TournamentUser player)
        {
            // The front row belongs to the random phase, and the group cells for it already refuse to ask.
            if (!draft.Release(player, group.Team))
                return;

            session.Save();
            markDraftUnsaved();

            showStatus(DraftStrings.RemovedInfo(displayNameFor(player), displayNameFor(group.Team)));
        }

        private void onPanelSelected(DraftUserPanel panel)
        {
            // Clicking the selected panel again clears the selection.
            selectedPanel.Value = selectedPanel.Value == panel ? null : panel;
        }

        /// <summary>
        /// Fetch the profile of every pool player which is still missing one.
        /// </summary>
        private void populateUsers(IEnumerable<TournamentUser> users)
        {
            foreach (TournamentUser user in users)
            {
                if (user.OnlineID <= 0 || !string.IsNullOrEmpty(user.Username))
                    continue;

                userCache.GetUserAsync(user.OnlineID).ContinueWith(t => Schedule(() =>
                {
                    var result = t.GetResultSafely();

                    if (result == null)
                        return;

                    var statistics = result.GetStatisticsFor(LadderInfo.Ruleset.Value);

                    user.Username = result.Username;
                    user.CountryCode = result.CountryCode;
                    user.CoverUrl = result.CoverUrl;
                    user.Rank = statistics?.GlobalRank;
                    user.PP = statistics?.PP;
                    user.ProfileFetchedAt = DateTimeOffset.UtcNow;

                    // TournamentUser is a plain serializable model with no bindables of its own, so the panel
                    // is told to re-read the profile it was built from.
                    if (panelByPlayer.TryGetValue(user, out DraftUserPanel? panel))
                        panel.Refresh();
                }));
            }
        }

        #endregion

        #region Display

        private static string displayNameFor(TournamentUser player) => string.IsNullOrEmpty(player.Username) ? $@"#{player.OnlineID}" : player.Username;

        private static string displayNameFor(TournamentTeam team) => string.IsNullOrEmpty(team.FullName.Value) ? team.Acronym.Value : team.FullName.Value;

        private void showWarning(LocalisableString text)
        {
            warningContainer.Clear();
            warningContainer.Add(new WarningBox(text)
            {
                Dismissable = true,
            });
        }

        private void clearWarning() => warningContainer.Clear();

        /// <summary>
        /// Say what an empty pool means, in the pool itself. A pool with players in it says nothing.
        /// </summary>
        private void refreshPoolWarning()
        {
            LocalisableString? text = null;

            if (!poolCandidates.Any())
            {
                if (draft.AssignedPlayers.Any(p => p.Tier == DraftInfo.NO_TIER))
                    text = DraftStrings.NoPlayerWarning;
                else if (draft.Candidates.Count != 0)
                    text = DraftStrings.FrontRowOnlyWarning(DraftSession.PLAYER_LIST_FILENAME);
                else
                    text = DraftStrings.PoolEmptyWarning(DraftSession.PLAYER_LIST_FILENAME);
            }

            // Handing a group back to the pool walks it player by player, so this is asked repeatedly with the
            // same answer; only a change is worth rebuilding the box for.
            if (text == poolWarningText)
                return;

            poolWarningText = text;

            poolWarning.Clear();

            if (text != null)
                poolWarning.Add(new WarningBox(text.Value));
        }

        private void showStatus(LocalisableString text, bool failure = false)
            => status.Value = new SettingsNote.Data(text, failure ? SettingsNote.Type.Warning : SettingsNote.Type.Informational);

        private void clearStatus() => status.Value = null;

        #endregion
    }
}
