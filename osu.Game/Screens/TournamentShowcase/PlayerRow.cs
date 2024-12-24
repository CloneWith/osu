// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class PlayerRow : FillFlowContainer
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly ShowcaseUser user;
        private readonly Bindable<string> playerId = new Bindable<string>();
        private readonly RulesetInfo? ruleset;

        private readonly Container userPanelContainer;

        public PlayerRow(ShowcaseTeam team, ShowcaseUser user, RulesetInfo? ruleset)
        {
            this.user = user;
            this.ruleset = ruleset;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Direction = FillDirection.Horizontal;

            Spacing = new Vector2(3);

            InternalChildren = new Drawable[]
            {
                new FormNumberBox
                {
                    Width = 0.3f,
                    Caption = @"Player ID",
                    Current = playerId,
                    AllowDecimals = false
                },
                userPanelContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.59f,
                    Child = new UserListPanel(user.ToAPIUser(), height: 60, cornerRadius: 5)
                },
                new GrayButton(FontAwesome.Solid.TimesCircle, new Vector2(20))
                {
                    Width = 32,
                    Height = 32,
                    TooltipText = @"Remove this user",
                    Action = () =>
                    {
                        Expire();
                        team.Members.Remove(user);
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            playerId.Value = user.OnlineID.ToString();
            playerId.BindValueChanged(id =>
            {
                bool idValid = int.TryParse(id.NewValue, out int newId) && newId >= 0;
                user.OnlineID = idValid ? newId : 0;

                if (id.NewValue != id.OldValue)
                    user.Username = string.Empty;

                if (!string.IsNullOrEmpty(user.Username))
                {
                    updatePanel();
                    return;
                }

                PopulatePlayer(ruleset, user, updatePanel, updatePanel);
            }, true);
        }

        public void PopulatePlayer(RulesetInfo? ruleset, ShowcaseUser user, Action? success = null, Action? failure = null, bool immediate = false)
        {
            var req = new GetUserRequest(user.OnlineID, ruleset);

            if (immediate)
            {
                api.Perform(req);
                populate();
            }
            else
            {
                req.Success += _ => { populate(); };
                req.Failure += _ =>
                {
                    user.OnlineID = 1;
                    failure?.Invoke();
                };

                api.Queue(req);
            }

            void populate()
            {
                var res = req.Response;

                if (res == null)
                    return;

                user.OnlineID = res.Id;

                user.Username = res.Username;
                user.CoverUrl = res.CoverUrl;
                user.CountryCode = res.CountryCode;
                user.Rank = res.Statistics?.GlobalRank;

                success?.Invoke();
            }
        }

        private void updatePanel() => Scheduler.AddOnce(() =>
        {
            userPanelContainer.Child = new UserListPanel(user.ToAPIUser(), height: 50, cornerRadius: 5);
        });
    }
}
