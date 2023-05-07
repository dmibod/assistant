namespace Assistant.Tenant.Infrastructure.Services;

using System.Collections.Concurrent;
using Assistant.Tenant.Core.Messaging;
using Assistant.Tenant.Core.Repositories;
using Assistant.Tenant.Core.Services;
using Common.Core.Messaging.TenantResolver;
using Common.Core.Security;
using Common.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

public class KanbanTenantResolver : ITenantResolver
{
    private readonly IServiceProvider serviceProvider;
    private readonly IDictionary<string, string> map = new ConcurrentDictionary<string, string>();
    private readonly ISet<string> system = new HashSet<string>();

    public KanbanTenantResolver(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public string Resolve(object message)
    {
        var notifications = message as List<KanbanNotification>;

        if (notifications == null || notifications.Count == 0)
        {
            return Identity.System;
        }

        var notification = notifications.First();

        if (this.map.Count > 0 && this.map.TryGetValue(notification.BoardId, out var owner))
        {
            return owner;
        }

        if (this.system.Count > 0 && this.system.Contains(notification.BoardId))
        {
            return Identity.System;
        }

        this.UpdateMap();

        if (this.map.TryGetValue(notification.BoardId, out var value))
        {
            return value;
        }

        this.system.Add(notification.BoardId);

        return Identity.System;
    }

    private void UpdateMap()
    {
        this.serviceProvider.Execute(Identity.System, scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<IKanbanService>();
            var repository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

            var owners = repository.FindAllTenantsAsync().Result;
            foreach (var owner in owners)
            {
                var boardIds = service.FindBoardIdsByOwnerAsync(owner).Result;
                foreach (var boardId in boardIds.Where(id => !this.map.ContainsKey(id)))
                {
                    this.map.Add(boardId, owner);
                }
            }
        });
    }
}