using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab4v2.Data;
using Lab4v2.Models;
using Azure.Storage.Blobs;
using Azure;

namespace Lab4.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsDbContext _context;

        private readonly BlobServiceClient _blobServiceClient;


        public NewsController(NewsDbContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        // GET: News
        public async Task<IActionResult> Index(string Id)
        {
            var newsDbContext = _context.News.Include(n => n.NewsBoard).Where(x => x.NewsBoardID == Id);
            await newsDbContext.ToListAsync();

            // var news = await _context.News.Where(x =>x.NewsBoardID == Id).ToListAsync();
            ViewBag.NewsBoardID = Id;

            return View(newsDbContext);
        }


        // GET: News/Create
        public IActionResult Create(string NewsBoardId)
        {

            ViewData["NewsBoardID"] = new SelectList(_context.NewsBoards, "Id", "Id");
            ViewBag.NewsBoardID = NewsBoardId;
            return View();
        }


        // POST: News/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FileName,Url,NewsBoardID,imageFile")] News news)
        {

            string tempName;

            if (news.imageFile == null)
            {
                return RedirectToAction("Error", "Home");

            }

            var absolutePath = Path.GetFullPath(news.imageFile.FileName);

            tempName = news.imageFile.FileName;
            string containerName = "news-images"; // valid container name
            BlobContainerClient containerClient;

            try
            {
                // create the container if it does not already exist
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
                containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
            }
            catch (RequestFailedException)
            {
                // the container already exists
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            }

            try
            {
                // generate a unique blob name using a GUID
                string blobName = Guid.NewGuid().ToString() + Path.GetExtension(news.imageFile.FileName);

                // upload the file to the container
                var blockBlob = containerClient.GetBlobClient(blobName);

                using (var memStream = new MemoryStream())
                {
                    await news.imageFile.CopyToAsync(memStream);
                    memStream.Position = 0;
                    await blockBlob.UploadAsync(memStream);
                }

                // update the News object with the blob name and URL
                news.FileName = tempName;
                news.Url = blockBlob.Uri.AbsoluteUri;
            }
            catch (RequestFailedException)
            {
                // handle the exception
                RedirectToPage("Error");
            }

            if (ModelState.IsValid)
            {
                news.Id = Guid.NewGuid();
                _context.Add(news);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "News", new { id = news.NewsBoardID });
            }
            else
            {
                ViewData["NewsBoardID"] = new SelectList(_context.NewsBoards, "Id", "Id", news.NewsBoardID);
                return RedirectToPage("Error");
            }
        }


        // GET: News/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.News == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .Include(n => n.NewsBoard)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.News == null)
            {
                return Problem("Entity set 'NewsDbContext.News'  is null.");
            }
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "News", new { id = news.NewsBoardID });
        }

        private bool NewsExists(Guid id)
        {
            return _context.News.Any(e => e.Id == id);
        }
    }
}
