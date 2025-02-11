namespace Common.Infrastructure.Extensions
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Controllers.Attributes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Headers;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Net.Http.Headers;

    public static class AcceptTypeExtensions
    {
        private static ApiException InvalidAcceptType => new ApiException("Ongeldig formaat.", StatusCodes.Status406NotAcceptable);

        public static string ToMimeTypeString(this AcceptType acceptType)
        {
            return acceptType switch
            {
                AcceptType.Json => AcceptTypes.Json,
                AcceptType.JsonLd => AcceptTypes.JsonLd,
                AcceptType.Xml => AcceptTypes.Xml,
                AcceptType.Atom => AcceptTypes.Atom,
                _ => throw new ArgumentOutOfRangeException(nameof(acceptType), acceptType, null)
            };
        }

        public static string ToProblemResponseMimeTypeString(this AcceptType acceptType)
        {
            return acceptType switch
            {
                AcceptType.Json => AcceptTypes.JsonProblem,
                AcceptType.JsonLd => AcceptTypes.JsonProblem,
                AcceptType.Xml => AcceptTypes.XmlProblem,
                AcceptType.Atom => AcceptTypes.XmlProblem,
                _ => throw new ArgumentOutOfRangeException(nameof(acceptType), acceptType, null)
            };
        }

        public static AcceptType DetermineAcceptType(
            this RequestHeaders requestHeaders,
            ActionDescriptor? actionDescriptor)
        {
            var acceptHeaders = requestHeaders.Accept;

            if (acceptHeaders == null || acceptHeaders.Count == 0)
                return AcceptType.Json;

            var headersByQuality = acceptHeaders
                .OrderByDescending(header => header, MediaTypeHeaderValueComparer.QualityComparer)
                .Where(header => header.MediaType.HasValue);

            foreach (var headerValue in headersByQuality)
            {
                if (headerValue.Contains(AcceptTypes.Atom))
                    return AcceptType.Atom;

                if (headerValue.Contains(AcceptTypes.Xml))
                    return AcceptType.Xml;

                if (headerValue.Contains(AcceptTypes.JsonProblem))
                    return AcceptType.JsonProblem;

                if (headerValue.Contains(AcceptTypes.JsonLd))
                    return AcceptType.JsonLd;

                if (headerValue.Contains(AcceptTypes.Json))
                    return AcceptType.Json;

                if (headerValue.Contains(AcceptTypes.Any))
                {
                    // We like to default to json,
                    // but we need to pick something the controller actually produces
                    if (!(actionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
                        return AcceptType.Json;

                    var producesAttribute = controllerActionDescriptor
                        .ControllerTypeInfo
                        .GetCustomAttributes(inherit: true)
                        .OfType<ApiProducesAttribute>()
                        .SingleOrDefault();

                    if (producesAttribute == null)
                        return AcceptType.Json;

                    foreach (var possibleContentType in producesAttribute.ContentTypes)
                    {
                        switch (possibleContentType)
                        {
                            case AcceptTypes.Atom:
                                return AcceptType.Atom;

                            case AcceptTypes.Xml:
                                return AcceptType.Xml;

                            case AcceptTypes.JsonLd:
                                return AcceptType.JsonLd;

                            case AcceptTypes.Json:
                                return AcceptType.Json;
                        }
                    }

                    return AcceptType.Json;
                }
            }

            throw InvalidAcceptType;
        }

        public static AcceptType ValidateFor(
            this AcceptType acceptType,
            EndpointType endpointType)
            => endpointType
                .GetAcceptedTypes()
                .Contains(acceptType)
                ? acceptType
                : throw InvalidAcceptType;

        public static bool Contains(
            this RequestHeaders requestHeaders,
            AcceptType acceptType)
            => requestHeaders
                .Accept
                .Any(headerValue => headerValue.Contains(acceptType.ToMimeTypeString()));

        private static bool Contains(
            this MediaTypeHeaderValue headerValue,
            string mimeType)
            => headerValue
                .MediaType
                .Value
                .Contains(mimeType, StringComparison.InvariantCultureIgnoreCase);
    }
}
