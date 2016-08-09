using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace Solver
{
    class Program
    {
        static void Main(string[] args)
        {
            DataModel model = new DataModel();
            model.Initialize(@"C:\Users\xbox1\OneDrive\Cicada\Solver\Solver\DataSource\liber-master");

            for (int sectionIndex = 7; sectionIndex < model.Sections.Count - 2; sectionIndex++)
            {
                var section = model.Sections[sectionIndex];
                var rez = section.Characters.NGramOffsets().Where(k => k.Key[0].GematriaIndex == k.Key[1].GematriaIndex).ToList();

            }
            Paragraph testParagraph = new Paragraph();
            testParagraph.AddRange(
            File.ReadAllLines(@"DataSource\CicadaSentences.txt").Select(l =>
            {
                Sentence s = new Sentence();
                s.AddRange(l.Split(' ').Select(w => w.ToWord()));
                return s;
            }).ToList());


            //var output1 = Ciphers.EncodeCondiNew(testParagraph.Characters.Take(736).ToList());
            //var output2 = Ciphers.EncodeCondiNew(output1);



            //int[] key = new int[] { };
            StringBuilder doubles = new StringBuilder();
            for (int sectionIndex = 7; sectionIndex < model.Sections.Count - 2; sectionIndex++)
            {
                var section = model.Sections[sectionIndex];
                var count = section.Characters.Count;
                var counter = section.Characters.NGrams()/*.Where(n => n.Key[0].GematriaIndex == n.Key[1].GematriaIndex)*/.ToArray();
                doubles.AppendFormat("Section {0} ({1} runes) - unique pairs count: {2} ({3})\r\n",
                    sectionIndex - 6,
                    count,
                    counter.Length,
                    String.Join(", ", counter.Select(k => k.Key.ToString() + "=" + k.Value.Count)));

            }
        }
    }
}
