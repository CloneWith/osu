// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// This file is partly modified by GooGuTeam.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public partial class WebSettings : SettingsSubsection
    {
        protected override LocalisableString Header => OnlineSettingsStrings.WebHeader;

        [Resolved]
        private OsuGame? game { get; set; }

        private SettingsTextBox customApiUrlTextBox = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = OnlineSettingsStrings.ExternalLinkWarning,
                    Current = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = OnlineSettingsStrings.PreferNoVideo,
                    Current = config.GetBindable<bool>(OsuSetting.PreferNoVideo)
                })
                {
                    Keywords = new[] { "no-video" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = OnlineSettingsStrings.AutomaticallyDownloadMissingBeatmaps,
                    Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadMissingBeatmaps),
                })
                {
                    Keywords = new[] { "spectator", "replay" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = OnlineSettingsStrings.ShowExplicitContent,
                    Current = config.GetBindable<bool>(OsuSetting.ShowOnlineExplicitContent),
                })
                {
                    Keywords = new[] { "nsfw", "18+", "offensive" }
                },
                customApiUrlTextBox = new SettingsTextBox
                {
                    LabelText = OnlineSettingsStrings.CustomApiUrl,
                    Current = config.GetBindable<string>(OsuSetting.CustomApiUrl)
                }
            };

            customApiUrlTextBox.Current.BindValueChanged(onCustomApiUrlChanged, true);
        }

        private string lastApiUrl = string.Empty;
        private bool isInitialLoad = true;
        private ScheduledDelegate? pendingValidation;
        private const double debounce_delay = 500;

        private static readonly Regex hostPortPattern = new Regex(
            pattern:
            @"^(?:" +
            @"(?:(?:[A-Za-z0-9-]+)\.)+[A-Za-z0-9-]+" +
            @"|" +
            @"(?:(?:25[0-5]|2[0-4]\d|1?\d{1,2})\.){3}(?:25[0-5]|2[0-4]\d|1?\d{1,2})" +
            @")(?::(?<port>\d{1,5}))?$",
            options: RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private void onCustomApiUrlChanged(ValueChangedEvent<string> e)
        {
            if (isInitialLoad)
            {
                var initRaw = (e.NewValue ?? string.Empty).Trim();
                lastApiUrl = normalizeToHttps(initRaw);
                isInitialLoad = false;
                return;
            }

            pendingValidation?.Cancel();
            pendingValidation = Scheduler.AddDelayed(() =>
            {
                string rawInput = (customApiUrlTextBox.Current.Value ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(rawInput))
                {
                    maybeShowRestartIfChanged(string.Empty);
                    customApiUrlTextBox.SetNoticeText(string.Empty, false);
                    return;
                }

                string hostPort = stripSchemeAndPath(rawInput);

                if (!isValidHostPort(hostPort))
                {
                    customApiUrlTextBox.SetNoticeText(OnlineSettingsStrings.CustomApiUrlInvalid, true);
                    return;
                }

                string normalised = "https://" + hostPort;
                customApiUrlTextBox.SetNoticeText(string.Empty, false);
                maybeShowRestartIfChanged(normalised);
            }, debounce_delay);
        }

        private static bool isValidHostPort(string hostPort)
        {
            var m = hostPortPattern.Match(hostPort);
            if (!m.Success) return false;

            var g = m.Groups["port"];

            if (g.Success)
            {
                if (!int.TryParse(g.Value, out int port)) return false;
                if (port < 1 || port > 65535) return false;
            }

            return true;
        }

        private static string normalizeToHttps(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            string hostPort = stripSchemeAndPath(raw);
            return isValidHostPort(hostPort) ? "https://" + hostPort : hostPort;
        }

        private static string stripSchemeAndPath(string input)
        {
            string s = Regex.Replace(input, @"^\s*https?://", "", RegexOptions.IgnoreCase);

            int slash = s.IndexOf('/');
            if (slash >= 0) s = s.Substring(0, slash);

            s = s.TrimEnd('/');

            return s;
        }

        private void maybeShowRestartIfChanged(string normalizedNewValue)
        {
            if (!string.Equals(lastApiUrl, normalizedNewValue, StringComparison.OrdinalIgnoreCase))
            {
                lastApiUrl = normalizedNewValue;
                customApiUrlTextBox.SetNoticeText(OnlineSettingsStrings.CustomApiUrlRestartRequired, false);
            }
        }
    }
}
