using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackSim
{
    public struct Message
    {
        public string MessageCode { get; set;  }
        public string Reciever {  get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public Message(string code, string reciever, Dictionary<string, object> paras)
        {
            MessageCode = code;
            Reciever= reciever;
            Parameters = paras;
        }
    }
}
