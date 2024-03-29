﻿using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lyralabs.TempMailServer
{
    public static class ExtensionMethods
    {
        public static TService Resolve<TService>(this IServiceProvider serviceProvider, params object[] parameters)
        {
            var service = serviceProvider.GetService<TService>();

            if (service is null)
            {
                service = ActivatorUtilities.CreateInstance<TService>(serviceProvider, parameters);
            }

            return service;
        }
    }
}
