using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageGallery.Models
{
    public class SearchResultComparer : IEqualityComparer<AzureSearchResultModel>
    {
        public bool Equals(AzureSearchResultModel x, AzureSearchResultModel y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(AzureSearchResultModel obj)
        {
            throw new NotImplementedException();
        }
    }
}