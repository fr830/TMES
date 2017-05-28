using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities
{
    public class Exploder
    {
        public string id_record { get; set; }
        public string Type { get; set; }
        public string Ind { get; set; }
        public string Denotation { get; set; }
        public string Amount { get; set; }
        public string Depth { get; set; }
    }

    public class WorkDetail
    {
        public string Ind { get; set; }
        public string Denotation { get; set; }
        public string Chain { get; set; }
        public string Department { get; set; }
        public  string Duration { get; set; }
        public string Operation { get; set; }
    }

    public class StandartWorks
    {
        public  string id { get; set; }
        public string NameWork { get; set; }
        public string Duration { get; set; }
    }
    
}
