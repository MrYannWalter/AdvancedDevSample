using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedDevSample.Infrastructure.Exceptions
{
    public class InfrastructureException : Exception
    {
        public InfrastructureException() { }

        public InfrastructureException(string message) : base(message) { }

        public InfrastructureException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
