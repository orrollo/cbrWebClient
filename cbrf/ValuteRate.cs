using System.Globalization;

namespace cbrf
{
    public class ValuteRate : RateEntity
    {
        public int Nominal { get { return int.Parse(Get("NOMINAL", "1")); } }
        public decimal Value { get { return decimal.Parse(Get("VALUE", "1").Replace(',','.'), NumberStyles.Any, CultureInfo.InvariantCulture); } }

        protected override void SetRate(decimal value)
        {
            this["VALUE"] = (value*Nominal).ToString(CultureInfo.InvariantCulture);
        }

        protected override decimal GetRate()
        {
            return Value/Nominal;
        }

        public override object Clone()
        {
            return CopyDictionary(new ValuteRate());
        }
    }
}