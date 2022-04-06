﻿using System;

using IPinfo.Http.Client;
using IPinfo.Apis;
using IPinfo.Cache;
using IPinfo.Utilities;

namespace IPinfo
{
    /// <summary>
    /// The gateway for IPinfo SDK. This class holds the configuration of the SDK.
    /// </summary>
    public sealed class IPinfoClient
    {
        private readonly IHttpClient _httpClient;
        private readonly CacheHandler _cacheHandler;
        private readonly Lazy<IPApi> _ipApi;

        private IPinfoClient(
            string accessToken,
            IHttpClient httpClient,
            CacheHandler cacheHandler,
            IHttpClientConfiguration httpClientConfiguration)
        {
            this._httpClient = httpClient;
            this._cacheHandler = cacheHandler;
            this.HttpClientConfiguration = httpClientConfiguration;
            
            this._ipApi = new Lazy<IPApi>(
                () => new IPApi(this._httpClient, accessToken, cacheHandler));

            CountryHelper.Init();
        }

        /// <summary>
        /// Gets IPApi.
        /// </summary>
        public IPApi IPApi => this._ipApi.Value;
        
        /// <summary>
        /// Gets the configuration of the Http Client associated with this client.
        /// </summary>
        public IHttpClientConfiguration HttpClientConfiguration { get; }

        /// <summary>
        /// Builder class.
        /// </summary>
        public class Builder
        {
            private string _accessToken = "";
            private HttpClientConfiguration.Builder _httpClientConfig = new HttpClientConfiguration.Builder();
            private IHttpClient _httpClient;
            private CacheHandler _cacheHandler = new CacheHandler();

            /// <summary>
            /// Sets credentials for BearerAuth.
            /// </summary>
            /// <param name="accessToken">AccessToken.</param>
            /// <returns>Builder.</returns>
            public Builder AccessToken(string accessToken)
            {
                this._accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
                return this;
            }

            /// <summary>
            /// Sets HttpClientConfig.
            /// </summary>
            /// <param name="action"> Action. </param>
            /// <returns>Builder.</returns>
            public Builder HttpClientConfig(Action<HttpClientConfiguration.Builder> action)
            {
                if (action is null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                action(this._httpClientConfig);
                return this;
            }

            /// <summary>
            /// Sets the ICache implementation for the Builder.
            /// </summary>
            /// <param name="cache"> ICache implementation. Pass null to disable the cache.</param>
            /// <returns>Builder.</returns>
            public Builder Cache(ICache cache)
            {
                // Null is allowed here, which is being used to indicate that user do not want the cache.
                if(cache == null)
                {
                    this._cacheHandler = null;
                }
                else
                {
                    this._cacheHandler = new CacheHandler(cache);
                }
                return this;
            }

            /// <summary>
            /// Creates an object of the IPinfoClient using the values provided for the builder.
            /// </summary>
            /// <returns>IPinfoClient.</returns>
            public IPinfoClient Build()
            {
                this._httpClient = new HttpClientWrapper(this._httpClientConfig.Build());

                return new IPinfoClient(
                    this._accessToken,
                    this._httpClient,
                    this._cacheHandler,
                    this._httpClientConfig.Build());
            }
        }
    }
}
