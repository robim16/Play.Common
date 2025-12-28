using System;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
        {
            services.AddMassTransit(configure =>
            {
                // Se registran todos los consumidores de mensajes en el ensamblado de entrada
                configure.AddConsumers(Assembly.GetEntryAssembly());

                configure.UsingRabbitMq((context, configurator) =>
                {
                    // Se obtiene la configuración del contexto de servicios
                    var configuration = context.GetRequiredService<IConfiguration>();

                    var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();

                    configurator.Host(rabbitMQSettings.Host);

                    // Se configuran los endpoints usando el nombre del servicio en formato kebab-case
                    configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));

                    configurator.UseMessageRetry(retryConfigurator =>
                    {
                        // Configura 3 reintentos con un intervalo de 5 segundos
                        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                    });
                });
            });

            // services.AddMassTransitHostedService(); 

            return services;
        }
    }
}