using Entities.Models;
using Entities.ViewModels;
using API_CORE.Controllers.Models.Label;
using API_CORE.Controllers.Models.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ILabelRepository
    {
        public Task<List<LabelListingModel>> Listing(int status = -1, string label_name = null,string label_code = null, int page_index = 1, int page_size = 100);
    }
}
