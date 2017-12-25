using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace cbrf
{
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

        public RateEntities GetValuteRates(string valuteId, DateTime startDate, DateTime endDate)
        {
            var xml = DownloadXml("dynamic.asp?date_req1={0}&date_req2={1}&VAL_NM_RQ={2}".Fmt(d2s(startDate),d2s(endDate),valuteId));
            return new RateEntities(valuteId, ParseXml<ValuteRate>(xml));
            //return ParseXml<ValuteRate>(xml).Cast<RateEntity>().ToList();
        }

        private string DownloadXml(string restUrl)
        {
            using (var client = new WebClient())
            {
                var xml = client.DownloadString(GetUrl(restUrl));
                return xml;
            }
        }

        public List<RateEntities> GetMetallRates(DateTime startDate, DateTime endDate)
        {
            var xml = DownloadXml("metall.asp?date_req1={0}&date_req2={1}".Fmt(d2s(startDate), d2s(endDate)));
            var list = ParseXml<MetallRate>(xml).Cast<RateEntity>().ToList();

            var metCodes = list.Select(x => x.Id).Distinct().ToArray();
            return metCodes.Select(metCode => new RateEntities(metCode, list.Where(x => metCode == x.Id))).ToList();
        }
    }
}