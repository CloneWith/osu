// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Screens.OnlinePlay
{
    public interface ISubScreenWithTitle : IOsuScreen
    {
        string Title { get; }

        string ShortTitle { get; }

        LocalisableString LocalisableTitle => default;

        bool ShowHeaderLine => true;
    }
}
