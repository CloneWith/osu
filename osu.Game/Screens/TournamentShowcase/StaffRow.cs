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
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class StaffRow : FillFlowContainer
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly ShowcaseStaff user;
        private readonly Bindable<string> playerId = new Bindable<string>();
        private readonly Bindable<string> role = new Bindable<string>();
        private readonly Container userPanelContainer;

        public StaffRow(ShowcaseStaff user, ShowcaseConfig config)
        {
            this.user = user;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Direction = FillDirection.Full;

            Spacing = new Vector2(5);

            Masking = true;
            CornerRadius = 10;

            InternalChildren = new Drawable[]
            {
                userPanelContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 1f,
                    Child = new UserListPanel(user.ToAPIUser())
                },
                new FormNumberBox
                {
                    Width = 0.44f,
                    Caption = @"Staff ID",
                    Current = playerId,
                    AllowDecimals = false,
                },
                new FormTextBox
                {
                    Width = 0.44f,
                    Caption = @"Staff role",
                    HintText = @"This would be shown on the staff list screen, in the staff card.",
                    Current = role,
                },
                new IconButton
                {
                    RelativeSizeAxes = Axes.X,
                    Width = 0.1f,
                    Icon = FontAwesome.Solid.TimesCircle,
                    TooltipText = @"Remove staff",
                    Action = () =>
                    {
                        Expire();
                        config.Staffs.Remove(user);
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            playerId.Value = user.OnlineID.ToString();
            role.Value = user.Role;

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

                PopulatePlayer(user, updatePanel, updatePanel);
            }, true);

            role.BindValueChanged(s => { user.Role = s.NewValue; }, true);
        }

        public void PopulatePlayer(ShowcaseUser user, Action? success = null, Action? failure = null, bool immediate = false)
        {
            var req = new GetUserRequest(user.OnlineID);

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
            userPanelContainer.Child = new UserListPanel(user.ToAPIUser());
        });
    }
}
