// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Components;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneChessPiece : TournamentTestScene
    {
        private readonly List<KeyValuePair<string, List<int>>> availableMods =
        [
            new KeyValuePair<string, List<int>>("NM", [1, 2, 3, 4, 5]),
            new KeyValuePair<string, List<int>>("HD", [1, 2, 3]),
            new KeyValuePair<string, List<int>>("HR", [1, 2, 3]),
            new KeyValuePair<string, List<int>>("DT", [1, 2, 3, 4]),
            new KeyValuePair<string, List<int>>("FM", [1, 2, 3, 4, 5]),
        ];

        [Test]
        public void TestDefaultConstructor()
        {
            AddStep("Add empty chess piece", () =>
            {
                Clear();
                Add(new FumoChessPiece
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            });
        }

        [Test]
        public void TestNormalModIcons()
        {
            AddStep("Add mod icons", addNormalModIcons);
        }

        private void addNormalModIcons()
        {
            FillFlowContainer iconFlow;
            Clear();

            Add(iconFlow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(15),
            });

            foreach (var modPair in availableMods)
            {
                iconFlow.Add(new FillFlowContainer
                {
                    Name = @$"{modPair.Key} mod icons",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(15),
                    ChildrenEnumerable = modPair.Value.Select(i => new FumoChessPiece(modPair.Key, i.ToString())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }),
                });
            }
        }
    }
}
