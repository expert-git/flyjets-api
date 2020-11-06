using System;
using System.Collections.Generic;
using System.Text;

namespace FlyJetsV2.Services
{
    public class ServiceOperationResult<T>: ServiceOperationResult
    {
        public T Item { get; set; }
    }

    public class ServiceOperationResult
    {
        public bool IsSuccessfull { get; set; }
        public List<ErrorCodes> Errors { get; set; }
    }
}
