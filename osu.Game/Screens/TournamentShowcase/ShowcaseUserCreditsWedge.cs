// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Models;
using osu.Game.Screens.SelectV2;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseUserCreditsWedge : CompositeDrawable
    {
        private const int user_entry_height = 20;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        private FillFlowContainer userFlow = null!;

        private CancellationTokenSource? cancellationSource;
        private readonly List<int> userIds = new List<int>();

        public ShowcaseUserCreditsWedge(ShowcaseBeatmap beatmap)
        {
            AutoSizeAxes = Axes.Both;

            if (string.IsNullOrWhiteSpace(beatmap.CreditUserIds.Value))
                return;

            List<string> userValues = beatmap.CreditUserIds.Value.Split(',').Where(s => !string.IsNullOrWhiteSpace(s))
                                             .ToList();

            foreach (string v in userValues)
            {
                if (int.TryParse(v, out int userId))
                    userIds.Add(userId);
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Alpha = 0;
            AlwaysPresent = true;
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new WedgeBackground(),
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
                    Children = new Drawable[]
                    {
                        new TruncatingSpriteText
                        {
                            Shadow = true,
                            Font = OsuFont.Style.Heading2,
                            Text = "Credits:",
                        },
                        userFlow = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(3f),
                            Padding = new MarginPadding(5),
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            cancellationSource?.Cancel();
            cancellationSource = new CancellationTokenSource();

            Task.Run(populateInfo).ContinueWith(_ => Scheduler.Add(() => this.FadeIn(1000, Easing.OutQuint)));
        }

        private async Task populateInfo()
        {
            try
            {
                if (userIds.Count == 0)
                    return;

                var foundUsers = await userLookupCache.GetUsersAsync(userIds.ToArray()).ConfigureAwait(false);

                foundUsers.Where(u => u != null).ForEach(user => Scheduler.Add(() => userFlow.Add(new FillFlowContainer
                {
                    Name = "User line",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.X,
                    Height = user_entry_height,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "User avatar",
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(user_entry_height),
                            Masking = true,
                            CornerRadius = 5,
                            Child = new UpdateableAvatar(user)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = user!.Username,
                            Font = OsuFont.Style.Body,
                        },
                    },
                })));
            }
            catch (Exception e)
            {
                Logger.Log($"Error while populating showcase item {e}");
            }
        }
    }
}
