using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.entity
{
     public class Electronics : Product
    {
        public string Brand { get; set; }
        public int WarrantyPeriod { get; set; }

        // Constructor with all parameters
        public Electronics(int productId, string productName, string description, double price, int quantityInStock, string type, string brand, int warrantyPeriod)
            : base(productId, productName, description, price, quantityInStock, type)
        {
            Brand = brand;
            WarrantyPeriod = warrantyPeriod;
        }

        // Optional: Override ToString() method for better debugging output
        public override string ToString()
        {
            return $"{base.ToString()}, Brand: {Brand}, Warranty Period: {WarrantyPeriod} months";
        }
    }
}
