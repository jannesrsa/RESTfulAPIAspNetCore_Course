﻿using System.Collections.Generic;

namespace Library.API.Services
{
    public interface IPropertyMappingService
    {
        IDictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();

        bool ValidMappingExistsFor<TSource, TDestination>(string fields);
    }
}