﻿// Copyright (c) Ryan Foster. All rights reserved. 
// Licensed under the Apache License, Version 2.0.

using StandardResponse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIAM.PasswordlessLogin.Services.Email
{
    public interface IEmailTemplateService
    {
        Task<Status> SendEmailAsync(string templateName, string to, IDictionary<string, string> fields);        
    }
}
