using FlyJetsV2.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FlyJetsV2.Services
{
    public class FeeTypeService
    {
        private IConfiguration _config;

        public FeeTypeService(IConfiguration config)
        {
            _config = config;
        }

        internal List<FeeType> GetFeesTypes()
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return dbContext.FeesTypes
                    .OrderBy(ft => ft.Order)
                    .ToList();
            }
        }
    }
}
