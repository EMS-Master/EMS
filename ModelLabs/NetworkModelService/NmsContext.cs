using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using FTN.Services.NetworkModelService.NmsModel;

namespace FTN.Services.NetworkModelService
{
    public class NmsContext : DbContext
    {
        public DbSet<DeltaModel> DeltaModels { get; set; }

    }
}
