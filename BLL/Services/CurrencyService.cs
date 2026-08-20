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
            return amount.ToString("C", CultureInfo.CurrentCulture);
        }
    }
}
