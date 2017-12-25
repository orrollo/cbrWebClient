using System;
using System.Globalization;

namespace cbrf
{
    public abstract class RateEntity : IdEntity, ICloneable
    {
        public decimal Rate
        {
            get { return GetRate(); }
            set { SetRate(value); }
        }

        protected abstract void SetRate(decimal value);

        public DateTime Date { get { return DateTime.ParseExact(Get("DATE", ""),"dd.MM.yyyy",CultureInfo.InvariantCulture); } }
        protected abstract decimal GetRate();

        public override string ToString()
        {
            return "Id={0};Date={1};Rate={2}".Fmt(Id, Date, Rate);
        }

        public abstract object Clone();
    }
}