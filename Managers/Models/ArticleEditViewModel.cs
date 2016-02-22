using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Managers.Models
{
    public class ArticleEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [AllowHtml]
        public string Body { get; set; }

        public DateTime? PublishDate { get; set; }

        [Required]
        public string Author { get; set; }

        public IList<CommentViewModel> Comments { get; set; }    
    }
}