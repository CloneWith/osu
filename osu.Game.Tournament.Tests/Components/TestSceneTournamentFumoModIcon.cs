// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Components;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneTournamentFumoModIcon : TournamentTestScene
    {
        private FillFlowContainer<TournamentFumoModIcon> fillFlow = new FillFlowContainer<TournamentFumoModIcon>();

        [Test]
        public void TestIconState()
        {
            AddStep("collapse badges", () => fillFlow.Children.ForEach(icon => icon.Collapse()));
            AddStep("expand badges", () => fillFlow.Children.ForEach(icon => icon.Expand()));
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
