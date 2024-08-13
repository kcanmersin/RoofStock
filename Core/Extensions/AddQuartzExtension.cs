using Core.Service.DeleteUnconfirmedUsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace Core.Extensions
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddQuartzExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                q.AddJob<DeleteUnconfirmedUsersJob>(opts => opts.WithIdentity("DeleteUnconfirmedUsersJob"));

                q.AddTrigger(opts => opts
                    .ForJob("DeleteUnconfirmedUsersJob")
                    .WithIdentity("DeleteUnconfirmedUsersTrigger")
                    //  15:40
                    .WithCronSchedule("0 40 15 * * ?"));

                // Her dakika 
                //.WithCronSchedule("0 * * * * ?"));

                // Her 5 dakikada bir 
                //.WithCronSchedule("0 0/5 * * * ?"));

                // Her gün saat 00:00'da
                //.WithCronSchedule("0 0 0 * * ?"));
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            services.AddScoped<DeleteUnconfirmedUsersJob>();

            return services;
        }
    }
}
