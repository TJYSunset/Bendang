using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Troschuetz.Random;
using Troschuetz.Random.Generators;

namespace Bendang
{
    public class PseudoJapaneseGenerator
    {
        public PseudoJapaneseGenerator(int seed)
        {
            Random = new TRandom(new MT19937Generator(seed));
        }

        private TRandom       Random { get; }
        private StringBuilder Sb     { get; } = new StringBuilder();

        public string Generate()
        {
            double ps, pp;

            ps = .7;
            CreateSentence:

            pp = .8;
            CreatePhrase:
            Sb.Append(Phrase(out var doNotTerminateSentence));
            if (doNotTerminateSentence || Random.NextDouble() < pp)
            {
                pp *= pp;
                goto CreatePhrase;
            }

            if (Random.NextDouble() < ps)
            {
                ps *= ps;
                Sb.Append(Comma());
                goto CreateSentence;
            }

            Sb.Append(Period());

            if (Random.NextDouble() < ps)
            {
                ps *= ps;
                goto CreateSentence;
            }

            return Sb.ToString();
        }

        private string Phrase(out bool doNotTerminateSentence, bool allowNested = true)
        {
            string Generic(IList<string> set, double continuationProbability, IList<string> nonInitial = null,
                           IList<string> nonTerminal = null)
            {
                var sb = new StringBuilder();
                AddChar:
                sb.Append(set.RandomElement(Random));
                if (Random.NextDouble() < continuationProbability)
                {
                    continuationProbability *= continuationProbability;
                    goto AddChar;
                }

                var str = sb.ToString();

                DoubleCheck:
                if (nonInitial != null && nonInitial.Any(x => str.StartsWith(x)))
                {
                    str = set.RandomElement(Random) + str.Substring(1);
                    goto DoubleCheck;
                }

                if (nonTerminal != null && nonTerminal.Any(x => str.EndsWith(x)))
                {
                    str = str.Substring(0, str.Length - 1) + nonTerminal.RandomElement(Random);
                    goto DoubleCheck;
                }

                return str;
            }

            string Hiragana()
            {
                return Generic(Data.Hiragana, .8, Data.HiraganaNonInitial, Data.HiraganaNonTerminal);
            }

            string Katakana()
            {
                return Generic(Data.Katakana, .8, Data.KatakanaNonInitial, Data.KatakanaNonTerminal);
            }

            string Kanji()
            {
                return Generic(Data.JouyouKanji, .4);
            }

            string Katsuyou()
            {
                return Data.Katsuyou.RandomElement(Random);
            }

            string Numeral()
            {
                return Generic(Data.Numeral, .5, Data.NumeralNonInitial, Data.NumeralNonTerminal);
            }

            string Nested()
            {
                return string.Format(Data.NestedPhrase.RandomElement(Random), Phrase(out _, false));
            }

            var p = Random.NextDouble();
            if (allowNested && Random.NextDouble() < .05)
            {
                doNotTerminateSentence = true;
                return Nested();
            }

            if (Random.NextDouble() < .05)
            {
                doNotTerminateSentence = true;
                return Numeral();
            }
            else
            {
                doNotTerminateSentence = false;
                return p < .3
                           ? Hiragana()
                           : p < .8
                               ? Kanji()
                               : Katakana()
                                 + (Random.NextDouble() < .4 ? Katsuyou() : "");
            }
        }

        private string Comma()
        {
            return Data.Comma.RandomElement(Random);
        }

        private string Period()
        {
            return Data.Period.RandomElement(Random);
        }
    }
}