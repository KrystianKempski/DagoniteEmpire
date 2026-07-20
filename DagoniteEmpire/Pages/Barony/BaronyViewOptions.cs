namespace DagoniteEmpire.Pages.Barony
{
    /// <summary>
    /// Shared barony view state, passed through CascadingValue.
    /// <see cref="DefaultExpanded"/> controls default section state:
    /// true = sections expanded by default, false = collapsed by default.
    /// Value changes trigger <see cref="Changed"/> so sections without
    /// manual overrides can refresh their state.
    /// </summary>
    public sealed class BaronyViewOptions
    {
        private bool _defaultExpanded = true;

        public bool DefaultExpanded
        {
            get => _defaultExpanded;
            set
            {
                if (_defaultExpanded == value)
                    return;
                _defaultExpanded = value;
                Version++;
                Changed?.Invoke();
            }
        }

        /// <summary>Increments on each default state change so sections can reset manual overrides.</summary>
        public int Version { get; private set; }

        public event Action? Changed;

        /// <summary>Fired when cumulative stocks / expected income may have changed (Resources, Budget transfers).</summary>
        public event Action? ResourceHudChanged;

        public void NotifyResourceHudChanged() => ResourceHudChanged?.Invoke();
    }
}
