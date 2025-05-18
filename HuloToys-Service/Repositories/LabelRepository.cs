using DAL;
using Entities.ConfigModels;
using Entities.Models;
using API_CORE.Controllers.Models.Label;
using API_CORE.Controllers.Models.Models;
using API_CORE.Controllers.Utilities.Lib;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace Repositories.IRepositories
{
    public class LabelRepository : ILabelRepository
    {
        private readonly LabelDAL labelDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public LabelRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            labelDAL = new LabelDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }

        public async Task<List<LabelListingModel>> Listing(int status = -1, string label_name = null,string label_code = null, int page_index = 1, int page_size = 100)
        {
            return await labelDAL.Listing(status,label_name, label_code, page_index,page_size);
        }
    }
}
