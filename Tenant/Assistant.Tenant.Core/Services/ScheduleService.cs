namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;
using Assistant.Tenant.Core.Repositories;
using Microsoft.Extensions.Logging;

public class ScheduleService : IScheduleService
{
    private readonly ITenantService tenantService;
    private readonly ITenantRepository repository;
    private readonly ILogger<ScheduleService> logger;

    public ScheduleService(
        ITenantService tenantService, 
        ITenantRepository repository, 
        ILogger<ScheduleService> logger)
    {
        this.tenantService = tenantService;
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<IEnumerable<Schedule>> FindAllAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.FindAllAsync));
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        return await this.repository.FindSchedules(tenant);
    }

    public async Task ExecuteScheduleAsync(ScheduleType scheduleType)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.ExecuteScheduleAsync), $"{scheduleType}");
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        await this.repository.ExecuteSchedule(tenant, scheduleType);
    }

    public async Task<Schedule> CreateAsync(ScheduleType scheduleType)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), $"{scheduleType}");
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        var schedule = await this.repository.FindSchedule(tenant, scheduleType);

        return schedule == null ? await this.repository.CreateScheduleAsync(tenant, scheduleType, ScheduleInterval.None) : schedule;
    }

    public async Task UpdateAsync(ScheduleType scheduleType, ScheduleInterval interval)
    {
        this.logger.LogInformation("{Method} with argument {Argument}", nameof(this.CreateAsync), $"{scheduleType}");
        
        var tenant = await this.tenantService.EnsureExistsAsync();

        var schedule = await this.repository.FindSchedule(tenant, scheduleType);

        if (schedule == null)
        {
            await this.repository.CreateScheduleAsync(tenant, scheduleType, interval);
        }
        else
        {
            await this.repository.UpdateSchedule(tenant, scheduleType, interval);
        }
    }
}