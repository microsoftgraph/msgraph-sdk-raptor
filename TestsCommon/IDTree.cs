using System.Collections.Generic;

namespace TestsCommon
{
    public class IDTree : Dictionary<string, IDTree>
    {
        public string Value { get; set; }
        public IDTree(string value) { Value = value; }
    }
}

