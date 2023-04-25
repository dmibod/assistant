﻿namespace Assistant.Tenant.Core.Services;

using Assistant.Tenant.Core.Models;

public interface IPositionService
{
    Task<IEnumerable<Position>> FindAllAsync();
    
    Task<Position> CreateAsync(Position position);

    Task RemoveAsync(string account, string asset);

    Task ResetTagAsync();

    Task ReplaceTagAsync(string oldValue, string newValue);

    Task UpdateTagAsync(string account, string asset);
}