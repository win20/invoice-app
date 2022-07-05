using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoicingApp
{
    public class JobItem
    {
        public StringCollection descLines;
        public string description;
        public string qty;
        public string rate;
        public string price;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public JobItem() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public JobItem(StringCollection descLines, string description, string qty, string rate, string price)
        {
            this.descLines = descLines;
            this.description = description;
            this.qty = qty;
            this.rate = rate;
            this.price = price;
        }

        public override string ToString()
        {
            return this.descLines + ", " + this.description + ", " + this.qty + ", " + this.rate + ", " + this.price;
        }
    }
}
