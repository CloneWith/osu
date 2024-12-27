// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Models;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseBeatmapInfoArea : FillFlowContainer
    {
        public readonly Bindable<ShowcaseBeatmap> Beatmap = new Bindable<ShowcaseBeatmap>();

        private ShowcaseUser selector = new ShowcaseUser();

        private readonly OsuTextFlowContainer areaContainer;
        private readonly OsuTextFlowContainer commentContainer;

        private readonly OsuSpriteText selectText;
        private readonly UpdateableAvatar selectorAvatar;
        private readonly OsuSpriteText selectorName;

        private readonly FillFlowContainer commentHeader;

        private void prepareData()
        {
            static void formatComment(SpriteText t) => t.Font = OsuFont.TorusAlternate.With(size: 22, weight: FontWeight.SemiBold);

            areaContainer.Clear();

            if (!string.IsNullOrEmpty(Beatmap.Value.BeatmapArea.Value))
            {
                areaContainer.AddIcon(FontAwesome.Solid.Star);
                areaContainer.AddParagraph(Beatmap.Value.BeatmapArea.Value, formatComment);
            }

            commentContainer.Clear();
            string[] commentText = Beatmap.Value.BeatmapComment.Value?.Split('\n') ?? Array.Empty<string>();

            if (commentText.Length > 0)
            {
                commentText.ForEach(p => commentContainer.AddParagraph(p, formatComment));
                commentHeader.FadeIn(500, Easing.OutQuint);
                commentContainer.FadeIn(800, Easing.OutQuint);
            }
            else
            {
                commentHeader.FadeOut(500, Easing.OutQuint);
                commentContainer.FadeOut(800, Easing.OutQuint);
            }

            selector = Beatmap.Value.Selector.Value;

            if (selector == null)
            {
                selectText.FadeOut(500, Easing.OutQuint);
                selectorAvatar.FadeOut(500, Easing.OutQuint);
                selectorName.FadeOut(500, Easing.OutQuint);
            }
            else
            {
                selectorAvatar.User = selector.ToAPIUser();
                selectorName.Text = selector.Username;

                selectText.FadeIn(500, Easing.OutQuint);
                selectorAvatar.FadeIn(500, Easing.OutQuint);
                selectorName.FadeIn(500, Easing.OutQuint);
            }
        }

        public ShowcaseBeatmapInfoArea()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            RelativeSizeAxes = Axes.Both;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(5);

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        selectorAvatar = new UpdateableAvatar(isInteractive: false)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(48),
                            CornerRadius = 24,
                            Masking = true
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                selectText = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = @"Selected by",
                                    Font = OsuFont.TorusAlternate.With(size: 24)
                                },
                                selectorName = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = @"A random player",
                                    Colour = new OsuColour().Sky,
                                    Font = OsuFont.TorusAlternate.With(size: 30, weight: FontWeight.SemiBold)
                                },
                            }
                        },
                    }
                },
                areaContainer = new OsuTextFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2)
                },
                commentHeader = new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Icon = FontAwesome.Solid.CommentAlt,
                            Size = new Vector2(16),
                            Colour = new OsuColour().Sky
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = @"Comment",
                            Font = OsuFont.TorusAlternate.With(size: 20, weight: FontWeight.SemiBold)
                        }
                    }
                },
                commentContainer = new OsuTextFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Left = 20 }
                },
            };

            if (Beatmap.Value != null)
            {
                prepareData();
            }

            Beatmap.BindValueChanged(_ => prepareData());
        }

        public void Animate()
        {
            //
        }
    }
}
