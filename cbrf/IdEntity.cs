using System.Collections.Generic;

namespace cbrf
{
    public class IdEntity : Entity
    {
        public string Id
        {
            get { return GetId(); }
        }

        protected virtual string GetId()
        {
            return Get("ID", "").Trim();
        }

        public override string ToString()
        {
            return "Id={0}".Fmt(Id);
        }

        protected object CopyDictionary(IDictionary<string,string> ret)
        {
            foreach (var pair in this) ret.Add(pair.Key, pair.Value);
            return ret;
        }
    }
}