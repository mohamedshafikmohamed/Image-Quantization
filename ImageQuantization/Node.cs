
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{
    public class Node
    {
        public int index;
        public double weight;
        public int to;
        public Node() { }
        public void set_key(double val)
        {
            this.weight = val;
        }
    }
}
