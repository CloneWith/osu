// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsDecimalBox : SettingsItem<double?>
    {
        private readonly bool allowNegative;

        public SettingsDecimalBox()
            : this(false)
        {
        }

        public SettingsDecimalBox(bool allowNegative = false) => this.allowNegative = allowNegative;

        protected override Drawable CreateControl() => new DecimalControl(allowNegative)
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

            public DecimalControl(bool allowNegative = false)
            {
                AutoSizeAxes = Axes.Y;

                OutlinedDecimalBox numberBox;

                InternalChildren = new[]
                {
                    numberBox = new OutlinedDecimalBox(allowNegative)
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

                    if (double.TryParse(textBox.Text, out double doubleVal))
                        Current.Value = doubleVal;
                    else
                    {
                        numberBox.NotifyInputError();
                        numberBox.Text = Current.Value?.ToString();
                    }
                };

                Current.BindValueChanged(e =>
                {
                    numberBox.Current.Value = e.NewValue?.ToString();
                });
            }
        }

        private partial class OutlinedDecimalBox : OutlinedTextBox
        {
            private readonly bool allowNegative;

            public OutlinedDecimalBox(bool allowNegative = false)
            {
                this.allowNegative = allowNegative;
                InputProperties = new TextInputProperties(allowNegative ? TextInputType.Text : TextInputType.Decimal, false);
            }

            protected override bool CanAddCharacter(char character)
                => char.IsAsciiDigit(character)
                   || character == '.'
                   || (allowNegative && character == '-');

            public new void NotifyInputError() => base.NotifyInputError();
        }
    }
}
