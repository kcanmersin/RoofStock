using Core.Notification.StockPriceAlert;
using Core.Service.DeleteUnconfirmedUsers;
using Core.Service.OrderBackgroundService;
using Core.Service.StockTrainingJobService;
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

                q.AddJob<CheckAndProcessOrdersJob>(opts => opts.WithIdentity("CheckAndProcessOrdersJob"));
                q.AddTrigger(opts => opts
                    .ForJob("CheckAndProcessOrdersJob")
                    .WithIdentity("CheckAndProcessOrdersTrigger")
                    .WithCronSchedule("0 * * * * ?")); 

                q.AddJob<CheckAndTriggerStockPriceAlertsJob>(opts => opts.WithIdentity("CheckAndTriggerStockPriceAlertsJob"));
                q.AddTrigger(opts => opts
                    .ForJob("CheckAndTriggerStockPriceAlertsJob")
                    .WithIdentity("CheckAndTriggerStockPriceAlertsTrigger")
                    .WithCronSchedule("0 * * * * ?")); 

                // Add DeleteUnconfirmedUsersJob
                q.AddJob<DeleteUnconfirmedUsersJob>(opts => opts.WithIdentity("DeleteUnconfirmedUsersJob"));
                q.AddTrigger(opts => opts
                    .ForJob("DeleteUnconfirmedUsersJob")
                    .WithIdentity("DeleteUnconfirmedUsersTrigger")
                    .WithCronSchedule("0 40 15 * * ?")); 

                // Add StockTrainingJob for BIST
                //q.AddJob<StockTrainingJob>(opts => opts.WithIdentity("BistStockTrainingJob"));
                //q.AddTrigger(opts => opts
                //    .ForJob("BistStockTrainingJob")
                //    .WithIdentity("BistStockTrainingTrigger")
                //    .WithCronSchedule("0 0 18 * * ?") // Example: BIST closing time
                //    .UsingJobData("FileName", "bist.txt")
                //    .UsingJobData("DaysBack", 500)
                //    .UsingJobData("Epochs", 100)
                //    .UsingJobData("BatchSize", 32)
                //    .UsingJobData("SeqLen", 60)
                //    .UsingJobData("ValidationSplit", 0.1f)
                //    .UsingJobData("LearningRate", 0.001f)
                //    .UsingJobData("DropoutRate", 0.2f));



                //q.AddTrigger(opts => opts
                //    .ForJob("NasdaqStockTrainingJob")
                //    .WithIdentity("NasdaqStockTrainingTrigger")
                //    .WithCronSchedule("0 0 4 * * ?")
                //    .UsingJobData("FileName", "nasdaq.txt")
                //    .UsingJobData("DaysBack", 500)
                //    .UsingJobData("Epochs", 3)
                //    .UsingJobData("BatchSize", 32)
                //    .UsingJobData("SeqLen", 60)
                //    .UsingJobData("ValidationSplit", 0.1f)
                //    .UsingJobData("LearningRate", 0.001f)
                //    .UsingJobData("DropoutRate", 0.2f));


            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            services.AddScoped<OrderBackgroundService>();
            //stockrpicealertservice
            services.AddScoped<StockPriceAlertService>();
            services.AddScoped<CheckAndProcessOrdersJob>();
            services.AddScoped<CheckAndTriggerStockPriceAlertsJob>();

            services.AddScoped<DeleteUnconfirmedUsersJob>();
            services.AddScoped<StockTrainingJob>();

            return services;
        }
    }
}
