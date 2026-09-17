// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Tournament.Caching;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Localisation;
using osu.Game.Tournament.Localisation.Screens;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.RandomPick.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.RandomPick
{
    public partial class RandomPickScreen : TournamentScreen
    {
        [Resolved]
        private DraftSession session { get; set; } = null!;

        [Resolved]
        private TournamentUserCache userCache { get; set; } = null!;

        [Resolved]
        private TournamentGameBase? gameBase { get; set; }

        private DraftInfo draft => session.Draft;

        /// <summary>
        /// The tier being drawn from. Every draw comes out of this one until it has been placed for every team
        /// it can be, at which point the screen moves on by itself.
        /// </summary>
        private int currentTier = 1;

        /// <summary>
        /// The box the next draw has been pointed at, or <c>null</c> when it goes wherever there is room.
        /// </summary>
        private PickerGroup? selectedGroup;

        /// <summary>
        /// Whether the persisted draft is being read. Changes made while that happens are not the operator's, so
        /// they must not report unsaved work.
        /// </summary>
        private bool restoring;

        private ScrollingPlayerContainer players = null!;
        private PickerGroupContainer groupsContainer = null!;

        private TournamentSpriteText tierText = null!;
        private TournamentSpriteText landedNameText = null!;
        private TournamentSpriteText statusText = null!;

        private Container warningContainer = null!;
        private TourneyButton tierButton = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new TourneyBackground(BackgroundType.Drawings)
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                // The layer the group boxes are laid out on. It takes the full height so that its two flows can
                // anchor to the top and the bottom of the screen.
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    RelativeSizeAxes = Axes.Both,

                    Padding = new MarginPadding
                    {
                        Top = 30f,
                        Bottom = 30f
                    },
                    Child = groupsContainer = new PickerGroupContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                players = new ScrollingPlayerContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.X,
                },
                landedNameText = new TournamentSpriteText
                {
                    Name = @"Landed player",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Position = new Vector2(0, 45f),
                    Colour = OsuColour.Gray(0.95f),
                    Alpha = 0,
                    Font = OsuFont.Torus.With(weight: FontWeight.Light, size: 42),
                },
                tierText = new TournamentSpriteText
                {
                    Name = @"Tier header",
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    Position = new Vector2(10, 6),

                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 20),
                },
                statusText = new TournamentSpriteText
                {
                    Name = @"Status",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,

                    Position = new Vector2(10, -6),

                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 16),
                },
                warningContainer = new Container
                {
                    Name = @"Warnings",
                    RelativeSizeAxes = Axes.Both,
                },
                new ControlPanel(needSaving: true, saveAction: saveAndReport)
                {
                    Children = new Drawable[]
                    {
                        new SectionHeader(ScreenStrings.RandomPick),
                        new TourneyButton
                        {
                            Name = @"Fetch players",
                            RelativeSizeAxes = Axes.X,
                            Text = DraftStrings.FetchPlayers,
                            Action = fetchPlayers,
                        },
                        new TourneyButton
                        {
                            Name = @"Start draw",
                            RelativeSizeAxes = Axes.X,
                            Text = RandomPickStrings.StartDraw,
                            Action = startDraw,
                        },
                        new TourneyButton
                        {
                            Name = @"Stop draw",
                            RelativeSizeAxes = Axes.X,
                            Text = RandomPickStrings.StopDraw,
                            Action = () => players.StopScrolling(),
                        },
                        tierButton = new TourneyButton
                        {
                            Name = @"Switch tier",
                            RelativeSizeAxes = Axes.X,
                            Action = switchTier,
                        },
                        new TourneyButton
                        {
                            Name = @"Write to bracket",
                            RelativeSizeAxes = Axes.X,
                            Text = RandomPickStrings.WriteToBracket,
                            Action = writeToBracket,
                        },
                        new ControlPanel.Spacer(),
                        new TourneyButton
                        {
                            Name = @"Reset",
                            RelativeSizeAxes = Axes.X,
                            Text = BaseStrings.Reset,
                            Action = reset,
                        },
                    },
                },
            };

            players.OnSelected += onPlayerDrawn;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // The pool and the group boxes mirror the draft's collections, so nothing else has to remember to redraw them.
            draft.Teams.BindCollectionChanged((_, _) => rebuildGroups());

            restoring = true;

            try
            {
                session.EnsureLoaded(LadderInfo);
            }
            finally
            {
                restoring = false;
            }

            refreshAll();
        }

        public override void Show()
        {
            base.Show();

            // The other half of the selection edits the same session, so what is on screen is rebuilt whenever
            // this screen comes back, unless a draw is in progress, which must not be interrupted.
            if (IsLoaded && !players.Scrolling)
                refreshAll();
        }

        public override void Hide()
        {
            if (IsLoaded)
                players.CancelScrolling();

            base.Hide();
        }

        #region Layout

        /// <summary>
        /// Rebuild the boxes, one per group.
        /// </summary>
        /// <remarks>
        /// They are new objects every time, so a selection cannot survive a rebuild: the box it referred to is
        /// disposed along with the rest, and it is forgotten rather than pointed back at a dead drawable.
        /// </remarks>
        private void rebuildGroups()
        {
            selectedGroup = null;

            groupsContainer.SetGroups(draft.Teams);

            foreach (PickerGroup group in groupsContainer.Groups)
                group.OnClicked += onGroupClicked;
        }

        /// <summary>
        /// Bring every part of the screen back in line with the draft.
        /// </summary>
        private void refreshAll()
        {
            rebuildGroups();
            refreshTier();
        }

        /// <summary>
        /// Point the screen at the first tier there is still something to do in, and show it.
        /// </summary>
        private void refreshTier()
        {
            for (int i = 0; i < DraftInfo.FRONT_ROW_TIERS; i++)
            {
                int candidate = (currentTier - 1 + i) % DraftInfo.FRONT_ROW_TIERS + 1;

                if (!canDrawFrom(candidate))
                    continue;

                currentTier = candidate;
                break;
            }

            applyTier();
        }

        /// <summary>
        /// Show <see cref="currentTier"/>: its name on the header and on the switch button, and its players in
        /// the strip.
        /// </summary>
        private void applyTier()
        {
            tierText.Text = RandomPickStrings.PoolHeader(currentTier);
            tierButton.Text = RandomPickStrings.TierSwitch(currentTier, DraftInfo.FRONT_ROW_TIERS);

            players.SetPlayers(playersOfTier(currentTier));
        }

        private IEnumerable<TournamentUser> playersOfTier(int tier) => draft.Candidates.Where(p => p.Tier == tier);

        #endregion

        #region Selection

        /// <summary>
        /// Point the next draw at a box, or hand the choice back to the other boxes by clicking the pointed-at
        /// one a second time.
        /// </summary>
        private void onGroupClicked(PickerGroup group)
        {
            setSelection(selectedGroup == group ? null : group);

            if (selectedGroup != null)
                showStatus(RandomPickStrings.SelectInfo(displayNameFor(selectedGroup.Team)));
            else
                clearStatus();
        }

        /// <summary>
        /// Show <paramref name="group"/> as the one the next draw should use.
        /// </summary>
        private void setSelection(PickerGroup? group)
        {
            if (selectedGroup == group)
                return;

            selectedGroup?.Selected = false;
            selectedGroup = group;
            selectedGroup?.Selected = true;
        }

        /// <summary>
        /// Say why a draw from <paramref name="tier"/> would lead nowhere. A pointed-at box with no room for it
        /// is a different matter from every box being full, because the fix is to point somewhere else.
        /// </summary>
        private void reportNoTarget(int tier)
        {
            if (selectedGroup == null)
            {
                showStatus(RandomPickStrings.NoTierSlotWarning(tier), true);
                return;
            }

            selectedGroup.FlashInputError();
            showStatus(RandomPickStrings.SelectedTierFilledWarning(displayNameFor(selectedGroup.Team), tier), true);
        }

        #endregion

        #region Drawing

        /// <summary>
        /// The box a draw from <paramref name="tier"/> goes into: the one the operator has pointed at, if there
        /// is one, and otherwise the first box which still has room in that tier.
        /// </summary>
        /// <returns><c>null</c> when the draw would lead nowhere.</returns>
        private PickerGroup? findGroupFor(int tier)
        {
            if (selectedGroup != null)
                return draft.HasFreeTierSlot(selectedGroup.Team, tier) ? selectedGroup : null;

            return groupsContainer.Groups.FirstOrDefault(group => draft.HasFreeTierSlot(group.Team, tier));
        }

        /// <summary>
        /// Whether a draw from <paramref name="tier"/> would lead anywhere: there has to be a player left in it
        /// as well as a team with the position free.
        /// </summary>
        private bool canDrawFrom(int tier) => playersOfTier(tier).Any() && findGroupFor(tier) != null;

        private void startDraw()
        {
            if (players.Scrolling)
                return;

            if (!playersOfTier(currentTier).Any())
            {
                showStatus(RandomPickStrings.NoTierPlayerWarning(currentTier), true);
                return;
            }

            if (findGroupFor(currentTier) == null)
            {
                reportNoTarget(currentTier);
                return;
            }

            clearStatus();
            players.StartScrolling();
        }

        private void switchTier()
        {
            if (players.Scrolling)
                return;

            currentTier = currentTier % DraftInfo.FRONT_ROW_TIERS + 1;

            clearStatus();

            // The operator asked for this tier in particular, so it is shown even when a draw from it could not
            // lead anywhere — reporting that is what the draw button is for.
            applyTier();
        }

        /// <summary>
        /// Place the player the strip landed on into the box the draw was aimed at.
        /// </summary>
        /// <remarks>
        /// The player is taken out of the strip only once they have been placed, so a draw which leads nowhere
        /// leaves a strip that can simply be stopped again.
        /// </remarks>
        private void onPlayerDrawn(TournamentUser player)
        {
            PickerGroup? group = findGroupFor(player.Tier);

            if (group == null || !draft.Assign(player, group.Team))
            {
                reportNoTarget(player.Tier);
                return;
            }

            // The operator's choice has been spent on the draw it was made for. The box the player went into
            // announces itself on its own, because it follows the roster like every other part of the screen.
            setSelection(null);

            players.RemovePlayer(player);

            landedNameText.Text = displayNameFor(player);
            landedNameText.FadeIn(200);

            session.Save();
            markUnsaved();

            showStatus(RandomPickStrings.DrawnInfo(displayNameFor(player), displayNameFor(group.Team)));

            advanceTierIfExhausted();
        }

        /// <summary>
        /// Move on to the next tier which can still be drawn from once the current one is done, and say so when
        /// there is no such tier left.
        /// </summary>
        private void advanceTierIfExhausted()
        {
            refreshTier();

            if (canDrawFrom(currentTier))
                return;

            // Nothing has anywhere left to go, which is either the end of the front row or every team having run
            // out of room for the one still being drawn from.
            bool frontRowLeft = draft.Candidates.Any(p => p.Tier != DraftInfo.NO_TIER);

            showStatus(frontRowLeft ? RandomPickStrings.NoTierSlotWarning(currentTier) : RandomPickStrings.AllDrawnWarning, true);
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

            IReadOnlyList<TournamentUser> pool = session.FillPool(ids);

            session.Save();
            markUnsaved();

            populateUsers(pool);

            // The list decides the tiers, so reading it may leave the current one with nothing to draw from.
            refreshTier();
        }

        private void writeToBracket()
        {
            int written = session.WriteToBracket(LadderInfo);

            session.Save();
            gameBase?.SaveChanges();

            rebuildGroups();
            showStatus(RandomPickStrings.WrittenInfo(written));
        }

        private void saveAndReport()
        {
            session.Save();
            showStatus(RandomPickStrings.SavedInfo);
        }

        /// <summary>
        /// Empty every box and start the draw over, the way the drawings screen's reset starts its own over.
        /// </summary>
        private void reset()
        {
            if (players.Scrolling)
                return;

            clearWarning();
            clearStatus();

            draft.ResetAssignments();

            session.Save();
            markUnsaved();

            // Rebuilding the boxes is also what takes the selection away.
            refreshAll();

            showStatus(RandomPickStrings.ResetInfo);
        }

        /// <summary>
        /// Report that the draft has changed since it was last saved.
        /// </summary>
        private void markUnsaved()
        {
            if (!restoring)
                SaveChangesButton.TriggerEnableSaving();
        }

        /// <summary>
        /// Fetch the profile of every pool player which is still missing one.
        /// </summary>
        /// <remarks>
        /// The lookups go through the shared user cache, so a player which is already in the bracket (and so has
        /// already been fetched) costs nothing beyond a dictionary read.
        /// </remarks>
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

                    refreshGroupContaining(user);
                }));
            }
        }

        /// <summary>
        /// Rebuild the box of the team <paramref name="player"/> is in, if any. <see cref="TournamentUser"/> is a
        /// plain serializable model with no bindables of its own, so a box which was built before the profile
        /// arrived has to be told to read it again.
        /// </summary>
        private void refreshGroupContaining(TournamentUser player)
        {
            foreach (PickerGroup group in groupsContainer.Groups)
            {
                if (group.Team.Players.Contains(player))
                {
                    group.Refresh();
                    return;
                }
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

        private void showStatus(LocalisableString text, bool failure = false)
        {
            statusText.Text = text;
            statusText.Colour = failure ? FumoColours.FlandreRed.Lighter : FumoColours.LightGreen.Lighter;
        }

        private void clearStatus() => statusText.Text = string.Empty;

        #endregion
    }
}
