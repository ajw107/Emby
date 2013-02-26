﻿using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Kernel;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.System;
using MediaBrowser.Networking.HttpServer;
using ServiceStack.ServiceHost;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MediaBrowser.Api
{
    /// <summary>
    /// Class GetSystemInfo
    /// </summary>
    [Route("/System/Info", "GET")]
    public class GetSystemInfo : IReturn<SystemInfo>
    {

    }

    /// <summary>
    /// Class RestartApplication
    /// </summary>
    [Route("/System/Restart", "POST")]
    [ServiceStack.ServiceHost.Api(("Restarts the application, if needed"))]
    public class RestartApplication
    {
    }

    [Route("/System/Shutdown", "POST")]
    public class ShutdownApplication
    {
    }
    
    /// <summary>
    /// Class GetConfiguration
    /// </summary>
    [Route("/System/Configuration", "GET")]
    public class GetConfiguration : IReturn<ServerConfiguration>
    {

    }

    /// <summary>
    /// Class UpdateConfiguration
    /// </summary>
    [Route("/System/Configuration", "POST")]
    public class UpdateConfiguration : IRequiresRequestStream
    {
        /// <summary>
        /// The raw Http Request Input Stream
        /// </summary>
        /// <value>The request stream.</value>
        public Stream RequestStream { get; set; }
    }

    /// <summary>
    /// Class SystemInfoService
    /// </summary>
    public class SystemService : BaseRestService
    {
        /// <summary>
        /// The _json serializer
        /// </summary>
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// The _app host
        /// </summary>
        private readonly IApplicationHost _appHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemService" /> class.
        /// </summary>
        /// <param name="jsonSerializer">The json serializer.</param>
        /// <param name="appHost">The app host.</param>
        /// <exception cref="System.ArgumentNullException">jsonSerializer</exception>
        public SystemService(IJsonSerializer jsonSerializer, IApplicationHost appHost)
            : base()
        {
            if (jsonSerializer == null)
            {
                throw new ArgumentNullException("jsonSerializer");
            }
            if (appHost == null)
            {
                throw new ArgumentNullException("appHost");
            }

            _appHost = appHost;
            _jsonSerializer = jsonSerializer;
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetSystemInfo request)
        {
            var result = Kernel.GetSystemInfo();

            return ToOptimizedResult(result);
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetConfiguration request)
        {
            var kernel = (Kernel)Kernel;

            var dateModified = File.GetLastWriteTimeUtc(Kernel.ApplicationPaths.SystemConfigurationFilePath);

            var cacheKey = (Kernel.ApplicationPaths.SystemConfigurationFilePath + dateModified.Ticks).GetMD5();

            return ToOptimizedResultUsingCache(cacheKey, dateModified, null, () => kernel.Configuration);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(RestartApplication request)
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                Kernel.PerformPendingRestart();
            });
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(ShutdownApplication request)
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                _appHost.Shutdown();
            });
        }

        /// <summary>
        /// Posts the specified configuraiton.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(UpdateConfiguration request)
        {
            var serverConfig = _jsonSerializer.DeserializeFromStream<ServerConfiguration>(request.RequestStream);

            var kernel = (Kernel)Kernel;

            kernel.UpdateConfiguration(serverConfig);
        }
    }
}
