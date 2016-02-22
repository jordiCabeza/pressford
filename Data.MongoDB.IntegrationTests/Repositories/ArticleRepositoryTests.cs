using System;
using System.Linq;
using System.Text;

using Data.MongoDB.Models;
using Data.MongoDB.Repositories;

using NUnit.Framework;

namespace Data.MongoDB.IntegrationTests.Repositories
{
    [TestFixture]
    public class ArticleRepositoryTests
    {
        private ArticleRepository articleRepository;

        [SetUp]
        public void SetUp()
        {
            articleRepository = new ArticleRepository();
        }

        [Test]
        public async void ShouldAddDeleteArticle()
        {
            // Arrange
            var article = new Article
                              {
                                  Title = "someTitle", Body = "someBody", Author = "Author2", PublishDate = DateTime.UtcNow,
                                  Comments =
                                      new[]
                                          {
                                              new Comment { Text = "someComment", Author = "Author2", Date = DateTime.UtcNow },
                                              new Comment { Text = "otherComment", Author = "Author3", Date = DateTime.UtcNow }
                                          }
                              };

            // Act
                // Add
                await articleRepository.Add(article);
                var addedArticle = await articleRepository.GetById(article.Id);
                
                // GetById
                Assert.IsNotNull(addedArticle);
                AssertAreEqual(addedArticle, article);

                // Update
                addedArticle.Title = "new Title";
                await articleRepository.Update(addedArticle);
                var updatedArticle = await articleRepository.GetById(article.Id);
                Assert.That(updatedArticle.Title, Is.EqualTo(addedArticle.Title));

                // Delete
                await articleRepository.Delete(updatedArticle.Id);
                var deletedArticle = await articleRepository.GetById(updatedArticle.Id);
                Assert.IsNull(deletedArticle);
        }

        [Test]
        public async void ShouldGetAll()
        {
            // Arrange
            var expectedArticles = new[]
                                {
                                    new Article { Title = Guid.NewGuid() + " Article1" },
                                    new Article { Title = Guid.NewGuid() + " Article2" },
                                    new Article { Title = Guid.NewGuid() + " Article3" }
                               };
            
            var originalArticles = await articleRepository.GetAll();
            var initialCount = originalArticles != null ? originalArticles.Count() : 0;
            
            foreach (var article in expectedArticles)
            {
                await articleRepository.Add(article);
            }
            
            // Act
            var articles = await articleRepository.GetAll();
            
            // Assert
            Assert.IsNotNull(articles);
            Assert.That(articles.Count(), Is.EqualTo(initialCount + 3));

            foreach (var article in expectedArticles)
            {
                Assert.That(articles.Any(article1 => article1.Title == article.Title));
            }
        }

        private void AssertAreEqual(Article article1, Article article2)
        {
            Assert.That(article1.Title, Is.EqualTo(article2.Title));
            Assert.That(article1.Body, Is.EqualTo(article2.Body));
            Assert.That(article1.Author, Is.EqualTo(article2.Author));
            
            var comments1 = article1.Comments;
            
            if (comments1 == null)
            {
                return;
            }

            for (int i = 0; i < comments1.Count; i++)
            {
                Assert.That(comments1[i].Text, Is.EqualTo(article2.Comments[i].Text));
                Assert.That(comments1[i].Author, Is.EqualTo(article2.Comments[i].Author));
                Assert.That(comments1[i].Date, Is.EqualTo(article2.Comments[i].Date).Within(TimeSpan.FromMilliseconds(1)));
            }
        }
    }
}