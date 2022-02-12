﻿using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;

namespace AWBWApp.Game.UI.Replay
{
    public class RollingCounter<T> : Container, IHasCurrentValue<T>
        where T : struct, IEquatable<T>
    {
        public Drawable DrawableCount { get; private set; }

        public T DisplayedCount
        {
            get => displayedCount;
            set
            {
                if (EqualityComparer<T>.Default.Equals(displayedCount, value))
                    return;
                displayedCount = value;
                UpdateDisplay();
            }
        }

        private T displayedCount;

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        protected bool IsRollingProportionalToChange => false;
        protected double RollingDuration => 1000;
        protected virtual Easing RollingEasing => Easing.OutQuint;

        private SpriteText displayedCountText;

        public RollingCounter()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            displayedCountText = CreateText();

            UpdateDisplay();
            Child = DrawableCount = displayedCountText;
        }

        protected void UpdateDisplay()
        {
            if (displayedCountText != null)
                displayedCountText.Text = FormatCount(DisplayedCount);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(val => TransformCount(DisplayedCount, val.NewValue), true);
        }

        protected void TransformCount(T currentValue, T newValue)
        {
            double rollingTotalDuration = RollingDuration;
            //IsRollingProportionalToChange ? GetProportionalDuration(currentValue, newValue) : RollingDuration;

            this.TransformTo(nameof(DisplayedCount), newValue, rollingTotalDuration, RollingEasing);
        }

        //protected double GetProportionalDuration(T currentValue, T newValue) => currentValue > newValue ? currentValue - newValue : newValue - currentValue;

        protected LocalisableString FormatCount(T count) => count.ToString();

        protected SpriteText CreateText() => new SpriteText();
    }
}