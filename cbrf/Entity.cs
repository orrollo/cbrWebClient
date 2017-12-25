using System.Collections.Generic;

namespace cbrf
{
    public class Entity : Dictionary<string, string>
    {
        protected string Get(string name, string defaultValue)
        {
            return this.ContainsKey(name) ? this[name] : defaultValue;
        }
    }
}