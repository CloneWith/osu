// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Screens.Board.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneModMapSection : TournamentTestScene
    {
        public TestSceneModMapSection()
        {
            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 500,
                Child = new ModMapSection(@"HR", @"Hard Rock")
                {
                    RelativeSizeAxes = Axes.X,
                    Width = 1,
                },
            });
        }
    }
}
