// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Platform
{
    public interface IAssociationManager
    {
        /// <summary>
        /// Installs or refreshes file and URI protocol associations for this client.
        /// </summary>
        void InstallAssociations();

        /// <summary>
        /// Updates associations with latest definitions.
        /// </summary>
        void UpdateAssociations();

        /// <summary>
        /// Remove all associations with the client.
        /// </summary>
        void UninstallAssociations();
    }
}
