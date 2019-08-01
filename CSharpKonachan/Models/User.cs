using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpKonachan.Models
{
    public class User
    {
        public string name { get; set; }
        public List<object> blacklisted_tags { get; set; }
        public int id { get; set; }
    }
}
