// This file is originally created by GooGuTeam.

namespace osu.Game.Medals.Awarders
{
    /// <summary>
    /// Interface for awarding medals to users
    /// </summary>
    public interface IMedalAwarder
    {
        /// <summary>
        /// The ID of the medal to be awarded.
        /// </summary>
        int MedalId { get; }

        /// <summary>
        /// Whether the medal detection is enabled (disabled if the user already owns this medal).
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Checks if the criteria for awarding the medal are met
        /// </summary>
        bool CheckMedalCriteria(OsuGameBase game);
    }
}
