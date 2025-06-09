using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCrossMicroserviceMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, InMemoryEventBus>();
            services.AddSingleton<INotificationHandler<Events.RateChangedEvent>, CrossServiceEventForwarder>();

            services.AddSingleton<IPositionValuePublisher, ConsolePositionValuePublisher>();
            services.AddScoped<INotificationHandler<Events.PositionValueCalculatedEvent>, PositionValueHandler>();

            return services;
        }
    }

    public interface IEventBus
    {
        Task PublishAsync<T>(T @event) where T : INotification;
        void Subscribe<T>(Action<T> handler) where T : INotification;
    }

    public class InMemoryEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public Task PublishAsync<T>(T @event) where T : INotification
        {
            if (_handlers.TryGetValue(typeof(T), out var handlers))
            {
                foreach (Action<T> handler in handlers)
                {
                    handler(@event);
                }
            }

            return Task.CompletedTask;
        }

        public void Subscribe<T>(Action<T> handler) where T : INotification
        {
            if (!_handlers.ContainsKey(typeof(T)))
            {
                _handlers[typeof(T)] = new List<Delegate>();
            }

            _handlers[typeof(T)].Add(handler);
        }
    }

    // This forwards events between microservices
    public class CrossServiceEventForwarder : INotificationHandler<Events.RateChangedEvent>
    {
        private readonly IEventBus _eventBus;

        public CrossServiceEventForwarder(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task Handle(Events.RateChangedEvent notification, CancellationToken cancellationToken)
        {
            await _eventBus.PublishAsync(notification);
        }
    }
}