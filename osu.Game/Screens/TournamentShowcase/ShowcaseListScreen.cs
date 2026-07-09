using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Screens.TournamentShowcase
{
    public partial class ShowcaseListScreen : OsuScreen, ISubScreenWithTitle
    {
        public string ShortTitle => @"Profiles";

        public LocalisableString LocalisableTitle => TournamentShowcaseStrings.Profiles;

        public override bool ShowFooter => true;
    }
}
