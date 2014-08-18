using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Caliburn.Micro;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public class DistributionViewModel : Screen
    {
        private readonly Type _symbolType;
        private readonly IEnumerable<string> _symbols;
        private event Action<IDistribution> NotifyDistribution = delegate { };
        private bool _adjustingProbabilities;

        public IList<Occurrence> Occurrences { get; private set; } 
        public IObservable<IDistribution> Distribution { get; private set; }

        public string SymbolTypeName { get { return _symbolType.Name; } }

        public DistributionViewModel(IDistribution distribution)
        {
            _symbolType = distribution.SymbolType;
            _symbols = Enum.GetNames(_symbolType);
            var probs = from v in Enum.GetValues(distribution.SymbolType).Cast<object>()
                        select distribution[v];

            Occurrences = new List<Occurrence>(_symbols.Zip(probs, (s, p)=>new Occurrence
            {
                Symbol = s,
                Probability = p
            }));

            foreach (var occ in Occurrences)
            {
                occ.PropertyChanged += (s,e) => AdjustProbabilities(occ);
            }

            Distribution = Observable.FromEvent<IDistribution>(
                h => NotifyDistribution += h,
                h => NotifyDistribution -= h);

            _adjustingProbabilities = false;
        }

        private void AdjustProbabilities(Occurrence changed)
        {
            if (_adjustingProbabilities) return;
            _adjustingProbabilities = true;

            try
            {
                var changedIdx = Occurrences.IndexOf(changed);
                var p = changed.Probability;
                // sum the probs we have to stretch. 
                var others = Occurrences.Except(new[] {changed}).ToArray();
                var rest = others.Sum(t => t.Probability); 

                p = p.Clamp(0.0, 1.0); // p is a probability
                var q = 1.0 - p;
                var toDistribute = q - rest;

                // now stretch all probabilities except what was changed by that value
                foreach (var occ in others)
                {
                    var share = rest > 0 ? occ.Probability/rest : 1.0/others.Length;
                    occ.Probability = (occ.Probability + share*toDistribute).Clamp(0.0, 1.0);
                }

                var fromProbs = typeof (DistributionViewModel)
                    .GetMethod("DistributionFromProbabilites", BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(_symbolType);
                var dist = (IDistribution) fromProbs.Invoke(null, new object[] {Occurrences});
                NotifyDistribution(dist);
            }
            finally
            {
                _adjustingProbabilities = false;
            }
        }

// ReSharper disable once UnusedMember.Local
        private static Distribution<T> DistributionFromProbabilites<T>(IEnumerable<Occurrence> probs) where T : /* Enum,*/ struct
        {
            return Distribution<T>.FromProbabilites(from t in probs
                                                    select Tuple.Create((T) Enum.Parse(typeof (T), t.Symbol), t.Probability));
        }

        public class Occurrence : PropertyChangedBase
        {
            private double _probability;

            public string Symbol { get; set; }

            public double Probability
            {
                get { return _probability; }
                set
                {
                    if (value.Equals(_probability)) return;
                    _probability = value;
                    NotifyOfPropertyChange();
                }
            }
        }
    }
}