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
    public class VigenereController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VigenereController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vigeneres
        public async Task<IActionResult> Index()
        {
            //var applicationDbContext = _context.Vigeneres.Include(v => v.IdentityUser);
            var applicationDbContext = _context.Vigeneres.Where(c => c.IdentityUserId == User.GetUserId());
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Vigeneres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Vigeneres.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var vigenere = await _context.Vigeneres
                .Include(v => v.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vigenere == null)
            {
                return View("NotFound");
            }

            return View(vigenere);
        }

        // GET: Vigeneres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vigeneres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Key,CipherText,IdentityUserId")] Vigenere vigenere)
        {
            ViewData["Header"] = "Vigenere Encryption";
            
            if (string.IsNullOrEmpty(vigenere.CipherText?.Trim()))
            {
                ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                return View("../Home/Output");
            }
            
            vigenere.IdentityUserId = User.GetUserId();

            if (!HW2.Utils.IsBase64Chars(vigenere.Key))
            {
                ViewData["Error"] = "The provided key is not suitable for Encryption";
                return View("../Home/Output");
            }
            
            if (ModelState.IsValid)
            {
                var (isOkay, cipherText) = HW2.Vigenere.Encrypt(vigenere.CipherText?.Trim(), vigenere.Key.Trim());
                if (!isOkay)
                {
                    ViewData["Error"] = "The provided input is not suitable for Encryption";
                    return View(vigenere);
                }
                
                vigenere.CipherText = cipherText;
                _context.Add(vigenere);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", vigenere.IdentityUserId);
            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult Decrypt()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrypt([Bind("Id,Key,CipherText,IdentityUserId")] Vigenere vigenere)
        {
            ViewData["Header"] = "Vigenere Decryption";
            ViewData["OutputDescription"] = "decryption output";

            if (vigenere.Id != 0)
            {
                var isOwner = await _context.Vigeneres.AnyAsync(e => e.Id == vigenere.Id && e.IdentityUserId == User.GetUserId());
                if (!isOwner)
                {
                    return View("NotFound");
                }
                vigenere = await _context.Vigeneres
                    .Include(c => c.IdentityUser)
                    .FirstOrDefaultAsync(m => m.Id == vigenere.Id);
            }
            
            if (string.IsNullOrEmpty(vigenere.CipherText?.Trim()) || !HW2.Utils.IsBase64Chars(vigenere.CipherText?.Trim()))
            {
                ViewData["Error"] = "The provided input was empty or not suitable for Decryption";
                return View("../Home/Output");
            }
            
            var (isOkay, plainText) = HW2.Vigenere.Decrypt(vigenere.CipherText?.Trim(), vigenere.Key.Trim());
            if (!isOkay)
            {
                ViewData["Error"] = "The provided input is not suitable for Decryption";
                return View("../Home/Output");
            }
            
            ViewData["OutputText"] = plainText;
            return View("../Home/Output");
        }

        /*
        // GET: Vigeneres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Vigeneres.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var vigenere = await _context.Vigeneres.FindAsync(id);
            if (vigenere == null)
            {
                return View("NotFound");
            }
            return View(vigenere);
        }

        // POST: Vigeneres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Key,CipherText")] Vigenere vigenere)
        {
            if (id != vigenere.Id)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Vigeneres.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    vigenere.IdentityUserId = User.GetUserId();
                    _context.Update(vigenere);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VigenereExists(vigenere.Id))
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
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", vigenere.IdentityUserId);
            return View(vigenere);
        }
*/
        
        // GET: Vigeneres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var isOwner = await _context.Vigeneres.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            var vigenere = await _context.Vigeneres
                .Include(v => v.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vigenere == null)
            {
                return View("NotFound");
            }

            return View(vigenere);
        }

        // POST: Vigeneres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isOwner = await _context.Vigeneres.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            var vigenere = await _context.Vigeneres.FindAsync(id);
            _context.Vigeneres.Remove(vigenere);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VigenereExists(int id)
        {
            return _context.Vigeneres.Any(e => e.Id == id);
        }
    }
}
