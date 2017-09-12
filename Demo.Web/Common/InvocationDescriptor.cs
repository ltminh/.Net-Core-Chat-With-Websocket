using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Demo.Web.Common
{
    public class InvocationDescriptor
    {
        [JsonProperty("methodName")]
        public string MethodName { get; set; }

        [JsonProperty("arguments")]
        public object[] Arguments { get; set; }
    }
}
