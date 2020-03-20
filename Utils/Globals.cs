using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExportTemplates.Utils
{
    public sealed class Globals
    { 
        private Dictionary<string, Object> GlobalsMap = null;

        private static readonly Globals instance = new Globals();

        static Globals() { }

        private Globals()
        {
            GlobalsMap = new Dictionary<string, object>();    
        }

        public void SetData (string key, Object value)
        {
            GlobalsMap[key] = value;
        }

        public Object GetData (string key)
        {
            return GlobalsMap[key];
        }

        
        public bool ContainsKey(string key)
        {
            return GlobalsMap.ContainsKey(key);
        }

        public static Globals Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
