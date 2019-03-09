using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoDeployer.FunctionApp.Tests
{
    class PagedList<T> : List<T>, IPagedList<T>
    {
        public string ContinuationToken => throw new NotImplementedException();
    }
}
