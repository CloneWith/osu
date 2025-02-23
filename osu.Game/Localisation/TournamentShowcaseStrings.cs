// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class TournamentShowcaseStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.TournamentShowcase";

        /// <summary>
        /// "showcase"
        /// </summary>
        public static LocalisableString ShowcaseButtonText => new TranslatableString(getKey(@"showcase_button_text"), @"showcase");

        /// <summary>
        /// "Tournament Information"
        /// </summary>
        public static LocalisableString TournamentInfoHeader => new TranslatableString(getKey(@"tournament_info_header"), @"Tournament Information");

        /// <summary>
        /// "Showcase Settings"
        /// </summary>
        public static LocalisableString ShowcaseSettingsHeader => new TranslatableString(getKey(@"showcase_settings_header"), @"Showcase Settings");

        /// <summary>
        /// "Beatmap Queue"
        /// </summary>
        public static LocalisableString BeatmapQueueHeader => new TranslatableString(getKey(@"beatmap_queue_header"), @"Beatmap Queue");

        /// <summary>
        /// "Intro Beatmap"
        /// </summary>
        public static LocalisableString IntroBeatmapHeader => new TranslatableString(getKey(@"intro_beatmap_header"), @"Intro Beatmap");

        /// <summary>
        /// "Current Profile"
        /// </summary>
        public static LocalisableString CurrentProfile => new TranslatableString(getKey(@"current_profile"), @"Current Profile");

        /// <summary>
        /// "The profile to be loaded and edited. You can find the files under the "showcase" directory of the data path."
        /// </summary>
        public static LocalisableString CurrentProfileDescription => new TranslatableString(getKey(@"current_profile_description"), @"The profile to be loaded and edited. You can find the files under the ""showcase"" directory of the data path.");

        /// <summary>
        /// "Default Ruleset"
        /// </summary>
        public static LocalisableString DefaultRuleset => new TranslatableString(getKey(@"default_ruleset"), @"Default Ruleset");

        /// <summary>
        /// "The default and fallback ruleset for showcase beatmaps."
        /// </summary>
        public static LocalisableString DefaultRulesetDescription => new TranslatableString(getKey(@"default_ruleset_description"), @"The default and fallback ruleset for showcase beatmaps.");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString TournamentName => new TranslatableString(getKey(@"tournament_name"), @"Name");

        /// <summary>
        /// "Tournament series name (e.g. osu! World Cup)"
        /// </summary>
        public static LocalisableString TournamentNamePlaceholder => new TranslatableString(getKey(@"tournament_name_placeholder"), @"Tournament series name (e.g. osu! World Cup)");

        /// <summary>
        /// "This would be shown as the subtitle at the intro screen."
        /// </summary>
        public static LocalisableString TournamentNameDescription => new TranslatableString(getKey(@"tournament_name_description"), @"This would be shown as the subtitle at the intro screen.");

        /// <summary>
        /// "Round Name"
        /// </summary>
        public static LocalisableString TournamentRound => new TranslatableString(getKey(@"tournament_round"), @"Round Name");

        /// <summary>
        /// "Tournament round (e.g. Semifinals)"
        /// </summary>
        public static LocalisableString TournamentRoundPlaceholder => new TranslatableString(getKey(@"tournament_round_placeholder"), @"Tournament round (e.g. Semifinals)");

        /// <summary>
        /// "This would be shown as the main title at the intro screen."
        /// </summary>
        public static LocalisableString TournamentRoundDescription => new TranslatableString(getKey(@"tournament_round_description"), @"This would be shown as the main title at the intro screen.");

        /// <summary>
        /// "Date and Time"
        /// </summary>
        public static LocalisableString DateAndTime => new TranslatableString(getKey(@"date_and_time"), @"Date and Time");

        /// <summary>
        /// "This would stay the same in the showcase. So use your own preferred format!"
        /// </summary>
        public static LocalisableString DateAndTimeDescription => new TranslatableString(getKey(@"date_and_time_description"), @"This would stay the same in the showcase. So use your own preferred format!");

        /// <summary>
        /// "In fact you can write anything here.\nThis would be shown below the main title at the intro screen."
        /// </summary>
        public static LocalisableString IntroCommentDescription => new TranslatableString(getKey(@"intro_comment_description"), @"In fact you can write anything here.\nThis would be shown below the main title at the intro screen.");

        /// <summary>
        /// "Interface Layout"
        /// </summary>
        public static LocalisableString InterfaceLayout => new TranslatableString(getKey(@"interface_layout"), @"Interface Layout");

        /// <summary>
        /// "The layout of the showcases screen."
        /// </summary>
        public static LocalisableString InterfaceLayoutDescription => new TranslatableString(getKey(@"interface_layout_description"), @"The layout of the showcases screen.");

        /// <summary>
        /// "Aspect Ratio"
        /// </summary>
        public static LocalisableString AspectRatio => new TranslatableString(getKey(@"aspect_ratio"), @"Aspect Ratio");

        /// <summary>
        /// "Defines the ratio of the showcase area. Change this when you need to record a video with specific sizes."
        /// </summary>
        public static LocalisableString AspectRatioDescription => new TranslatableString(getKey(@"aspect_ratio_description"), @"Defines the ratio of the showcase area. Change this when you need to record a video with specific sizes.");

        /// <summary>
        /// "Transform Duration"
        /// </summary>
        public static LocalisableString TransformDuration => new TranslatableString(getKey(@"transform_duration"), @"Transform Duration");

        /// <summary>
        /// "The length of the transform animation between screens, in milliseconds."
        /// </summary>
        public static LocalisableString TransformDurationDescription => new TranslatableString(getKey(@"transform_duration_description"), @"The length of the transform animation between screens, in milliseconds.");

        /// <summary>
        /// "Countdown before Start"
        /// </summary>
        public static LocalisableString StartCountdownDuration => new TranslatableString(getKey(@"start_countdown_duration"), @"Countdown before Start");

        /// <summary>
        /// "A duration before the showcase starts in immersive layout and before continuing halfway. Get prepared this time!"
        /// </summary>
        public static LocalisableString StartCountdownDurationDescription => new TranslatableString(getKey(@"start_countdown_duration_description"), @"A duration before the showcase starts in immersive layout and before continuing halfway. Get prepared this time!");

        /// <summary>
        /// "Outro Title"
        /// </summary>
        public static LocalisableString OutroTitle => new TranslatableString(getKey(@"outro_title"), @"Outro Title");

        /// <summary>
        /// "Outro Subtitle"
        /// </summary>
        public static LocalisableString OutroSubtitle => new TranslatableString(getKey(@"outro_subtitle"), @"Outro Subtitle");

        /// <summary>
        /// "Show Beatmap List in the Showcase"
        /// </summary>
        public static LocalisableString ShowBeatmapListInShowcase => new TranslatableString(getKey(@"show_beatmap_list_in_showcase"), @"Show Beatmap List in the Showcase");

        /// <summary>
        /// "List out all beatmaps at the beginning of the showcase."
        /// </summary>
        public static LocalisableString ShowBeatmapListInShowcaseDescription => new TranslatableString(getKey(@"show_beatmap_list_in_showcase_description"), @"List out all beatmaps at the beginning of the showcase.");

        /// <summary>
        /// "Add Beatmap"
        /// </summary>
        public static LocalisableString AddBeatmap => new TranslatableString(getKey(@"add_beatmap"), @"Add Beatmap");

        /// <summary>
        /// "Remove this beatmap"
        /// </summary>
        public static LocalisableString RemoveBeatmap => new TranslatableString(getKey(@"remove_beatmap"), @"Remove this beatmap");

        /// <summary>
        /// "Tournament Original"
        /// </summary>
        public static LocalisableString TournamentOriginal => new TranslatableString(getKey(@"tournament_original"), @"Tournament Original");

        /// <summary>
        /// "This means the content of the beatmap is exclusively created for this tournament. Shows an indicator when turned on."
        /// </summary>
        public static LocalisableString TournamentOriginalDescription => new TranslatableString(getKey(@"tournament_original_description"), @"This means the content of the beatmap is exclusively created for this tournament. Shows an indicator when turned on.");

        /// <summary>
        /// "Beatmap Chooser ID"
        /// </summary>
        public static LocalisableString BeatmapChooserID => new TranslatableString(getKey(@"beatmap_chooser_id"), @"Beatmap Chooser ID");

        /// <summary>
        /// "The user who chose or suggested this beatmap."
        /// </summary>
        public static LocalisableString BeatmapChooserIDDescription => new TranslatableString(getKey(@"beatmap_chooser_id_description"), @"The user who chose or suggested this beatmap.");

        /// <summary>
        /// "Beatmap Mod Type"
        /// </summary>
        public static LocalisableString BeatmapModType => new TranslatableString(getKey(@"beatmap_mod_type"), @"Beatmap Mod Type");

        /// <summary>
        /// "Will be used to show a correct icon for the beatmap."
        /// </summary>
        public static LocalisableString BeatmapModTypeDescription => new TranslatableString(getKey(@"beatmap_mod_type_description"), @"Will be used to show a correct icon for the beatmap.");

        /// <summary>
        /// "Mod Index"
        /// </summary>
        public static LocalisableString BeatmapModIndex => new TranslatableString(getKey(@"beatmap_mod_index"), @"Mod Index");

        /// <summary>
        /// "The index of the beatmap in this type of mod."
        /// </summary>
        public static LocalisableString BeatmapModIndexDescription => new TranslatableString(getKey(@"beatmap_mod_index_description"), @"The index of the beatmap in this type of mod.");

        /// <summary>
        /// "Difficulty Field"
        /// </summary>
        public static LocalisableString DifficultyField => new TranslatableString(getKey(@"difficulty_field"), @"Difficulty Field");

        /// <summary>
        /// "The major area this beatmap lays difficulty on."
        /// </summary>
        public static LocalisableString DifficultyFieldDescription => new TranslatableString(getKey(@"difficulty_field_description"), @"The major area this beatmap lays difficulty on.");

        /// <summary>
        /// "Have something else to show on the showcase screen?"
        /// </summary>
        public static LocalisableString BeatmapCommentDescription => new TranslatableString(getKey(@"beatmap_comment_description"), @"Have something else to show on the showcase screen?");

        /// <summary>
        /// "Beatmap Details"
        /// </summary>
        public static LocalisableString ShowBeatmapDetails => new TranslatableString(getKey(@"show_beatmap_details"), @"Beatmap Details");

        /// <summary>
        /// "Remove Replay Score"
        /// </summary>
        public static LocalisableString RemoveReplayScore => new TranslatableString(getKey(@"remove_replay_score"), @"Remove Replay Score");

        /// <summary>
        /// "No score associated with this beatmap."
        /// </summary>
        public static LocalisableString NoScoreAssociationPrompt => new TranslatableString(getKey(@"no_score_association_prompt"), @"No score associated with this beatmap.");

        /// <summary>
        /// "Use custom intro beatmap"
        /// </summary>
        public static LocalisableString UseCustomIntroBeatmap => new TranslatableString(getKey(@"use_custom_intro_beatmap"), @"Use custom intro beatmap");

        /// <summary>
        /// "If enabled, we will use the beatmap below as a fixed intro song for the showcase. Otherwise the first beatmap will be used."
        /// </summary>
        public static LocalisableString UseCustomIntroBeatmapDescription => new TranslatableString(getKey(@"use_custom_intro_beatmap_description"),
            @"If enabled, we will use the beatmap below as a fixed intro song for the showcase. Otherwise the first beatmap will be used.");

        /// <summary>
        /// "Save"
        /// </summary>
        public static LocalisableString SaveAction => new TranslatableString(getKey(@"save_action"), @"Save");

        /// <summary>
        /// "Start Showcase"
        /// </summary>
        public static LocalisableString StartShowcase => new TranslatableString(getKey(@"start_showcase"), @"Start Showcase");

        /// <summary>
        /// "Are you sure to exit this screen?"
        /// </summary>
        public static LocalisableString ExitScreenDialogTitle => new TranslatableString(getKey(@"exit_screen_dialog_title"), @"Are you sure to exit this screen?");

        /// <summary>
        /// "Oops..."
        /// </summary>
        public static LocalisableString ErrorDialogTitle => new TranslatableString(getKey(@"error_dialog_title"), @"Oops...");

        /// <summary>
        /// "Fill in the tournament title, round and ruleset at least!"
        /// </summary>
        public static LocalisableString ProfileErrorDialogText => new TranslatableString(getKey(@"profile_error_dialog_text"), @"Fill in the tournament title, round and ruleset at least!");

        /// <summary>
        /// "Beatmap list empty"
        /// </summary>
        public static LocalisableString EmptyBeatmapListDialogTitle => new TranslatableString(getKey(@"empty_beatmap_list_dialog_title"), @"Beatmap list empty");

        /// <summary>
        /// "Consider adding one here."
        /// </summary>
        public static LocalisableString EmptyBeatmapListDialogText => new TranslatableString(getKey(@"empty_beatmap_list_dialog_text"), @"Consider adding one here.");

        /// <summary>
        /// "Custom null intro map?"
        /// </summary>
        public static LocalisableString NullIntroMapDialogTitle => new TranslatableString(getKey(@"null_intro_map_dialog_title"), @"Custom null intro map?");

        /// <summary>
        /// "Specify a custom intro beatmap, or turn off the switch to use the first beatmap in the queue."
        /// </summary>
        public static LocalisableString NullIntroMapDialogText => new TranslatableString(getKey(@"null_intro_map_dialog_text"), @"Specify a custom intro beatmap, or turn off the switch to use the first beatmap in the queue.");

        /// <summary>
        /// "Manual Control"
        /// </summary>
        public static LocalisableString ManualControlState => new TranslatableString(getKey(@"manual_control_state"), @"Manual Control");

        /// <summary>
        /// "Auto Control"
        /// </summary>
        public static LocalisableString AutoControlState => new TranslatableString(getKey(@"auto_control_state"), @"Auto Control");

        /// <summary>
        /// "Comment"
        /// </summary>
        public static LocalisableString Comment => new TranslatableString(getKey(@"comment"), @"Comment");

        /// <summary>
        /// "Click again to exit"
        /// </summary>
        public static LocalisableString ExitConfirmText => new TranslatableString(getKey(@"exit_confirm_text"), @"Click again to exit");

        /// <summary>
        /// "Map Pool"
        /// </summary>
        public static LocalisableString MapPoolHeader => new TranslatableString(getKey(@"map_pool_header"), @"Map Pool");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
