using NuGetLite.Server.Core;
using System.Collections.Generic;

namespace NuGetLite.Server.Models
{
    public class QueryResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResponse"/> class
        /// </summary>
        /// <param name="totalHits"></param>
        /// <param name="data"></param>
        public QueryResponse(int totalHits, IEnumerable<NuGetPackageSummary> data)
        {
            this.TotalHits = totalHits;
            this.Data = data;
        }

        /// <summary>
        /// Gets or sets the total hits
        /// </summary>
        public int TotalHits
        { get; private set; }

        /// <summary>
        /// Gets or sets the data
        /// </summary>
        public IEnumerable<NuGetPackageSummary> Data
        { get; private set; }
    }
}
