﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableHotspotWindows
{
    public class Settings
    {
        public String CheckValidSettings { get; set; }
        public Boolean EnableLogging { get; set; }
        public Boolean StartHotspotAutomatically { get; set; }
        public Boolean StartMinimized { get; set; }
        public Boolean ShowMessageMinimized { get; set; }
    }
}
