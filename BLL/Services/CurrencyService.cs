using System;
using System.Collections.Generic;
using System.Text;

using System.Globalization;
using BLL.Interfaces;

namespace BLL.Services
{
    public class CurrencyService : ICurrencyService
    {
        public string Format(decimal amount)
        {
            // Uses the current thread culture (ar-AE) set at application startup
            // This formats the amount according to AED currency standards with proper symbol placement
            return amount.ToString("C", CultureInfo.CurrentCulture);
        }
    }
}
