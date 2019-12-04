using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WinnowEntity.Data;
using WinnowEntity.Models;
using WinnowEntity.Models.ViewModels;

namespace WinnowEntity.Controllers
{
    public class PagesController : Controller
    {
        private readonly IConfiguration _config;

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        private readonly ApplicationDbContext _context;

        public PagesController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: Pages

        //public async Task<IActionResult> PageDay(int id)
        //{
        //    var thisPage = await _context.Pages
        //       .FirstOrDefaultAsync(p => p.Id == id);

        //    return View(thisPage);
        //}

        public async Task<IActionResult> PageDay([FromRoute] int id, [FromQuery] string Page)
        {
            var monthString = Page.Split("-")[0];
            var dayString = Page.Split("-")[1];
            var thisPage = await _context.Pages
                .Where(p => p.Day == dayString && p.Month == monthString)
                .Include(p => p.Quotes)
                .FirstOrDefaultAsync();
                

                    //if it does, go to page
                    if (thisPage != null)
                    {
                        return View(thisPage);
                    }

                    //if it does not, create new page
                    else
                    {
                        using (SqlConnection conn = Connection)
                        {
                            //check to see if page exists. 
                            conn.Open();
                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = @"
                                INSERT INTO Pages (
                                    Month,
                                    Day,
                                    BookId)
                                OUTPUT Inserted.ID
                                VALUES (@newMonth, @newDay, @newBookId);
                                ";
                              cmd.Parameters.Add(new SqlParameter("@newMonth", monthString));
                              cmd.Parameters.Add(new SqlParameter("@newDay", dayString));
                              cmd.Parameters.Add(new SqlParameter("@newBookId", id));
                              int pageId = (Int32)cmd.ExecuteScalar();
                              var newPage = await _context.Pages
                                    .FirstOrDefaultAsync(m => m.Id == pageId);
                                return View(newPage);
                            }
                        }
                    }
        }

        // GET: Pages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .Include(p => p.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        // GET: Pages/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title");
            return View();
        }

        // POST: Pages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Month,Day,Thought,BookId")] Page page)
        {
            if (ModelState.IsValid)
            {
                _context.Add(page);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", page.BookId);
            return View(page);
        }

        // GET: Pages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages.FirstOrDefaultAsync(m => m.Id == id);
            var viewModel = new PageEditViewModel()
            {
                Page = page,
            };

            if (page == null)
            {
                return NotFound();
            }
            
            return View(viewModel);
        }

        // POST: Pages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PageEditViewModel viewModel)
        {
            //blowing up here

            var page = await _context.Pages.FirstOrDefaultAsync(m => m.Id == id);
            

            ModelState.Remove("Page.Month");
            ModelState.Remove("Page.Day");
            if (ModelState.IsValid)
            {
                try
                {
                    viewModel.Page = page;
                    viewModel.Page.Thought = page.Thought;
                    viewModel.Quote.PageId = page.Id;
                    viewModel.Quote.Page = page;
                    viewModel.Quote.CreationDate = DateTime.Now;
                    _context.Add(viewModel.Quote);
                    _context.Update(viewModel.Page);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PageExists(viewModel.Page.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("PageDay", "Pages", new { Id = viewModel.Page.BookId, page = $"{viewModel.Page.Month}-{viewModel.Page.Day}" });
            }

            if (id != viewModel.Page.Id)
            {
                return NotFound();
            }


            return View(viewModel);
        }

        // GET: Pages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .Include(p => p.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        // POST: Pages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PageExists(int id)
        {
            return _context.Pages.Any(e => e.Id == id);
        }
    }
}
