using System.Text.Json.Serialization;

namespace DA_Common.Barony
{
    /// <summary>
    /// Wektor 13 wartości PPB. Używany zarówno jako modyfikator addytywny, jak i procentowy
    /// przez wszystkie źródła modyfikatorów. Serializowany do JSON (wzorem BattleMap.CellsJson).
    /// Wartości procentowe wyrażane są w punktach procentowych (10 = +10%).
    /// </summary>
    public sealed class PpbVector
    {
        /// <summary>Wartości indeksowane wg <see cref="Ppb"/>. Zawsze długości <see cref="PpbCatalog.Count"/>.</summary>
        public decimal[] Values { get; set; } = new decimal[PpbCatalog.Count];

        public PpbVector()
        {
        }

        public PpbVector(decimal[] values)
        {
            SetValues(values);
        }

        [JsonIgnore]
        public decimal this[Ppb p]
        {
            get => Get(p);
            set => Set(p, value);
        }

        public decimal Get(Ppb p)
        {
            EnsureSize();
            return Values[(int)p];
        }

        public void Set(Ppb p, decimal value)
        {
            EnsureSize();
            Values[(int)p] = value;
        }

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                EnsureSize();
                foreach (var v in Values)
                {
                    if (v != 0m)
                        return false;
                }
                return true;
            }
        }

        public PpbVector Clone()
        {
            EnsureSize();
            return new PpbVector((decimal[])Values.Clone());
        }

        public void AddInPlace(PpbVector other)
        {
            if (other is null)
                return;
            EnsureSize();
            other.EnsureSize();
            for (int i = 0; i < PpbCatalog.Count; i++)
                Values[i] += other.Values[i];
        }

        public static PpbVector operator +(PpbVector a, PpbVector b)
        {
            var result = (a ?? new PpbVector()).Clone();
            result.AddInPlace(b);
            return result;
        }

        /// <summary>Suma listy wektorów (np. wszystkich modyfikatorów jednej sekcji).</summary>
        public static PpbVector Sum(IEnumerable<PpbVector?> vectors)
        {
            var result = new PpbVector();
            if (vectors is null)
                return result;
            foreach (var v in vectors)
                result.AddInPlace(v);
            return result;
        }

        /// <summary>
        /// Zapewnia poprawną długość tablicy po deserializacji z ewentualnie starszego/uszkodzonego JSON.
        /// </summary>
        public void EnsureSize()
        {
            if (Values is null)
            {
                Values = new decimal[PpbCatalog.Count];
                return;
            }
            if (Values.Length != PpbCatalog.Count)
            {
                var resized = new decimal[PpbCatalog.Count];
                Array.Copy(Values, resized, Math.Min(Values.Length, PpbCatalog.Count));
                Values = resized;
            }
        }

        private void SetValues(decimal[]? values)
        {
            Values = new decimal[PpbCatalog.Count];
            if (values is null)
                return;
            Array.Copy(values, Values, Math.Min(values.Length, PpbCatalog.Count));
        }
    }

    /// <summary>Pomocnicze operacje podsumowania PPB (baza + addytywne, następnie procentowe).</summary>
    public static class PpbMath
    {
        /// <summary>
        /// Wstępny wzór podsumowania: wynik = (baza + Σ addytywne) * (1 + Σ procent/100).
        /// Dokładny porządek zostanie potwierdzony przy dostarczeniu formuł.
        /// </summary>
        public static PpbVector Summarize(PpbVector? baseVec, PpbVector? additive, PpbVector? percent)
        {
            var result = new PpbVector();
            var b = baseVec ?? new PpbVector();
            var a = additive ?? new PpbVector();
            var p = percent ?? new PpbVector();
            b.EnsureSize();
            a.EnsureSize();
            p.EnsureSize();

            for (int i = 0; i < PpbCatalog.Count; i++)
            {
                var value = (b.Values[i] + a.Values[i]) * (1m + p.Values[i] / 100m);
                result.Values[i] = decimal.Round(value, 2);
            }
            return result;
        }
    }
}
