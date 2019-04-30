//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharePointPnP.ProvisioningApp.Infrastructure.ADAL
{
    public class WebCacheADALCache : TokenCache
    {
        private static readonly object FileLock = new object();
        string UserObjectId = string.Empty;
        string CacheId = string.Empty;

        public WebCacheADALCache(string userId)
        {
            UserObjectId = userId;
            CacheId = UserObjectId + "_TokenCache";
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            this.BeforeWrite = BeforeWriteNotification;

            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                if (HttpContext.Current != null &&
                    HttpContext.Current.Session[CacheId] != null)
                {
                    this.Deserialize((byte[])HttpContext.Current.Session[CacheId]);
                }
            }
        }

        public void Persist()
        {
            lock (FileLock)
            {
                if (HttpContext.Current != null)
                {
                    // reflect changes in the persistent store             
                    HttpContext.Current.Session[CacheId] = this.Serialize();

                    // once the write operation took place, restore the HasStateChanged bit to false             
                    this.HasStateChanged = false;
                }
            }
        }

        // Empties the persistent store.     
        public override void Clear()
        {
            base.Clear();
            System.Web.HttpContext.Current.Session.Remove(CacheId);
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            Persist();
        }

        // Triggered right before ADAL needs to access the cache.     
        // Reload the cache from the persistent store in case it changed since the last access.      
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after ADAL accessed the cache.     
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update         
            if (this.HasStateChanged)
            {
                Persist();
            }
        }

        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }
    }
}