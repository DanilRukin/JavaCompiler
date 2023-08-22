using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.Domain
{
    public static class TestEngineModule
    {
        public static IServiceCollection AddTestEngine(this IServiceCollection services)
        {
            services.AddMediatR(conf => { conf.RegisterServicesFromAssembly(typeof(TestEngineModule).Assembly); });
            return services;
        }
    }
}
