// Infrastructure/FeatureFlagInitializer.cs

using VerticalSliceArchitecture.Domain;

namespace VerticalSliceArchitecture.Infrastructure;

public static class FeatureFlagInitializer
{
    public static void SeedFeatureFlags(AppDbContext context)
    {
        // Check if feature flags already exist to prevent re-seeding
        //var existingFlags = context.FeatureFlags.ToList();
        //context.FeatureFlags.RemoveRange(existingFlags);
        //context.SaveChanges();

        if (context.FeatureFlags.Any())
        {
            return;
        }

        var featureFlags = new List<FeatureFlag>
        {
            // Flags for 'admin' user type
            new FeatureFlag { Name = "CreateProductEnabled", UserType = "admin", IsEnabled = true },
            new FeatureFlag { Name = "UpdateProductEnabled", UserType = "admin", IsEnabled = true },
            new FeatureFlag { Name = "DeleteProductEnabled", UserType = "admin", IsEnabled = true },
            new FeatureFlag { Name = "GetAllProductsEnabled", UserType = "admin", IsEnabled = true },
            new FeatureFlag { Name = "GetProductByIdEnabled", UserType = "admin", IsEnabled = true },
            new FeatureFlag { Name = "GetAllFeatureFlagsEnabled", UserType = "admin", IsEnabled = true },
            new FeatureFlag { Name = "UpdateFlagStatusEnabled", UserType = "admin", IsEnabled = true },

            // Flags for 'user' user type
            new FeatureFlag { Name = "CreateProductEnabled", UserType = "user", IsEnabled = false },
            new FeatureFlag { Name = "UpdateProductEnabled", UserType = "user", IsEnabled = true },
            new FeatureFlag { Name = "DeleteProductEnabled", UserType = "user", IsEnabled = false },
            new FeatureFlag { Name = "GetAllProductsEnabled", UserType = "user", IsEnabled = true },
            new FeatureFlag { Name = "GetProductByIdEnabled", UserType = "user", IsEnabled = false },
            new FeatureFlag { Name = "GetAllFeatureFlagsEnabled", UserType = "user", IsEnabled = false },
            new FeatureFlag { Name = "UpdateFlagStatusEnabled", UserType = "user", IsEnabled = false },

            // Flags for 'default' user type
            new FeatureFlag { Name = "CreateProductEnabled", UserType = "default", IsEnabled = false },
            new FeatureFlag { Name = "UpdateProductEnabled", UserType = "default", IsEnabled = false },
            new FeatureFlag { Name = "DeleteProductEnabled", UserType = "default", IsEnabled = false },
            new FeatureFlag { Name = "GetAllProductsEnabled", UserType = "default", IsEnabled = true },
            new FeatureFlag { Name = "GetProductByIdEnabled", UserType = "default", IsEnabled = false },
            new FeatureFlag { Name = "GetAllFeatureFlagsEnabled", UserType = "default", IsEnabled = false },
            new FeatureFlag { Name = "UpdateFlagStatusEnabled", UserType = "default", IsEnabled = false }
        };

        context.FeatureFlags.AddRange(featureFlags);
        context.SaveChanges();
    }
}