using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Managers.Article;
using Managers.Models;

using Pressford.Models;

using StructureMap.Query;

namespace Pressford.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleManager articleManager;

        public ArticleController(IArticleManager articleManager)
        {
            this.articleManager = articleManager;
        }

        // GET: Article
        public async Task<ActionResult> Index()
        {
            var articles = await articleManager.GetAllArticles();
            return View(articles);
        }

        // GET: Article/Details/5
        public async Task<ActionResult> Details(int id)
        {
            return View();
        }

        // GET: Article/Create
        public async Task<ActionResult> Create()
        {
            return View();
        }

        // POST: Article/Create
        [HttpPost]
        public async Task<ActionResult> Create(ArticleEditViewModel article)
        {
            if (ModelState.IsValid)
            {
                await articleManager.Add(article);
                return RedirectToAction("Index");
            }

            return View(article);
        }

        // GET: Article/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            var article = await articleManager.GetArticleById(Guid.Parse(id));
            
            return View(article);
        }

        // POST: Article/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ArticleEditViewModel article)
        {
            if (ModelState.IsValid)
            {
                await articleManager.Update(article);
                return RedirectToAction("Index");
            }
            
            return View(article);
        }

        // GET: Article/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            var article = await articleManager.GetArticleById(Guid.Parse(id));
            return View(article);
        }

        // POST: Article/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            await articleManager.Delete(Guid.Parse(id));
            return RedirectToAction("Index");
        }
    }
}
