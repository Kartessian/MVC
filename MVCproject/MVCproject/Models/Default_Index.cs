using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCproject
{
    public class Default_Index
    {
        public List<MapStyle> styles { get; set; } // available map styles for all users
        public List<UserMaps> maps { get; set; } // current existing maps for the user
        public List<MapDataset> datasets { get; set; } // datasets created by the user
    }
}