using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public class DistributionViewModel : SelectableViewModelBase
    {
        private readonly Type _symbolType;
        private bool _adjustingProbabilities;

        public DistributionViewModel(IDistribution distribution)
        {
            _symbolType = distribution.SymbolType;
            IEnumerable<double> probs = from v in Enum.GetValues(distribution.SymbolType).Cast<object>()
                                        select distribution[v];

            string[] symbols = Enum.GetNames(_symbolType);
            Occurrences = new List<Occurrence>(symbols.Zip(probs, (s, p) => new Occurrence
            {
                Symbol = s,
                Probability = p
            }));

            foreach (Occurrence occ in Occurrences)
            {
                Occurrence o = occ;
                occ.PropertyChanged += (s, e) => AdjustProbabilities(o);
            }

            Distribution = new ReactiveProperty<IDistribution>(distribution);

            _adjustingProbabilities = false;
        }

        public IList<Occurrence> Occurrences { get; private set; }
        public ReactiveProperty<IDistribution> Distribution { get; private set; }

        public override string DisplayName
        {
            get { return _symbolType.Name; }
        }

        private void AdjustProbabilities(Occurrence changed)
        {
            if (_adjustingProbabilities) return;
            _adjustingProbabilities = true;

            try
            {
                double p = changed.Probability;
                // sum the probs we have to stretch. 
                Occurrence[] others = Occurrences.Except(new[] {changed}).ToArray();
                double rest = others.Sum(t => t.Probability);

                p = p.Clamp(0.0, 1.0); // p is a probability
                double q = 1.0 - p;
                double toDistribute = q - rest;

                // now stretch all probabilities except what was changed by that value
                foreach (Occurrence occ in others)
                {
                    double share = rest > 0 ? occ.Probability/rest : 1.0/others.Length;
                    occ.Probability = (occ.Probability + share*toDistribute).Clamp(0.0, 1.0);
                }

                MethodInfo fromProbs = typeof (DistributionViewModel)
                    .GetMethod("DistributionFromProbabilites", BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(_symbolType);
                Distribution.Value = (IDistribution) fromProbs.Invoke(null, new object[] {Occurrences});
            }
            finally
            {
                _adjustingProbabilities = false;
            }
        }

// ReSharper disable once UnusedMember.Local
        private static Distribution<T> DistributionFromProbabilites<T>(IEnumerable<Occurrence> probs)
            where T : /* Enum,*/ struct
        {
            return Distribution<T>.FromProbabilites(from t in probs
                                                    select
                                                        Tuple.Create((T) Enum.Parse(typeof (T), t.Symbol), t.Probability));
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