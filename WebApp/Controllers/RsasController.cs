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
    public class RsaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RsaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rsas
        public async Task<IActionResult> Index()
        {
            //var applicationDbContext = _context.Rsas.Include(r => r.IdentityUser);
            var applicationDbContext = _context.Rsas.Where(c => c.IdentityUserId == User.GetUserId());
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Rsas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Rsas.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var rsa = await _context.Rsas
                .Include(r => r.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rsa == null)
            {
                return View("NotFound");
            }

            return View(rsa);
        }

        // GET: Rsas/Create
        public IActionResult Create()
        {
           return View();
        }

        // POST: Rsas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,P,Q,N,Phi,E,D,CipherText,IdentityUserId")] RSA rsa)
        {
            ViewData["Header"] = "RSA Encryption";
            
            if (string.IsNullOrEmpty(rsa.CipherText?.Trim()) || !HW2.Utils.IsEnglishLetters(rsa.CipherText?.Trim()))
            {
                ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                return View("../Home/Output");
            }
            
            rsa.IdentityUserId = User.GetUserId();
            
            if (!HW2.Utils.IsPrime(rsa.P) || !HW2.Utils.IsPrime(rsa.Q))
            {
                ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                return View("../Home/Output");
            }
            
            if (ModelState.IsValid)
            {
                var rsaObject = new HW2.RSA(rsa.P, rsa.Q);
                rsa.N = rsaObject.N;
                rsa.Phi = rsaObject.Phi;
                rsa.E = rsaObject.E;
                rsa.D = rsaObject.D;
                
                if (!rsaObject.isConfigured)
                {
                    ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                    return View("../Home/Output");
                }
                var (isOkay, cipherText) = rsaObject.Encrypt(rsa.CipherText?.Trim());
                if (!isOkay)
                {
                    ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                    return View("../Home/Output");
                }
                
                rsa.CipherText = cipherText;
                _context.Add(rsa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", rsa.IdentityUserId);
            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult Decrypt()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrypt([Bind("Id,P,Q,N,Phi,E,D,CipherText,IdentityUserId")] RSA rsa)
        {
            ViewData["Header"] = "RSA Decryption";
            ViewData["OutputDescription"] = "decryption output";

            if (rsa.Id != 0)
            {
                var isOwner = await _context.Rsas.AnyAsync(e => e.Id == rsa.Id && e.IdentityUserId == User.GetUserId());
                if (!isOwner)
                {
                    return View("NotFound");
                }
                rsa = await _context.Rsas
                    .Include(c => c.IdentityUser)
                    .FirstOrDefaultAsync(m => m.Id == rsa.Id);
            }
            
            if (string.IsNullOrEmpty(rsa.CipherText?.Trim()) || !HW2.Utils.IsBase64Chars(rsa.CipherText?.Trim()))
            {
                ViewData["Error"] = "The provided input was empty or not suitable for Decryption";
                return View("../Home/Output");
            }

            var rsaObject = new HW2.RSA(rsa.P, rsa.Q);
            rsaObject.setEvalue(rsa.E);
            rsaObject.setDvalue(rsa.D);
            
            var (isOkay, plainText) = rsaObject.Decrypt(rsa.CipherText?.Trim());
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
        public async Task<IActionResult> Generate([Bind("Id,P,Q,N,Phi,E,D,CipherText,IdentityUserId")] RSA rsa)
        {
            rsa.IdentityUserId = User.GetUserId();
            
            if (ModelState.IsValid)
            {
                rsa.P = (ulong)HW2.Utils.PrimesList[HW2.Utils.RandomObject.Next(0, HW2.Utils.PrimesList.Count - 1)];
                rsa.Q = (ulong)HW2.Utils.PrimesList[HW2.Utils.RandomObject.Next(0, HW2.Utils.PrimesList.Count - 1)];
                var rsaObject = new HW2.RSA(rsa.P, rsa.Q);
                rsa.N = rsaObject.N;
                rsa.Phi = rsaObject.Phi;
                rsa.E = rsaObject.E;
                rsa.D = rsaObject.D;
                
                if (!rsaObject.isConfigured)
                {
                    ViewData["Error"] = "The provided input is not suitable for Encryption";
                    return View("../Home/Output");
                }
                var (isOkay, cipherText) = rsaObject.Encrypt(rsa.CipherText?.Trim());
                if (!isOkay)
                {
                    ViewData["Error"] = "The provided input is not suitable for Encryption";
                    return View("../Home/Output");
                }
                
                rsa.CipherText = cipherText;
                _context.Add(rsa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return RedirectToAction(nameof(Index));
        }

        /*
        // GET: Rsas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Rsas.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var rsa = await _context.Rsas.FindAsync(id);
            if (rsa == null)
            {
                return View("NotFound");
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", rsa.IdentityUserId);
            return View(rsa);
        }

        // POST: Rsas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,P,Q,N,Phi,E,D,CipherText,IdentityUserId")] RSA rsa)
        {
            if (id != rsa.Id)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Rsas.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    rsa.IdentityUserId = User.GetUserId();
                    _context.Update(rsa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RSAExists(rsa.Id))
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
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", rsa.IdentityUserId);
            return View(rsa);
        }
*/
        
        // GET: Rsas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.Rsas.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var rsa = await _context.Rsas
                .Include(r => r.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rsa == null)
            {
                return View("NotFound");
            }

            return View(rsa);
        }

        // POST: Rsas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isOwner = await _context.Rsas.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            var rsa = await _context.Rsas.FindAsync(id);
            _context.Rsas.Remove(rsa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RSAExists(int id)
        {
            return _context.Rsas.Any(e => e.Id == id);
        }
    }
}
