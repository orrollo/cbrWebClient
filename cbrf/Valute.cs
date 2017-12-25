namespace cbrf
{
    public class Valute : IdEntity
    {
        public string Name { get { return Get("NAME", ""); } }
        public int Nominal { get { return int.Parse(Get("NOMINAL", "1")); } }
        public string EngName { get { return Get("ENGNAME", ""); } }
        public string ParentCode { get { return Get("PARENTCODE", "").Trim(); } }
        public int IsoNumCode { get { return int.Parse(Get("ISO_NUM_CODE", "-1")); } }
        public string IsoCharCode { get { return Get("ISO_CHAR_CODE", ""); } }
    }
}