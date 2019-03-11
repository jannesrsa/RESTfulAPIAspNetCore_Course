using System.Collections.Generic;

namespace Library.API.Models
{
    public abstract class LinkedResourceBaseDto
    {
        public List<LinkDto> Links { get; } = new List<LinkDto>();
    }
}