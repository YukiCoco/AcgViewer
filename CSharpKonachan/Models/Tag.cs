using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpKonachan.Models
{
    public class Tag
    {
        public int id { get; set; }
        public string name { get; set; }
        public int count { get; set; }
        public int type { get; set; }
        public bool ambiguous { get; set; }
    }
}
