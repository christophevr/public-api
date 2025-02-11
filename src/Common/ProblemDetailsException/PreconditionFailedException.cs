namespace Common.ProblemDetailsException
{
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Microsoft.AspNetCore.Http;

    public class PreconditionFailedException : ApiProblemDetailsException
    {
        public string RegistryName { get; }

        public PreconditionFailedException(string message, string registryName) : base(message, StatusCodes.Status412PreconditionFailed, new ExceptionProblemDetails(), null)
        {
            RegistryName = registryName;
        }
    }
}
