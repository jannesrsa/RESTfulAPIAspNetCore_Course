using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private IDictionary<string, PropertyMappingValue> _authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new string[]{"Id" }) },
                {"Age", new PropertyMappingValue(new string[]{"DateOfBirth" },true) },
                {"Genre", new PropertyMappingValue(new string[]{"Genre" }) },
                {"Name", new PropertyMappingValue(new string[]{"FirstName" ,"LastName"}) }
            };

        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
        }

        public IDictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            return _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>()
                .FirstOrDefault()
                ?.MappingDictionary;
        }
    }
}