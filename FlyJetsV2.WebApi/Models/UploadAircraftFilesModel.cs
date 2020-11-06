using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyJetsV2.WebApi.Models
{
    public class UploadAircraftFilesModel
    {
        public List<AircraftFile> Photos { get; set; }
        public List<AircraftFile> AirwothinessCertificates { get; set; }
        public List<AircraftFile> InsuranceDocuments { get; set; }
    }

    public class AircraftFile
    {
        public string Name { get; set; }
        public IFormFile File { get; set; }
    }
}
