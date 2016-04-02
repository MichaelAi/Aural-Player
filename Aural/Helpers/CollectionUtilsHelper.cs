using Aural.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aural.Helpers
{
    public static class CollectionUtilsHelper
    {
        public static ObservableCollection<PlaylistItem> ToObservableCollection<T>(this IList<PlaylistItem> items)
        {
            return new ObservableCollection<PlaylistItem>(items);
        }
    }
}
