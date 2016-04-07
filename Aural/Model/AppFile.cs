using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aural.Model
{
    public class AppFile
    {
        public string Name { get; set; }
        public StorageFile File { get; set; }
    }
}
