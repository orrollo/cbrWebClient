using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using cbrf;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new CbrApi();

            var rates = LoadData(api, new DateTime(2014, 10, 1), DateTime.Now);
            // form csv table
            BuildByCode("R01235", rates);
            
        }

        private static void BuildByCode(string code, List<RateEntities> rates)
        {
            BuildOutFile("{0}.txt".Fmt(code), rates, rates.FindIndex(x1 => x1.Id == code));
        }

        private static void BuildOutFile(string fileName, List<RateEntities> rates, int x)
        {
            using (var wrt = new StreamWriter(fileName))
            {
                var items = new string[rates.Count + 1];
                items[rates.Count] = "pred";
                for (int i = 0; i < rates.Count; i++) items[i] = rates[i].Id;
                wrt.WriteLine(string.Join("\t", items));
                var cnt = rates[0].Count;
                for (int j = 0; j < cnt; j++)
                {
                    if ((j + 1) == cnt) continue;
                    for (int i = 0; i < rates.Count; i++) items[i] = d2s(rates[i][j].Rate);
                    items[rates.Count] = d2s(rates[x][j + 1].Rate);
                    wrt.WriteLine(string.Join("\t", items));
                }
                wrt.Flush();
            }
        }

        private static string d2s(decimal val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        private static List<RateEntities> LoadData(CbrApi api, DateTime startDate, DateTime endDate)
        {
            var rates = new List<RateEntities>();
            var valutes = GetValutes(api, "USD", "GBP", "JPY", "CHF", "EUR", "CNY", "TRY");
            rates.AddRange(valutes.Select(valute => api.GetValuteRates(valute.Id, startDate, endDate)));
            rates.AddRange(api.GetMetallRates(startDate, endDate));
            // get diffs
            var ids = rates.Select(x => x.Id).ToList();
            foreach (var id in ids)
            {
                var entities = rates.First(x=>x.Id==id);
                entities.DateSort();
                rates.Add(entities.GetDiffList());
            }
            // get averages for 7, 14, 28, 56 days
            for (int i = 0; i < 4; i++)
            {
                var days = 7*(1 << i);
                foreach (var id in ids) rates.Add(rates.First(x => x.Id == id).GetAverageList(days));
            }
            return rates;
        }

        private static List<Valute> GetValutes(CbrApi api, params string[] codes)
        {
            var valutesList = api.GetValutesList();
            return codes.Select(code => valutesList.FirstOrDefault(x => x.IsoCharCode == code)).Where(valute => valute != null).ToList();
        }
    }
}
