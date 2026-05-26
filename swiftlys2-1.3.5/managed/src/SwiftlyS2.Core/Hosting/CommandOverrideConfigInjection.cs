using Microsoft.Extensions.DependencyInjection;
using SwiftlyS2.Core.Models;

namespace SwiftlyS2.Core.Hosting;

internal static class CommandOverrideConfigInjection
{
    public static IServiceCollection AddCommandOverrideConfig( this IServiceCollection self )
    {
        _ = self.AddOptions<CommandOverrideConfig>()
            .BindConfiguration("CommandOverrides")
            .ValidateOnStart();

        return self;
    }
}
