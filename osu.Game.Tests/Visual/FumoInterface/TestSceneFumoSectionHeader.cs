// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceFumo;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.FumoInterface
{
    public partial class TestSceneFumoSectionHeader : OsuTestScene
    {
        private readonly FumoSectionHeader sectionHeader;

        public TestSceneFumoSectionHeader()
        {
            Container mainContainer;

            Child = mainContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300),
                Masking = true,
                Children = new Drawable[]
                {
                    sectionHeader = new FumoSectionHeader
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = OsuIcon.Chat,
                        Text = "Test Header",
                    },
                },
            };

            AddSliderStep("Viewport width", 300, 800, 300, w => mainContainer.Width = w);
        }

        [Test]
        public void TestColourChanges()
        {
            AddStep("Set colour to white", () => sectionHeader.AccentColour = Color4.White);

            AddStep("Set colour to red", () => sectionHeader.AccentColour = FumoColours.FlandreRed.Regular);
            AddStep("Set colour to blue", () => sectionHeader.AccentColour = FumoColours.SeaBlue.Regular);
            AddStep("Set colour to green", () => sectionHeader.AccentColour = FumoColours.LightGreen.Regular);
            AddStep("Set colour to yellow", () => sectionHeader.AccentColour = FumoColours.SunshineYellow.Regular);
            AddStep("Set colour to purple", () => sectionHeader.AccentColour = FumoColours.DeepPurple.Regular);
        }

        [Test]
        public void TestIconChanges()
        {
            AddSliderStep("Icon size", 16, 48, 20, s => sectionHeader.IconSize = s);

            AddStep("Set FontAwesome icon", () => sectionHeader.Icon = FontAwesome.Regular.Clipboard);
            AddStep("Set icon to empty", () => sectionHeader.Icon = new IconUsage());
            AddStep("Set OsuIcon icon", () => sectionHeader.Icon = OsuIcon.Tournament);
        }

        [Test]
        public void TestTextChanges()
        {
            AddSliderStep("Font size", 16, 48, 20, s => sectionHeader.Font = sectionHeader.Font.With(size: s));
            AddStep("Set normal font weight", () => sectionHeader.Font = sectionHeader.Font.With(weight: FontWeight.Regular));
            AddStep("Set semibold font weight", () => sectionHeader.Font = sectionHeader.Font.With(weight: FontWeight.SemiBold));
            AddStep("Set bold font weight", () => sectionHeader.Font = sectionHeader.Font.With(weight: FontWeight.Bold));

            AddStep("Set text to empty", () => sectionHeader.Text = string.Empty);
            AddStep("Set normal text", () => sectionHeader.Text = @"NM Maps");
            AddStep("Set CJK text", () => sectionHeader.Text = @"烤翅いく");
            AddStep("Set long text", () => sectionHeader.Text = @"复杂的问题也需要优雅的解决方案(｡･∀･)ﾉﾞ");
        }
    }
}
