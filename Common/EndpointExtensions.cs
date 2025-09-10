using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace VerticalSliceArchitecture.Common
{
    public static class EndpointExtensions
    {
        /// <summary>
        /// Finds all static classes in the assembly that contain a public static --Map-- method
        /// and automatically invokes them. This allows each feature slice to register its
        /// own Minimal API endpoints without touching Program.cs.
        /// </summary>
        public static void MapEndpoints(this IEndpointRouteBuilder app)
        {
            var types = typeof(Program).Assembly.GetTypes()
                .Where(t => t.IsClass && t.IsAbstract && t.IsSealed) // static class
                .Where(t => t.GetMethod("Map", BindingFlags.Public | BindingFlags.Static) != null);

            foreach (var type in types)
            {
                var mapMethod = type.GetMethod("Map", BindingFlags.Public | BindingFlags.Static);
                mapMethod?.Invoke(null, new object[] { app });
            }
        }
    }
}
