using FlyJetsV2.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FlyJetsV2.Services
{
    public class TaxTypeService
    {
        private IConfiguration _config;

        public TaxTypeService(IConfiguration config)
        {
            _config = config;
        }

        internal List<TaxType> GetTaxesTypes()
        {
            using (FlyJetsDbContext dbContext = new FlyJetsDbContext(_config))
            {
                return dbContext.TaxesTypes
                    .OrderBy(ft => ft.Order)
                    .ToList();
            }
        }
    }
}
