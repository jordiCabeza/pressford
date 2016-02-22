using System;
using System.Collections.Generic;

namespace Data.Models
{
    public class Article
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
 
        public string Body { get; set; }

        public DateTime PublishDate { get; set; }

        public string Author { get; set; }

        public IEnumerable<Comment> Comments { get; set; }
    }
}