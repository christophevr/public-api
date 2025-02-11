namespace Public.Api.Parcel.Oslo
{
    using Autofac.Features.AttributeFilters;
    using Be.Vlaanderen.Basisregisters.Api;
    using Common.Infrastructure;
    using Common.Infrastructure.Controllers;
    using Common.Infrastructure.Controllers.Attributes;
    using FeatureToggle;
    using Infrastructure.Configuration;
    using Infrastructure.Swagger;
    using Infrastructure.Version;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using RestSharp;

    [ApiVisible]
    [ApiVersion(Version.V2)]
    [AdvertiseApiVersions(Version.V2)]
    [ApiRoute("")]
    [ApiExplorerSettings(GroupName = "Percelen")]
    [ApiOrder(Order = ApiOrder.Parcel)]
    [ApiProduces(EndpointType.Oslo)]
    public partial class ParcelOsloController : RegistryApiController<ParcelOsloController>
    {
        protected override string NotFoundExceptionMessage => "Onbestaand perceel.";
        protected override string GoneExceptionMessage => "Verwijderd perceel.";

        public ParcelOsloController(
            [KeyFilter(RegistryKeys.ParcelV2)] IRestClient restClient,
            [KeyFilter(RegistryKeys.ParcelV2)] IFeatureToggle cacheToggle,
            ConnectionMultiplexerProvider redis,
            ILogger<ParcelOsloController> logger)
            : base(restClient, cacheToggle, redis, logger) { }

        private static ContentFormat DetermineFormat(ActionContext context)
            => ContentFormat.For(EndpointType.Oslo, context);
    }
}
