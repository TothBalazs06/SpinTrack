using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpinTrack
{
    // Helper class to define the structure of a record
    public class Record
    {
        public string Artist { get; set; }
        public string AlbumTitle { get; set; }
        public string ReleaseYear { get; set; }
        public string Category { get; set; }
        public string Length { get; set; }
        public string Quantity { get; set; }
        public bool HasOuterCover { get; set; }
        public bool HasInnerCover { get; set; }
        public string VinylQuality { get; set; }
        public string SleeveQuality { get; set; }
    }
}
