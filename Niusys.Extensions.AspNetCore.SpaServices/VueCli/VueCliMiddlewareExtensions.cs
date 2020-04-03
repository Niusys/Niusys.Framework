using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Niusys.Extensions.AspNetCore.SpaServices.VueCli
{
    public static class VueCliMiddlewareExtensions
    {
        public static void UseVueCliServer(
          this IMulitSpaBuilder spaBuilder,
          string npmScript)
        {
            if (spaBuilder == null)
            {
                throw new ArgumentNullException(nameof(spaBuilder));
            }

            var spaOptions = spaBuilder.Options;

            if (string.IsNullOrEmpty(spaOptions.SourcePath))
            {
                throw new InvalidOperationException($"To use {nameof(UseVueCliServer)}, you must supply a non-empty value for the {nameof(MulitSpaOptions.SourcePath)} property of {nameof(MulitSpaOptions)} when calling {nameof(MulitSpaApplicationBuilderExtensions.UseMulitSpa)}.");
            }

            VueCliMiddleware.Attach(spaBuilder, npmScript);
        }
    }

}
