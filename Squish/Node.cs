using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squish
{
    class Node
    {
        public Node Parent;
        public List<Node> Children = new List<Node>();

        public List<string> Words = new List<string>();

        public Node CreateChild()
        {
            var child = new Node();
            child.Parent = this;
            Children.Add(child);
            return child;
        }
        
        public override string ToString()
        {
            var output = new StringBuilder();
            ToString(output);
            return output.ToString();
        }

        protected void ToString(StringBuilder output)
        {
            output.Append("(");
            foreach (var word in Words)
            {
                output.Append(word);
                output.Append(" ");
            }
            output.Append(":");
            foreach (var child in Children)
            {
                child.ToString(output);
            }
            output.Append(")");
        }
    }
}
