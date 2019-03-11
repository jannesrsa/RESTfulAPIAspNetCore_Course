using System.Collections.Generic;

namespace Library.API.Services
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public PropertyMapping(IDictionary<string, PropertyMappingValue> mappingDictionary)
        {
            MappingDictionary = mappingDictionary;
        }

        public IDictionary<string, PropertyMappingValue> MappingDictionary { get; }
    }
}