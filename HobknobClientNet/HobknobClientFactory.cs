﻿using System;

namespace HobknobClientNet
{
    public interface IHobknobClientFactory
    {
        IHobknobClient Create(string etcdHost, int etcdPort, string applicationName, TimeSpan cacheUpdateInterval, EventHandler<CacheUpdateFailedArgs> cacheUpdateFailed);
    }

    public class HobknobClientFactory : IHobknobClientFactory
    {
        public IHobknobClient Create(string etcdHost, int etcdPort, string applicationName, TimeSpan cacheUpdateInterval, EventHandler<CacheUpdateFailedArgs> cacheUpdateFailed)
        {
            var etcdKeysPath = string.Format("http://{0}:{1}/v2/keys/", etcdHost, etcdPort);

            var etcdClient = new EtcdClient(new Uri(etcdKeysPath));
            var featureToggleProvider = new FeatureToggleProvider(etcdClient, applicationName);
            var featureToggleCache = new FeatureToggleCache(featureToggleProvider, cacheUpdateInterval);
            var hobknobClient = new HobknobClient(featureToggleCache, applicationName);
            if (cacheUpdateFailed == null)
                throw new ArgumentNullException("cacheUpdateFailed", "Cached update handler is emtpy");
            featureToggleCache.CacheUpdateFailed += cacheUpdateFailed;

            featureToggleCache.Initialize();

            return hobknobClient;
        }
    }
}
