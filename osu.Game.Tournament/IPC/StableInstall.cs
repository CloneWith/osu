// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Logging;

namespace osu.Game.Tournament.IPC
{
    /// <summary>
    /// Inspects an osu! stable installation directory for the requirements of tournament usage.
    /// </summary>
    public static class StableInstall
    {
        /// <summary>
        /// The configuration file of an osu! stable installation.
        /// </summary>
        public const string CONFIG_FILENAME = @"osu!.cfg";

        /// <summary>
        /// The executable of an osu! stable installation.
        /// </summary>
        public const string EXECUTABLE_FILENAME = @"osu!.exe";

        /// <summary>
        /// The file which stable writes to when accepting the tournament mode prompt.
        /// </summary>
        public const string TOURNAMENT_ACCEPTED_FILENAME = @"tournament_accepted";

        /// <summary>
        /// The configuration file written by stable when it is launched in tournament mode.
        /// </summary>
        public const string TOURNAMENT_CONFIG_FILENAME = @"tournament.cfg";

        /// <summary>
        /// The file through which stable communicates the currently selected beatmap and mods.
        /// </summary>
        public const string IPC_FILENAME = @"ipc.txt";

        /// <summary>
        /// The file through which stable communicates the current tournament state.
        /// </summary>
        public const string IPC_STATE_FILENAME = @"ipc-state.txt";

        /// <summary>
        /// The file through which stable communicates the current team scores.
        /// </summary>
        public const string IPC_SCORES_FILENAME = @"ipc-scores.txt";

        /// <summary>
        /// The file through which stable communicates the current chat channel.
        /// </summary>
        public const string IPC_CHANNEL_FILENAME = @"ipc-channel.txt";

        /// <summary>
        /// The key in <see cref="CONFIG_FILENAME"/> which specifies the update stream of the installation.
        /// </summary>
        public const string RELEASE_STREAM_KEY = @"_ReleaseStream";

        /// <summary>
        /// The update stream which is required for tournament usage.
        /// </summary>
        public const string REQUIRED_RELEASE_STREAM = @"CuttingEdge";

        /// <summary>
        /// All files which are written to by stable to communicate with external tools.
        /// </summary>
        public static readonly string[] IPC_FILENAMES =
        {
            IPC_FILENAME,
            IPC_STATE_FILENAME,
            IPC_SCORES_FILENAME,
            IPC_CHANNEL_FILENAME,
        };

        /// <summary>
        /// Inspects the installation at the supplied path.
        /// </summary>
        /// <param name="path">The path to the candidate directory.</param>
        /// <returns>The state of the installation at the supplied path.</returns>
        public static StableInstallStatus Inspect(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return default;

            string configPath = Path.Combine(path, CONFIG_FILENAME);

            if (!File.Exists(configPath) || !File.Exists(Path.Combine(path, EXECUTABLE_FILENAME)))
                return default;

            string? releaseStream = readReleaseStream(configPath);

            return new StableInstallStatus(
                true,
                string.Equals(releaseStream, REQUIRED_RELEASE_STREAM, StringComparison.OrdinalIgnoreCase),
                File.Exists(Path.Combine(path, TOURNAMENT_CONFIG_FILENAME))
                && File.Exists(Path.Combine(path, TOURNAMENT_ACCEPTED_FILENAME))
                && File.Exists(Path.Combine(path, IPC_FILENAME)),
                releaseStream);
        }

        /// <summary>
        /// Reads the update stream from the configuration file of an installation.
        /// </summary>
        /// <remarks>
        /// The configuration file is an ini file, but the section a key belongs to is irrelevant here,
        /// as <see cref="RELEASE_STREAM_KEY"/> is unique within it.
        /// </remarks>
        private static string? readReleaseStream(string configPath)
        {
            try
            {
                foreach (string line in File.ReadLines(configPath))
                {
                    int separator = line.IndexOf('=');

                    if (separator < 0)
                        continue;

                    if (!line[..separator].Trim().Equals(RELEASE_STREAM_KEY, StringComparison.OrdinalIgnoreCase))
                        continue;

                    return line[(separator + 1)..].Trim();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to read the update stream from {configPath}");
            }

            return null;
        }
    }

    /// <summary>
    /// The state of an osu! stable installation as required for tournament usage.
    /// </summary>
    public readonly record struct StableInstallStatus(bool IsInstall, bool IsCuttingEdge, bool IsTournamentReady, string? ReleaseStream)
    {
        /// <summary>
        /// Whether the installation can be used for tournament usage without any further setup.
        /// </summary>
        public bool IsUsable => IsInstall && IsCuttingEdge && IsTournamentReady;
    }
}
