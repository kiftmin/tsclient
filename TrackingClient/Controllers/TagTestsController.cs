using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrackingClient.Models;
using Microsoft.Extensions.Options;
using TrackingClient.Services;

namespace TrackingClient.Controllers
{
    public class TagTestsController : Controller
    {
        private readonly DBCtx _context;
        private readonly IOptions<MyAppSettings> _options;

        public TagTestsController(DBCtx context, IOptions<MyAppSettings> options)
        {
            _context = context;
            _options = options;
        }

        // GET: TagTests
        public async Task<IActionResult> Index()
        {
            return View(await _context.TagTests.ToListAsync());
        }

        // GET: TagTests/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagTest = await _context.TagTests
                .FirstOrDefaultAsync(m => m.TagID .Equals(id));
            if (tagTest == null)
            {
                return NotFound();
            }

            return View(tagTest);
        }

        public async Task<IActionResult> Create([Bind("TagID,TagNumber,ClientName,Reader,ReaderConn,Time")] TagTest tagTest)
        {
            string start = "1";
            if (start =="1")
            {
                tagTest.Reader = _options.Value.Reader;
               
                tagTest.ClientName = Global.MQTTReader.ClientName;
                tagTest.ReaderConn = Global.ReaderConnectionStatus;
                tagTest.Time = Global.ReaderConnectionStatusDate;
            }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index","Home");
        }

        // GET: TagTests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagTest = await _context.TagTests.FindAsync(id);
            if (tagTest == null)
            {
                return NotFound();
            }
            return View(tagTest);
        }

        // POST: TagTests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TagID,TagNumber")] TagTest tagTest)
        {
            if (id != tagTest.TagID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    tagTest.Reader = _options.Value.Reader;

                    tagTest.ClientName = Global.MQTTReader.ClientName;
                    tagTest.ReaderConn = Global.ReaderConnectionStatus;
                    tagTest.Time = Global.ReaderConnectionStatusDate;
                    _context.Update(tagTest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagTestExists(tagTest.TagID))
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
            return View(tagTest);
        }

        // GET: TagTests/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagTest = await _context.TagTests
                .FirstOrDefaultAsync(m => m.TagID == id);
            if (tagTest == null)
            {
                return NotFound();
            }

            return View(tagTest);
        }

        // POST: TagTests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tagTest = await _context.TagTests.FindAsync(id);
            _context.TagTests.Remove(tagTest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagTestExists(string id)
        {
            return _context.TagTests.Any(e => e.TagID == id);
        }
    }
}
