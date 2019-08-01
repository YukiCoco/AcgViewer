using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpKonachan.Models
{
    public class Artist
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? alias_id { get; set; }
        public object group_id { get; set; }
        public List<object> urls { get; set; }
    }
}
