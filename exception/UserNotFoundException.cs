using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.exception
{
    public class UserNotFoundException : System.Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}

namespace OrderManagementSystem
{
    [Serializable]
    class ProductNotFoundException : Exception
    {
        public ProductNotFoundException()
        {
        }

        public ProductNotFoundException(string? message) : base(message)
        {
        }

        public ProductNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}