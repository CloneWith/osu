// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.Versioning;
using osu.Game.Platform;

namespace osu.Desktop.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsAssociationManagerAdapter : IAssociationManager
    {
        public void InstallAssociations() => WindowsAssociationManager.InstallAssociations();

        public void UpdateAssociations() => WindowsAssociationManager.UpdateAssociations();

        public void UninstallAssociations() => WindowsAssociationManager.UninstallAssociations();
    }
}
