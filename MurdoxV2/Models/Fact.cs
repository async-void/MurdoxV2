using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class Fact
    {
        public int Id { get; set; }
        public required string FactUrl { get; set; }//https://www.thefactsite.com/1000-interesting-facts/
        public required string Content { get; set; }
        public string? Category { get; set; }
    }
}
