using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Data.MongoDB.Repositories;

using Managers.Models;

namespace Managers.Article
{
    public class ArticleManager : IArticleManager
    {
        private readonly IArticleRepository articleRepository;

        public ArticleManager(IArticleRepository articleRepository)
        {
            this.articleRepository = articleRepository;
        }

        public async Task<IList<ArticleViewModel>> GetAllArticles()
        {
            var domainArticles = await articleRepository.GetAll();
            return domainArticles.Select(MapToViewModel).ToList();
        }

        public async Task<ArticleEditViewModel> GetArticleById(Guid id)
        {
            return MapToEditViewModel(await articleRepository.GetById(id));
        }

        public async Task Add(ArticleEditViewModel article)
        {
            await articleRepository.Add(MapToDomain(article));
        }

        public async Task Update(ArticleEditViewModel article)
        {
            await articleRepository.UpdateArticle(MapToDomain(article));
        }

        public async Task Delete(Guid id)
        {
            await articleRepository.Delete(id);
        }

        private static ArticleEditViewModel MapToEditViewModel(Data.MongoDB.Models.Article domainArticle)
        {
            return new ArticleEditViewModel
                       {
                           Id = domainArticle.Id,
                           Title = domainArticle.Title,
                           Body = domainArticle.Body,
                           Author = domainArticle.Author,
                           PublishDate = domainArticle.PublishDate
                       };
        }

        private static ArticleViewModel MapToViewModel(Data.MongoDB.Models.Article domainArticle)
        {
            return new Models.ArticleViewModel
                       {
                           Id = domainArticle.Id,
                           Title = domainArticle.Title,
                           PublishDate = domainArticle.PublishDate,
                           Author = domainArticle.Author
                       };
        }

        private static Data.MongoDB.Models.Article MapToDomain(ArticleEditViewModel viewModel)
        {
            return new Data.MongoDB.Models.Article
            {
                Id = viewModel.Id,
                Title = viewModel.Title,
                Body = viewModel.Body,
                PublishDate = viewModel.PublishDate,
                Author = viewModel.Author
            };
        }
    }
}