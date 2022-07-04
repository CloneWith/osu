// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.LLin.SideBar.PluginsPage;

namespace osu.Game.Screens.LLin.SideBar
{
    internal class SidebarPluginsPage : OsuScrollContainer, ISidebarContent
    {
        public string Title => "Plugins";
        public IconUsage Icon => FontAwesome.Solid.Boxes;

        [BackgroundDependencyLoader]
        private void load()
        {
            ScrollbarVisible = false;
            RelativeSizeAxes = Axes.Both;

            Add(new PluginsSection());
        }
    }
}
