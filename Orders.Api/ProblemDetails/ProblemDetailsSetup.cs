using Microsoft.AspNetCore.Mvc;

namespace Orders.Api.ProblemDetails;

public static class ProblemDetailsSetup
{
    public static void AddCustomProblemDetails(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                // Only show full exception detail in dev
                if (!env.IsDevelopment() && ctx.ProblemDetails.Detail?.Length > 0)
                {
                    ctx.ProblemDetails.Detail = null;
                }
                ctx.ProblemDetails.Extensions["service"] = "Orders.Api";
                ctx.ProblemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
            };
        });
    }
}
