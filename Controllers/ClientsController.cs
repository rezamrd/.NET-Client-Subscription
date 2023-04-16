using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab4v2.Data;
using Lab4v2.Models;
using Lab4v2.Models.ViewModels;

namespace Lab4v2.Controllers
{
    public class ClientsController : Controller
    {
        private readonly NewsDbContext _context;

        public ClientsController(NewsDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index(int id = 0)
        {

            //Aassignment 2
            NewsBoardViewModel newsBoardViewModel = new NewsBoardViewModel();
            newsBoardViewModel.Clients = await _context.Clients.ToListAsync();
            if (id != 0)
            {
                newsBoardViewModel.NewsBoards = await _context.NewsBoards.ToListAsync();

                var newsboardId = await _context.Subscriptions
                .Where(i => i.ClientId == id)
                .Select(i => i.NewsBoardId)
                .ToListAsync();

                newsBoardViewModel.Subscriptions = await _context.Subscriptions
                    .Where(c => newsboardId.Contains(c.NewsBoardId))
                    .Distinct()
                    .ToListAsync();
            }

            return View(newsBoardViewModel);
   
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LastName,FirstName,BirthDate")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LastName,FirstName,BirthDate")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Clients == null)
            {
                return Problem("Entity set 'NewsDbContext.Clients'  is null.");
            }
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
          return _context.Clients.Any(e => e.Id == id);
        }


        public async Task<IActionResult> EditSubscriptions(int id = 0)
        {

            if (id == 0)
            {
                return NotFound();
            }
            else
            {
                NewsBoardViewModel newsBoardViewModel = new NewsBoardViewModel();

                newsBoardViewModel.Subscriptions = await _context.Subscriptions
                                    .Where(c => c.ClientId == id)
                                    .AsNoTracking()
                                    .ToListAsync();
                newsBoardViewModel.Clients = await _context.Clients
                                    .Where(s => s.Id == id)
                                    .AsNoTracking()
                                    .ToListAsync();
                newsBoardViewModel.NewsBoards = await _context.NewsBoards
                                    .AsNoTracking()
                                    .ToListAsync();

                return View(newsBoardViewModel);
            }
        }
        

        public async Task<IActionResult> RemoveSubscriptions(int ClientId, string NewsBoardId)
        {
            var removeRow = new Subscription { ClientId = ClientId, NewsBoardId = NewsBoardId };

            _context.Subscriptions.Remove(removeRow);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> AddSubscriptions(int ClientId, string NewsBoardId)
        {
            var addRow = new Subscription { ClientId = ClientId, NewsBoardId = NewsBoardId };

            _context.Subscriptions.Add(addRow);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        
    }
}
