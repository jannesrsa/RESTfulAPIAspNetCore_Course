using System.Collections.Generic;

namespace Library.API.Models
{
    public class LinkedCollectionResrouceWrapperDto<T> : LinkedResourceBaseDto
        where T : LinkedResourceBaseDto
    {
        public IEnumerable<T> Value
        {
            get; set;
        }
    }
}