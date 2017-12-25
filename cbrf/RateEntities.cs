using System;
using System.Collections.Generic;
using System.Linq;

namespace cbrf
{
    public class RateEntities : List<RateEntity>
    {
        public string Id { get; set; }

        public RateEntities(string id) : base()
        {
            Id = id;
        }

        public RateEntities(string id, IEnumerable<RateEntity> src) : base(src)
        {
            Id = id;
        }

        public void DateSort()
        {
            Sort((a,b)=>a.Date.CompareTo(b.Date));
        }

        public RateEntities GetDiffList()
        {
            var ret = new RateEntities("D_{0}".Fmt(Id));
            //var dates = GetDates();
            RateEntity prev = null;
            //foreach (DateTime date in dates)
            for (int i=0;i<this.Count;i++)
            {
                var cur = this[i];
                RateEntity item = (RateEntity) cur.Clone();
                item.Rate = prev == null ? 0 : cur.Rate - prev.Rate;
                ret.Add(item);
                prev = cur;
            }
            ret.DateSort();
            return ret;
        }

        public List<DateTime> GetDates()
        {
            var dates = this.Select(x => x.Date).ToList();
            dates.Sort();
            return dates;
        }

        public RateEntities GetAverageList(int days)
        {
            int idx = 0, n = 0;

            var ret = new RateEntities("AV{1}_{0}".Fmt(Id, days));
            //var dates = GetDates();
            decimal sum = 0;
            for (int i = 0; i < Count; i++)
            {
                int j = i - days;
                if (j >= 0)
                {
                    var cur = this[j];
                    sum -= cur.Rate;
                    n--;
                }
                var entity = this[i];
                var item = (RateEntity)entity.Clone();
                sum += entity.Rate;
                if (n < days) n++;
                item.Rate = sum / n;
                ret.Add(item);
            }
            ret.DateSort();
            return ret;
        }
    }
}