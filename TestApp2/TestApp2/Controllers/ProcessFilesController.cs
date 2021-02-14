using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestApp2.Data;
using TestApp2.Models;

namespace TestApp2.Controllers
{
    [Authorize]
    public class ProcessFilesController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProcessFilesController(
            SignInManager<ApplicationUser> signInManager, 
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            var files = new List<ProcessFile>();

            if (User.IsInRole("Receptionist"))
            {
                files = await _context.ProcessFile.Where(pf => pf.RouteStatus == 1).ToListAsync();
            }
            else if (User.IsInRole("HR Manager"))
            {
                files = await _context.ProcessFile.Where(pf => pf.RouteStatus == 2).ToListAsync();
            }
            else if (User.IsInRole("Managing Director"))
            {
                files = await _context.ProcessFile.Where(pf => pf.RouteStatus == 3).ToListAsync();
            }
            else
            {
                files = await _context.ProcessFile.ToListAsync();
            }

            return View(files);
        }     

        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,RouteStatus")] ProcessFile processFile)
        {
            processFile.RouteStatus = 1;
            if (ModelState.IsValid)
            {
                _context.Add(processFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(processFile);
        }

    
        public async Task<IActionResult> ReviewFile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var processFile = await _context.ProcessFile
                .SingleOrDefaultAsync(m => m.Id == id);
            if (processFile == null)
            {
                return NotFound();
            }

            return View(processFile);
        }

       
        [HttpPost, ActionName("ReviewFile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewFile(int id, bool status)
        {
            var file = await _context.ProcessFile.SingleOrDefaultAsync(pf => pf.Id == id);

            if (User.IsInRole("Receptionist"))
            {
                if (status == true)
                {
                    file.RouteStatus = 2;
                    _context.Update(file);
                }
            }
            else if (User.IsInRole("HR Manager"))
            {
                if (status == true)
                {
                    file.RouteStatus = 3;
                    _context.Update(file);
                }
                else
                {
                    file.RouteStatus = 1;
                    _context.Update(file);
                }
            }
            else if (User.IsInRole("Managing Director"))
            {
                if (status == true)
                {
                    file.RouteStatus = 1;
                    _context.Update(file);
                }
                else
                {
                    file.RouteStatus = 2;
                    _context.Update(file);
                }
            }
            else
            {
                return RedirectToAction("ReviewFile", id);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }       
              
    }
}
