using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OEIS
{
    public static class Parser
    {
        public static OeisRow[] FromDatabase(SQLiteConnectionWithLock db)
        {
            using (db.Lock())
            {
                return db.Table<OeisRow>().ToArray();
            }
        }

        private static void ParseOEIS(SQLiteConnectionWithLock db, string folder)
        {
            List<Task> tasks = new List<Task>();
            List<long> times = new List<long>();
            var list = new int[] { 84720, 115245, 184834, 184866, 221694, 230052, 261806, 261880, 262260, 262626, 269415, 270439, 271212, 271213, 271214, 271215, 271216, 271217, 271218, 271972, 271973, 272004, 272005, 272006, 272120, 272459, 272604, 272605, 272606, 272607, 272608, 272609, 273006, 273060, 273125, 273140, 273148, 273164, 273165, 273166, 273255, 273256, 273257, 273258, 273260, 273286, 273287, 273457, 273488, 273492, 273495, 273531, 273621, 273664, 273669, 273673, 273775, 273845, 273972, 273973, 273974, 273975, 273976, 273977, 273978, 273979, 273980, 274004, 274009, 274055, 274058, 274059, 274060, 274123, 274137, 274189, 274216, 274222, 274223, 274224, 274246, 274319, 274323, 274327, 274377, 274378, 274379, 274398, 274399, 274458, 274473, 274474, 274475, 274476, 274482, 274483, 274487, 274489, 274494, 274495, 274519, 274546, 274600, 274601, 274602, 274607, 274608, 274647, 274648, 274649, 274661, 274662, 274683, 274684, 274734, 274735, 274771, 274772, 274778, 274796, 274820, 274821, 274824, 274825, 274845, 274846, 274847, 274911, 274912, 274913, 274914, 274915, 274916, 274917, 274918, 274919, 274920, 274921, 274922, 274950, 274951, 274952, 274977, 274981, 274982, 274983, 274984, 274985, 274986, 274987, 274988, 274989, 274990, 274991, 274992, 274993, 274994, 274995, 274996, 274997, 274998, 274999, 275000, 275001, 275002, 275003, 275004, 275005, 275006, 275007, 275008, 275009, 275010, 275011, 275012, 275013, 275014, 275015, 275016, 275017, 275018, 275019, 275020, 275021, 275022, 275023, 275024, 275025, 275026, 275027, 275028, 275029, 275030, 275066, 275078, 275108, 275110, 275113, 275149, 275152, 275154, 275155, 275156, 275157, 275158, 275159, 275160, 275161, 275162, 275163, 275164, 275173, 275174, 275175, 275176, 275198, 275220, 275255, 275277, 275288, 275306, 275308, 275322, 275324, 275325, 275326, 275327, 275328, 275329, 275330, 275331, 275341, 275343, 275372, 275384, 275388, 275390, 275391, 275415, 275432, 275435, 275436, 275437, 275438, 275439, 275440, 275441, 275442, 275443, 275444, 275445, 275446, 275447, 275448, 275451, 275471, 275472, 275475, 275477, 275478, 275479, 275481, 275483, 275484, 275485, 275520, 275537, 275539, 275540, 275541, 275542, 275544, 275545, 275571, 275573, 275575, 275578, 275579, 275581, 275583, 275585, 275586, 275587, 275588, 275589, 275598, 275599, 275600, 275601, 275602, 275603, 275604, 275605, 275606, 275607, 275608, 275609, 275610, 275611, 275615, 275623, 275624, 275625, 275626, 275627, 275628, 275629, 275630, 275631, 275632, 275633, 275634, 275635, 275636, 275637, 275638, 275639, 275640, 275641, 275642, 275643, 275644, 275645, 275646, 275647, 275651, 275652, 275653, 275654, 275655, 275657, 275659, 275660, 275661, 275662, 275665, 275666, 275669, 275672, 275677, 275680, 275681, 275682, 275683, 275684, 275685, 275686, 275687, 275690, 275694, 275695, 275696, 275698, 275699, 275701, 275702, 275703, 275704 };
            //for (int i = 0; i <= 278000; i++)
            foreach (int i in list)
            {
                string subFolder = String.Format("{0}\\{1}", folder, ((int)(i / 10000) * 10000).ToString("000000"));
                string filePath1 = String.Format("{0}\\b{1}.txt", subFolder, i.ToString("000000"));
                string filePath2 = String.Format("{0}\\b{1}.txt.m29", folder, i.ToString("000000"));
                if (File.Exists(filePath2) /*&& db.Table<OeisRow>().Where(o => o.OeisId == i).Count() == 0*/)
                {
                    tasks.Add(ParseM29File(filePath2, i, db));
                    //tasks.Add(ParseM29File(filePath2, i, db));
                    if (tasks.Count % 1000 == 0)
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        Task.WaitAll(tasks.ToArray());
                        tasks.Clear();
                        sw.Stop();
                        times.Add(sw.ElapsedMilliseconds);
                        Console.WriteLine("Time {0} @ i {1}", times.Average(), i);
                    }

                }

            }
            Task.WaitAll(tasks.ToArray());
            tasks.Clear();
        }

        private static Task ParseM29File(string m29FilePath, int oeisID, SQLiteConnectionWithLock db)
        {
            return Task.Factory.StartNew(() =>
            {
                sbyte[] mod29 = File.ReadAllBytes(m29FilePath).Select(m => (sbyte)m).ToArray();
                if (mod29.Length == 0)
                {
                    return;
                }
                byte[] mod29Rev = mod29.Select(m => (sbyte)((28 - m) % 29)).Select(m => (byte)m).ToArray();
                byte[] diff1 = Enumerable.Range(0, mod29.Length).Select(i => (sbyte)(mod29[(i + 1) % mod29.Length] - mod29[i])).Select(m => (byte)m).ToArray();
                byte[] diff2 = Enumerable.Range(0, mod29.Length).Select(i => (sbyte)(mod29[(i + 2) % mod29.Length] - mod29[i])).Select(m => (byte)m).ToArray();
                byte[] diff3 = Enumerable.Range(0, mod29.Length).Select(i => (sbyte)(mod29[(i + 3) % mod29.Length] - mod29[i])).Select(m => (byte)m).ToArray();
                using (db.Lock())
                {
                    db.InsertOrReplace(new OeisRow
                    {
                        OeisId = oeisID,
                        Mod29 = mod29.Select(m => (byte)m).ToArray()
                    });
                }
            });
        }

        private static Task ParseOEISFile(string oeisFilePath, int oiesID)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    IEnumerable<string> rows = File.ReadLines(oeisFilePath);
                    List<sbyte> results = new List<sbyte>();
                    foreach (string row in rows)
                    {
                        if (row.StartsWith("#"))
                            continue;

                        var temp = row.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length == 0 || String.IsNullOrWhiteSpace(temp[0]))
                            continue;


                        string[] values = temp[0].Split(new char[] { ' ', '\t', ',', '{', '}', '\\', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                        BigInteger streamVal;
                        if (BigInteger.TryParse(values.Take(2).Last(), out streamVal))
                        {
                            sbyte result = (sbyte)(streamVal % 29);
                            results.Add(result);
                        }
                        else
                        {
                            Console.WriteLine("{0} - {1}", oeisFilePath, row);
                        }
                    }
                    File.WriteAllBytes(oeisFilePath + ".m29", results.Select(r => (byte)r).ToArray());
                }
                catch (Exception ex)
                {

                }
            });
        }
    }
}
