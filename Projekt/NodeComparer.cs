using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt
{
    class NodeComparer : IEqualityComparer<HtmlNode> 
    {
        public bool Equals(
            HtmlNode x, 
            HtmlNode y)
        {
            return HtmlNode.Equals(x,y);
        }
        public int GetHashCode(HtmlNode node)
        {
            return node.Name.GetHashCode();
        }
    }
}
