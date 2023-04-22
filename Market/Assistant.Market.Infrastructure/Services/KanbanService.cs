namespace Assistant.Market.Infrastructure.Services;

using Assistant.Market.Core.Services;
using KanbanApi.Client;
using Microsoft.Extensions.Logging;

public class KanbanService : IKanbanService
{
    private readonly ApiClient apiClient;
    private readonly ILogger<KanbanService> logger;

    public KanbanService(ApiClient apiClient, ILogger<KanbanService> logger)
    {
        this.apiClient = apiClient;
        this.logger = logger;
    }
}