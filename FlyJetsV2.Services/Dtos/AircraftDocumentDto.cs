using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services.Dtos
{
    public class AircraftDocumentDto
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public byte Type { get; set; }
        public byte Order { get; set; }
    }
}
