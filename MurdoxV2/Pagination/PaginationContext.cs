using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Pagination
{
    public sealed class PaginationContext<T>
    {
        public IReadOnlyList<T> Pages { get; set; }
        public int CurrentIndex { get; set; }
        public string ContextType { get; set; }

        public PaginationContext(IReadOnlyList<T> pages, int currentIndex, string contextType)
        {
            Pages = pages;
            CurrentIndex = currentIndex;
            ContextType = contextType;
        }

        public bool HasNext => CurrentIndex < Pages.Count - 1;
        public bool HasPrevious => CurrentIndex > 0;
    }
}
