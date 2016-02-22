using System;

namespace Managers.Models
{
    public class ArticleViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public DateTime? PublishDate { get; set; }
    }
}