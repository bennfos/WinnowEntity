using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WinnowEntity.Data;
using WinnowEntity.Models;

namespace WinnowEntity.Controllers
{
    public class QuotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Quotes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Quotes.Include(q => q.Page);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Quotes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .Include(q => q.Page)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        // GET: Quotes/Create
        public IActionResult Create()
        {
            ViewData["PageId"] = new SelectList(_context.Pages, "Id", "Day");
            return View();
        }

        // POST: Quotes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuoteText,QuoteAuthor,CreationDate,PageId")] Quote quote)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PageId"] = new SelectList(_context.Pages, "Id", "Day", quote.PageId);
            return View(quote);
        }

        // GET: Quotes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null)
            {
                return NotFound();
            }
            ViewData["PageId"] = new SelectList(_context.Pages, "Id", "Day", quote.PageId);
            return View(quote);
        }

        // POST: Quotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuoteText,QuoteAuthor,CreationDate,PageId")] Quote quote)
        {
            if (id != quote.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuoteExists(quote.Id))
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
            ViewData["PageId"] = new SelectList(_context.Pages, "Id", "Day", quote.PageId);
            return View(quote);
        }

        // GET: Quotes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .Include(q => q.Page)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        // POST: Quotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuoteExists(int id)
        {
            return _context.Quotes.Any(e => e.Id == id);
        }
    }
}
