using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedDevSample.Application.DTOs.Product.ProductRequests
{
    public class ChangePriceRequest
    {
        [Range(0.01, double.MaxValue)]
        public decimal NewPrice { get; set; }
    }
}
