// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseErrorScreen : OsuScreen
    {
        public override string Title => @"Error!";

        private readonly Exception? exception;
        private readonly Action? backAction;

        private OsuTextFlowContainer textFlow = null!;

        public ShowcaseErrorScreen(Exception? exception = null, Action? backAction = null)
        {
            this.exception = exception;
            this.backAction = backAction;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    Height = 500,
                    Masking = true,
                    CornerRadius = 10,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.5f),
                        Radius = 10,
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#262626"),
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Icon = FontAwesome.Solid.Bomb,
                                    Size = new Vector2(50),
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Font = OsuFont.Style.Title,
                                    Text = TournamentShowcaseStrings.ErrorScreenTitle,
                                },
                                textFlow = new OsuTextFlowContainer
                                {
                                    AlwaysPresent = true,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Padding = new MarginPadding { Top = 20, Horizontal = 50 },
                                    TextAnchor = Anchor.TopCentre,
                                    FirstLineIndent = 0.5f,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                },
                                new ClickTwiceButton
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Action = backAction,
                                    IdleIcon = FontAwesome.Regular.ArrowAltCircleLeft,
                                    Text = CommonStrings.Exit,
                                },
                            },
                        },
                    },
                },
            };

            textFlow.AddText(TournamentShowcaseStrings.ErrorScreenFirst);
            textFlow.NewLine();
            textFlow.AddText(TournamentShowcaseStrings.ErrorScreenSecond);

            if (exception != null)
            {
                textFlow.NewParagraph();
                textFlow.AddText(TournamentShowcaseStrings.ErrorScreenException);
                textFlow.NewLine();
                textFlow.AddText(exception.Message);
                textFlow.NewParagraph();
                textFlow.AddText(TournamentShowcaseStrings.ErrorScreenExceptionDetail);
            }

            textFlow.NewParagraph();
            textFlow.AddText("");
        }
    }
}
