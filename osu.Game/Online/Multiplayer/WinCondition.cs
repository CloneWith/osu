// This file is originally created by GooGuTeam.

using System.ComponentModel;

namespace osu.Game.Online.Multiplayer
{
    public enum WinCondition
    {
        [Description("Score")]
        Score,

        [Description("Accuracy")]
        Accuracy,

        [Description("Combo")]
        Combo,

        [Description("PP")]
        Pp
    }
}
