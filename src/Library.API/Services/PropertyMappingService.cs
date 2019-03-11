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

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields
                // are coming from an orderBy string, this part must be
                // ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}