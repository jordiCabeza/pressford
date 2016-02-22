using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Managers.Models;

namespace Managers.Article
{
    public interface IArticleManager
    {
        Task<IList<Models.ArticleViewModel>> GetAllArticles();
        
        Task<ArticleEditViewModel> GetArticleById(Guid id);
        
        Task Add(Models.ArticleEditViewModel article);

        Task Update(Models.ArticleEditViewModel article);

        Task Delete(Guid id);
    }
}