using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class Bank
    {
        public int Id { get; set; }
        public double Balance { get; set; }
        public double Deposit_Amount { get; set; }
        public double Withdraw_Amount { get; set; }
        public DateTimeOffset Deposit_Timestamp { get; set; }
        public DateTimeOffset Withdraw_Timestamp { get; set; }

        //Navigation Property One Bank to Many ServerMembers
        public ICollection<ServerMember> Members { get; set; } = [];
    }
}
