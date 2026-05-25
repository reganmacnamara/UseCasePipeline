using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UseCasePipeline.Exceptions
{
    public class UseCaseConfigurationException(string message) : Exception(message)
    {

    }
}
