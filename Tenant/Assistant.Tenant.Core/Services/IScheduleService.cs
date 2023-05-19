namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IScheduleService
{
    Task<IEnumerable<Schedule>> FindAllAsync();
    
    Task ExecuteScheduleAsync(ScheduleType scheduleType);
    
    Task<Schedule> CreateAsync(ScheduleType scheduleType);
    
    Task UpdateAsync(ScheduleType scheduleType, ScheduleInterval interval);
}