using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace ConsoleApplication1
{
    public static class StringHelper
    {
        public static string Fmt(this string fmt, params object[] args)
        {
            return string.IsNullOrEmpty(fmt) || args == null || args.Length == 0 ? fmt : string.Format(fmt, args);
        }
    }

    public class Entity : Dictionary<String, String>
    {
        protected string Get(string name, string defaultValue)
        {
            return this.ContainsKey(name) ? this[name] : defaultValue;
        }
    }

    public class IdEntity : Entity
    {
        public string Id { get { return GetId(); } }

        protected virtual string GetId()
        {
            return Get("ID", "").Trim();
        }

        public override string ToString()
        {
            return "Id={0}".Fmt(Id);
        }
    }

    public class Valute : IdEntity
    {
        public string Name { get { return Get("NAME", ""); } }
        public int Nominal { get { return int.Parse(Get("NOMINAL", "1")); } }
        public string EngName { get { return Get("ENGNAME", ""); } }
        public string ParentCode { get { return Get("PARENTCODE", "").Trim(); } }
        public int IsoNumCode { get { return int.Parse(Get("ISO_NUM_CODE", "-1")); } }
        public string IsoCharCode { get { return Get("ISO_CHAR_CODE", ""); } }
    }

    public abstract class RateEntity : IdEntity
    {
        public decimal Rate { get { return GetRate(); } }
        public DateTime Date { get { return DateTime.ParseExact(Get("DATE", ""),"dd.MM.yyyy",CultureInfo.InvariantCulture); } }
        protected abstract decimal GetRate();

        public override string ToString()
        {
            return "Id={0};Date={1};Rate={2}".Fmt(Id, Date, Rate);
        }
    }

    public class ValuteRate : RateEntity
    {
        public int Nominal { get { return int.Parse(Get("NOMINAL", "1")); } }
        public decimal Value { get { return decimal.Parse(Get("VALUE", "1").Replace(',','.'), NumberStyles.Any, CultureInfo.InvariantCulture); } }

        protected override decimal GetRate()
        {
            return Value/Nominal;
        }
    }

    public class MetallRate : RateEntity
    {
        protected override string GetId()
        {
            switch (Code)
            {
                case "1":
                    return "MGOLD";
                case "2":
                    return "MSILVER";
                case "3":
                    return "MPLATINUM";
                case "4":
                    return "MPALLADIUM";
                default:
                    return Code;
            }
        }

        public string Code { get { return Get("CODE","0"); } }
        public decimal Buy { get { return decimal.Parse(Get("BUY", "1").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture); } }
        public decimal Sell { get { return decimal.Parse(Get("SELL", "1").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture); } }

        protected override decimal GetRate()
        {
            return Sell;
        }
    }

    public class CbrApi
    {
        public string BaseUrl { get; set; }

        public CbrApi(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public CbrApi()
            : this("http://www.cbr.ru/scripts/XML_")
        {
            
        }

        protected string GetUrl(string restPart)
        {
            return BaseUrl + restPart;
        }

        public List<T> ParseXml<T>(string xml) where T:Entity, new()
        {
            var ret = new List<T>();
            if (!string.IsNullOrEmpty(xml))
            {
                using (var textReader = new StringReader(xml))
                {
                    T currentEntity = null;
                    string currentName = null;
                    using (var rdr = new XmlTextReader(textReader))
                    {
                        int mode = 0;
                        while (rdr.Read())
                        {
                            if (rdr.NodeType == XmlNodeType.Element)
                            {
                                if (mode == 1)
                                {
                                    if (currentEntity != null) ret.Add(currentEntity);
                                    currentEntity = new T();
                                    if (rdr.HasAttributes)
                                    {
                                        for (int idx = 0; idx < rdr.AttributeCount; idx++)
                                        {
                                            rdr.MoveToAttribute(idx);
                                            var name = (rdr.Name ?? "-").ToUpper();
                                            var value = rdr.Value;
                                            currentEntity.Add(name, value);
                                        }
                                    }
                                }
                                if (mode == 2) currentName = rdr.Name.ToUpper();
                                if (!rdr.IsEmptyElement) mode++;
                            }
                            else if (rdr.NodeType == XmlNodeType.Text)
                            {
                                if (mode == 3 && currentEntity != null) currentEntity.Add(currentName ?? "-", rdr.Value);
                            }
                            else if (rdr.NodeType == XmlNodeType.EndElement)
                            {
                                if (mode == 2)
                                {
                                    if (currentEntity != null) ret.Add(currentEntity);
                                    currentEntity = null;
                                }
                                if (mode > 0) mode--;
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public List<Valute> GetValutesList()
        {
            var xml = DownloadXml("valFull.asp");
            return ParseXml<Valute>(xml);
        }

        protected string d2s(DateTime date)
        {
            return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        public List<RateEntity> GetValuteRates(string valuteId, DateTime startDate, DateTime endDate)
        {
            var xml = DownloadXml("dynamic.asp?date_req1={0}&date_req2={1}&VAL_NM_RQ={2}".Fmt(d2s(startDate),d2s(endDate),valuteId));
            return ParseXml<ValuteRate>(xml).Cast<RateEntity>().ToList();
        }

        private string DownloadXml(string restUrl)
        {
            using (var client = new WebClient())
            {
                var xml = client.DownloadString(GetUrl(restUrl));
                return xml;
            }
        }

        public List<RateEntity> GetMetallRates(DateTime startDate, DateTime endDate)
        {
            var xml = DownloadXml("metall.asp?date_req1={0}&date_req2={1}".Fmt(d2s(startDate), d2s(endDate)));
            return ParseXml<MetallRate>(xml).Cast<RateEntity>().ToList();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var api = new CbrApi();

            var startDate = new DateTime(2010, 1, 1);
            var endDate = DateTime.Now;

            var rates = new Dictionary<string, List<RateEntity>>();

            var valutes = GetValutes(api, "USD", "GBP", "JPY", "CHF", "EUR", "CNY", "TRY");
            foreach (var valute in valutes) rates[valute.Id] = api.GetValuteRates(valute.Id, startDate, endDate);

            var metals = api.GetMetallRates(startDate, endDate);
            var metCodes = metals.Select(x => x.Id).Distinct().ToArray();
            foreach (var metCode in metCodes) rates[metCode] = metals.Where(x => x.Id == metCode).ToList();
        }

        private static List<Valute> GetValutes(CbrApi api, params string[] codes)
        {
            var valutesList = api.GetValutesList();
            return codes.Select(code => valutesList.FirstOrDefault(x => x.IsoCharCode == code)).Where(valute => valute != null).ToList();
        }
    }
}
