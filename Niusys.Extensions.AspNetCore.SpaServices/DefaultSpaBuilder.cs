// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using System;

namespace Niusys.Extensions.AspNetCore.SpaServices
{
    internal class DefaultSpaBuilder : IMulitSpaBuilder
    {
        public IApplicationBuilder ApplicationBuilder { get; }

        public MulitSpaOptions Options { get; }

        public DefaultSpaBuilder(IApplicationBuilder applicationBuilder, MulitSpaOptions options)
        {
            ApplicationBuilder = applicationBuilder 
                ?? throw new ArgumentNullException(nameof(applicationBuilder));

            Options = options
                ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
