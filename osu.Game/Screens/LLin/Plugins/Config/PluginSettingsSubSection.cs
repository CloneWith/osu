// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.LLin.Plugins.Config
{
    public abstract class PluginSettingsSubSection : SettingsSubsection
    {
        private readonly LLinPlugin plugin;
        protected IPluginConfigManager ConfigManager;

        protected override LocalisableString Header => plugin.Name;

        protected PluginSettingsSubSection(LLinPlugin plugin)
        {
            this.plugin = plugin;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            ConfigManager = dependencies.Get<LLinPluginManager>().GetConfigManager(plugin);
            return dependencies;
        }
    }
}
