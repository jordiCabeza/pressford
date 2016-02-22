using System;
using System.Threading.Tasks;

using Data.MongoDB.Models;

namespace Data.MongoDB.Repositories
{
    public interface IArticleRepository : IBaseRepository<Article, Guid>
    {
        Task UpdateArticle(Article article);
    }
}