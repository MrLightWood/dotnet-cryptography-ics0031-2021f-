using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;
using Microsoft.AspNetCore.Authorization;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    [Authorize]
    public class CaesarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaesarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Caesar
        public async Task<IActionResult> Index()
        {
            //var applicationDbContext = _context.Caesars.Include(c => c.IdentityUser);
            var applicationDbContext = _context.Caesars.Where(c => c.IdentityUserId == User.GetUserId());
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Caesar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Caesars.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var caesar = await _context.Caesars
                .Include(c => c.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caesar == null)
            {
                return View("NotFound");
            }

            return View(caesar);
        }

        // GET: Caesar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Caesar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ShiftAmount,CipherText")] Caesar caesar)
        {
            ViewData["Header"] = "Caesar Encryption";
            
            if (string.IsNullOrEmpty(caesar.CipherText?.Trim()))
            {
                ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                return View("../Home/Output");
            }
            
            caesar.IdentityUserId = User.GetUserId();
            
            if (ModelState.IsValid)
            {
                var (isOkay, cipherText) = HW2.Caesar.Encrypt(caesar.CipherText?.Trim(), caesar.ShiftAmount%65);
                if (!isOkay)
                {
                    ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                    return View("../Home/Output");
                }
                caesar.CipherText = cipherText;
                _context.Add(caesar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", User.GetUserId());
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Caesar/Decrypt
        public IActionResult Decrypt()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrypt([Bind("Id,ShiftAmount,CipherText")] Caesar caesar)
        {
            ViewData["Header"] = "Caesar Decryption";
            ViewData["OutputDescription"] = "decryption output";

            if (caesar.Id != 0)
            {
                var isOwner = await _context.Caesars.AnyAsync(e => e.Id == caesar.Id && e.IdentityUserId == User.GetUserId());
                if (!isOwner)
                {
                    return View("NotFound");
                }
                caesar = await _context.Caesars
                    .Include(c => c.IdentityUser)
                    .FirstOrDefaultAsync(m => m.Id == caesar.Id);
            }
            
            if (string.IsNullOrEmpty(caesar.CipherText?.Trim()) || !HW2.Utils.IsBase64Chars(caesar.CipherText?.Trim()))
            {
                ViewData["Error"] = "The provided input was empty or not suitable for Decryption";
                return View("../Home/Output");
            }
            
            var (isOkay, plainText) = HW2.Caesar.Decrypt(caesar.CipherText?.Trim(), caesar.ShiftAmount%65);
            if (!isOkay)
            {
                ViewData["Error"] = "The provided input is not suitable for Decryption";
                return View("../Home/Output");
            }
            
            ViewData["OutputText"] = plainText;
            return View("../Home/Output");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate([Bind("Id,ShiftAmount,CipherText")] Caesar caesar)
        {
            if (string.IsNullOrEmpty(caesar.CipherText?.Trim()))
            {
                ViewData["Header"] = "Caesar Encryption";
                ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                return View("../Home/Output");
            }
            
            caesar.IdentityUserId = User.GetUserId();
            
            if (ModelState.IsValid)
            {
                caesar.ShiftAmount = HW2.Utils.RandomObject.Next(0, 1000);
                
                var (isOkay, cipherText) = HW2.Caesar.Encrypt(caesar.CipherText?.Trim(), caesar.ShiftAmount%65);
                if (!isOkay)
                {
                    ViewData["Error"] = "The provided input is not suitable for Encryption";
                    return View("../Home/Output");
                }
                
                caesar.CipherText = cipherText;
                _context.Add(caesar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return RedirectToAction(nameof(Index));
        }
        /*
        // GET: Caesar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Caesars.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var caesar = await _context.Caesars.FindAsync(id);
            if (caesar == null)
            {
                return View("NotFound");
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", caesar.IdentityUserId);
            return View(caesar);
        }

        // POST: Caesar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ShiftAmount,CipherText,IdentityUserId")] Caesar caesar)
        {
            if (id != caesar.Id)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Caesars.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    caesar.IdentityUserId = User.GetUserId();
                    _context.Update(caesar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaesarExists(caesar.Id))
                    {
                        return View("NotFound");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", caesar.IdentityUserId);
            return View(caesar);
        }
*/
        // GET: Caesar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var isOwner = await _context.Caesars.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            var caesar = await _context.Caesars
                .Include(c => c.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (caesar == null)
            {
                return View("NotFound");
            }

            return View(caesar);
        }

        // POST: Caesar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isOwner = await _context.Caesars.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            var caesar = await _context.Caesars.FindAsync(id);
            _context.Caesars.Remove(caesar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CaesarExists(int id)
        {
            return _context.Caesars.Any(e => e.Id == id);
        }
    }
}
