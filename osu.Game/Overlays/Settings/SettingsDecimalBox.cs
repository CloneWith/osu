// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsDecimalBox : SettingsItem<double?>
    {
        protected override Drawable CreateControl() => new DecimalControl
        {
            RelativeSizeAxes = Axes.X,
        };

        private sealed partial class DecimalControl : CompositeDrawable, IHasCurrentValue<double?>
        {
            private readonly BindableWithCurrent<double?> current = new BindableWithCurrent<double?>();

            public Bindable<double?> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            public DecimalControl()
            {
                AutoSizeAxes = Axes.Y;

                OutlinedDecimalBox numberBox;

                InternalChildren = new[]
                {
                    numberBox = new OutlinedDecimalBox
                    {
                        RelativeSizeAxes = Axes.X,
                        CommitOnFocusLost = true
                    }
                };

                numberBox.OnCommit += (textBox, _) =>
                {
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        Current.Value = null;
                        return;
                    }

                    if (double.TryParse(textBox.Text, out double intVal))
                        Current.Value = intVal;
                    else
                        numberBox.NotifyInputError();

                    // trigger Current again to either restore the previous text box value, or to reformat the new value via .ToString().
                    Current.TriggerChange();
                };

                Current.BindValueChanged(e =>
                {
                    numberBox.Current.Value = e.NewValue?.ToString();
                });
            }
        }

        private partial class OutlinedDecimalBox : OutlinedTextBox
        {
            protected override bool CanAddCharacter(char character)
                => char.IsAsciiDigit(character) || (character == '.' && !Text.Contains('.'));

            public new void NotifyInputError() => base.NotifyInputError();
        }
    }
}
