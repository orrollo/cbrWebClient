using System.Collections.Generic;
using System.Globalization;

namespace cbrf
{
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

        public string Code
        {
            get { return Get("CODE","0"); }
        }

        public decimal Buy
        {
            get
            {
                return decimal.Parse(Get("BUY", "1").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            }
        }

        public decimal Sell
        {
            get
            {
                return decimal.Parse(Get("SELL", "1").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            }
        }

        protected override void SetRate(decimal value)
        {
            this["BUY"] = value.ToString(CultureInfo.InvariantCulture);
            this["SELL"] = value.ToString(CultureInfo.InvariantCulture);
        }

        protected override decimal GetRate()
        {
            return Sell;
        }

        public override object Clone()
        {
            return CopyDictionary(new MetallRate());
        }
    }
}