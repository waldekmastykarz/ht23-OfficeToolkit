using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace HttpSample.Function.SharePoint.Services
{
    public interface ISharePointClient
    {
        Task<GraphServiceClient> GetGraphAPIClient();
    }
}
