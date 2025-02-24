// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class BackgroundSelectStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.BackgroundSelect";

        /// <summary>
        /// "Background Settings"
        /// </summary>
        public static LocalisableString BackgroundSettingsTitle => new TranslatableString(getKey(@"background_settings_title"), @"Background Settings");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString Unknown => new TranslatableString(getKey(@"unknown"), @"Unknown");

        /// <summary>
        /// "Background Dim"
        /// </summary>
        public static LocalisableString BackgroundDim => new TranslatableString(getKey(@"background_dim"), @"Background Dim");

        /// <summary>
        /// "Select background for"
        /// </summary>
        public static LocalisableString SelectBackgroundFor => new TranslatableString(getKey(@"select_background_for"), @"Select background for");

        /// <summary>
        /// "Save to all"
        /// </summary>
        public static LocalisableString SaveAll => new TranslatableString(getKey(@"save_all"), @"Save to all");

        /// <summary>
        /// "Reset background settings?"
        /// </summary>
        public static LocalisableString ResetBackgroundTitle => new TranslatableString(getKey(@"reset_background_title"), @"Reset background settings?");

        /// <summary>
        /// "Are you sure to reset these to default? This cannot be undone."
        /// </summary>
        public static LocalisableString ResetBackgroundText => new TranslatableString(getKey(@"reset_background_text"),
            @"Are you sure to reset these to default? This cannot be undone.");

        /// <summary>
        /// "Yes, but just reset the selected one."
        /// </summary>
        public static LocalisableString DialogResetOne => new TranslatableString(getKey(@"dialog_reset_one"), @"Yes, but just reset the selected one.");

        /// <summary>
        /// "Yes, reset all of them."
        /// </summary>
        public static LocalisableString DialogResetAll => new TranslatableString(getKey(@"dialog_reset_all"), @"Yes, reset all of them.");

        /// <summary>
        /// "I'd rather stay the same."
        /// </summary>
        public static LocalisableString DialogCancel => new TranslatableString(getKey(@"dialog_cancel"), @"I'd rather stay the same.");

        /// <summary>
        /// "Select a file!"
        /// </summary>
        public static LocalisableString PromptSelectFile => new TranslatableString(getKey(@"prompt_select_file"), @"Select a file!");

        /// <summary>
        /// "Using: "
        /// </summary>
        public static LocalisableString PromptFileUsing => new TranslatableString(getKey(@"prompt_file_using"), @"Using: ");

        /// <summary>
        /// "Invalid file type."
        /// </summary>
        public static LocalisableString PromptFileInvalid => new TranslatableString(getKey(@"prompt_file_invalid"), @"Invalid file type.");

        /// <summary>
        /// "Preview on the right!"
        /// </summary>
        public static LocalisableString PromptFilePreview => new TranslatableString(getKey(@"prompt_file_preview"), @"Preview on the right!");

        /// <summary>
        /// "Videos"
        /// </summary>
        public static LocalisableString FileTypeVideo => new TranslatableString(getKey(@"file_type_video"), @"Videos");

        /// <summary>
        /// "Images"
        /// </summary>
        public static LocalisableString FileTypeImage => new TranslatableString(getKey(@"file_type_image"), @"Images");

        /// <summary>
        /// "{0} must be selected from current \"{1}\" directory."
        /// </summary>
        public static LocalisableString PromptFilePath(LocalisableString type, LocalisableString path) => new TranslatableString(getKey(@"prompt_file_path"),
            @"{0} must be selected from current ""{1}"" directory.", type, path);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

