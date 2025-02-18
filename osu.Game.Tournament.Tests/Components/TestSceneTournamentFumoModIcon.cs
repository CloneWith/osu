// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Tournament.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneTournamentFumoModIcon : TournamentTestScene
    {
        private FillFlowContainer<TournamentFumoModIcon> fillFlow = new FillFlowContainer<TournamentFumoModIcon>();

        [Resolved]
        private TextureStore textureStore { get; set; } = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
        }

        [Test]
        public void TestIconState()
        {
            AddStep("collapse badges", () => fillFlow.Children.ForEach(icon => icon.Collapse()));
            AddStep("expand badges", () => fillFlow.Children.ForEach(icon => icon.Expand()));
        }

        [Test]
        public void TestIconColourChange()
        {
            AddStep("change main background colour", () => fillFlow.Children.First().MainBackgroundColour = Color4.SkyBlue);
            AddStep("change icon colour", () => fillFlow.Children.First().MainIconColour = Color4Extensions.FromHex("#535353"));
            AddStep("change badge background colour", () => fillFlow.Children.First().BadgeBackgroundColour = Color4Extensions.FromHex("#535353"));
            AddStep("change badge text colour", () => fillFlow.Children.First().BadgeTextColour = Color4.SkyBlue);
        }

        [Test]
        public void TestIconTextureChange()
        {
            AddStep("use texture as icon", () => fillFlow.Children.First().Texture = textureStore.Get("Icons/check-circle"));
            AddAssert("SpriteIcon component is hidden", () => fillFlow.Children.First().ChildrenOfType<SpriteIcon>().First().Alpha == 0);
            AddStep("use builtin icon type", () => fillFlow.Children.First().Icon = FontAwesome.Regular.CheckCircle);
            AddAssert("Sprite component is hidden", () => fillFlow.Children.First().ChildrenOfType<Sprite>().First(s => s.Name == "Sprite Icon").Alpha == 0);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(fillFlow = new FillFlowContainer<TournamentFumoModIcon>
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Full,
                Spacing = new Vector2(10),
                ChildrenEnumerable = ModIconColours.ColourMap.Select(kv => new TournamentFumoModIcon(kv.Key, "1"))
            });
        }
    }
}
