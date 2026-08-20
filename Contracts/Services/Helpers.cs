using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace Contracts.Services
{
    public static class Helpers
    {
        public static string GetStringSafe(DbDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? string.Empty : r.GetFieldValue<string>(i);
        }
    }
}
