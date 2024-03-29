﻿namespace Common.Core.Security;

using System.Security.Principal;

public class Identity : IIdentity
{
    public const string System = "system";
    
    public Identity(string? name)
    {
        this.Name = name;
    }

    public string? AuthenticationType => string.Empty;
    
    public bool IsAuthenticated => true;
    
    public string? Name { get; }
}