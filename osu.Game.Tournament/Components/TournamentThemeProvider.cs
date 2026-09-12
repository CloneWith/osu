// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// Resolves the <see cref="RoundTheme"/> of the current match's round and keeps it up to date.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Cached for injection by <see cref="TournamentGameBase"/>, so any drawable below it can request this
    /// type in its <see cref="osu.Framework.Allocation.BackgroundDependencyLoaderAttribute"/>.
    /// </para>
    /// <para>
    /// The chain of source bindables (current match -> that match's round -> the round's theme properties)
    /// is tracked here rather than in each consumer. Because every tournament screen is constructed eagerly
    /// at startup, thus unable to receive the theme at dependency injection time.
    /// </para>
    /// <para>
    /// This intentionally only covers the stream area. The control area follows the <see cref="OverlayColourProvider"/>.
    /// </para>
    /// </remarks>
    public sealed class TournamentThemeProvider
    {
        /// <summary>
        /// The theme of the round currently being played. Never <see langword="null"/>.
        /// </summary>
        public readonly Bindable<RoundTheme> Current = new Bindable<RoundTheme>(RoundTheme.DEFAULT);

        private readonly Bindable<TournamentMatch?> match = new Bindable<TournamentMatch?>();
        private readonly IBindable<TournamentRound?> round = new Bindable<TournamentRound?>();
        private readonly IBindable<bool> useCustomThemeColour = new BindableBool();
        private readonly IBindable<Colour4> themeColour = new BindableColour4(Colour4.White);

        /// <summary>
        /// Starts tracking the given ladder.
        /// </summary>
        /// <remarks>
        /// Must be passed the ladder instance which is actually cached for injection. <see cref="TournamentGameBase"/>
        /// replaces its ladder reference once the bracket has been read, so an earlier instance must not be used.
        /// </remarks>
        /// <param name="ladder">the ladder to track.</param>
        public void Attach(LadderInfo ladder)
        {
            match.BindTo(ladder.CurrentMatch);
            match.BindValueChanged(_ =>
            {
                round.UnbindAll();

                if (match.Value != null)
                    round.BindTo(match.Value.Round);

                round.BindValueChanged(_ => bindRound(), true);
            }, true);
        }

        private void bindRound()
        {
            // The round instance is replaced wholesale when switching matches, so the inner bindings have to be
            // dropped first - otherwise edits to the previous round would keep updating the theme.
            useCustomThemeColour.UnbindAll();
            themeColour.UnbindAll();

            if (round.Value != null)
            {
                useCustomThemeColour.BindTo(round.Value.UseCustomThemeColour);
                themeColour.BindTo(round.Value.ThemeColour);
            }

            useCustomThemeColour.BindValueChanged(_ => update());
            themeColour.BindValueChanged(_ => update());

            update();
        }

        private void update()
        {
            var resolved = RoundTheme.FromRound(round.Value);

            // avoid firing a redundant notification while a colour picker is being dragged.
            if (resolved.Equals(Current.Value))
                return;

            Current.Value = resolved;
        }
    }
}
