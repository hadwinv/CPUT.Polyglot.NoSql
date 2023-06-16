using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Common.Helpers.NodeExpressions
{
    public class Tree
    {
        public string? Id { get; set; }
        
        public string Name { get; set; }

        public string Cardinality { get; set; }


        private Tree _parent;
        public Tree Parent 
        { 
            get 
            { 
                return _parent; 
            } 
        }

        private List<Tree> _children;
        public IEnumerable<Tree> Children
        {
            get 
            { 
                return _children == null ? Enumerable.Empty<Tree>() : _children.ToArray(); 
            }
        }

        public Tree Root { get { return _parent == null ? this : _parent.Root; } }

        public Tree()
        {
            Name = string.Empty;
        }

        public void Add(Tree child)
        {
            if (child == null)
                throw new ArgumentNullException();
            
            if (child._parent != null)
                throw new InvalidOperationException("A tree node must be removed from its parent before adding as child.");
            
            if (this.Ancestors().Contains(child))
                throw new InvalidOperationException("A tree cannot be a cyclic graph.");
            
            if (_children == null)
                _children = new List<Tree>();
           
            child._parent = this;

            _children.Add(child);
        }

        public bool Remove(Tree child)
        {
            if (child == null)
                throw new ArgumentNullException();
            if (child._parent != this)
                return false;
            Debug.Assert(_children.Contains(child), "At this point, the node is definately a child");
            child._parent = null;
            _children.Remove(child);
            if (!_children.Any())
                _children = null;
            return true;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
