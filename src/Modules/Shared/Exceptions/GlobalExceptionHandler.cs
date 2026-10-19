using Microsoft.AspNetCore.Diagnostics;

namespace PeopleOnDuty.src.Modules.Shared.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandlerFeature
    {
        public Exception Error => throw new NotImplementedException();

    }
}