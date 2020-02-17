using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Niusys.Extensions.TypeFinders
{
    public class DefaultTypeFinder : ITypeFinder
    {
        public DefaultTypeFinder(IOptions<TypeFinderOptions> options)
        {
            this.Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public TypeFinderOptions Options { get; }

        public List<Type> FindAllType<TInterface>()
        {
            return FindAllType(typeof(TInterface));
        }

        public List<Type> FindAllType(Type type)
        {
            var dependencyContext = DependencyContext.Default;
            var assembliesToScan = dependencyContext.RuntimeLibraries
                .SelectMany(lib => lib.GetDefaultAssemblyNames(dependencyContext).Select(Assembly.Load))
                .Where(x => Regex.IsMatch(x.FullName, Options.AssemblyMatchRegex, RegexOptions.Compiled))
                .ToArray();

            var allTypes = assembliesToScan.SelectMany(a => a.ExportedTypes).ToArray();

            var foundTypes = allTypes
                .Where(t => !t.GetTypeInfo().IsAbstract && t.GetInterfaces().SingleOrDefault(x => x == type) != null)
                .ToList();

            return foundTypes;
        }

        public Type FindFirstOrDefalt(Type type)
        {
            var dependencyContext = DependencyContext.Default;
            var assembliesToScan = dependencyContext.RuntimeLibraries
                .SelectMany(lib => lib.GetDefaultAssemblyNames(dependencyContext).Select(Assembly.Load))
                .Where(x => Regex.IsMatch(x.FullName, Options.AssemblyMatchRegex, RegexOptions.Compiled))
                .ToArray();

            var allTypes = assembliesToScan.SelectMany(a => a.ExportedTypes).ToArray();

            var foundType = allTypes
                .FirstOrDefault(t => !t.GetTypeInfo().IsAbstract && t.GetInterfaces().SingleOrDefault(x => x == type) != null);

            return foundType;
        }
    }
}
