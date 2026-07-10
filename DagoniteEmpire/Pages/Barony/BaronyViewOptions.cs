namespace DagoniteEmpire.Pages.Barony
{
    /// <summary>
    /// Współdzielony stan widoku baronii. Przekazywany jako CascadingValue.
    /// <see cref="DefaultExpanded"/> steruje domyślnym stanem rozwinięcia sekcji:
    /// true = sekcje domyślnie rozwinięte, false = domyślnie zwinięte.
    /// Zmiana wartości podnosi <see cref="Changed"/>, by sekcje bez ręcznego
    /// nadpisania mogły odświeżyć swój stan.
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

        /// <summary>Rośnie przy każdej zmianie domyślnego stanu — sekcje resetują wtedy ręczne nadpisanie.</summary>
        public int Version { get; private set; }

        public event Action? Changed;
    }
}
