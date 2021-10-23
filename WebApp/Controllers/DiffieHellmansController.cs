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
    public class DiffieHellmanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiffieHellmanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DiffieHellmans
        public async Task<IActionResult> Index()
        {
            //var applicationDbContext = _context.DiffieHellmans.Include(d => d.IdentityUser);
            var applicationDbContext = _context.DiffieHellmans.Where(c => c.IdentityUserId == User.GetUserId());
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DiffieHellmans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.DiffieHellmans.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var diffieHellman = await _context.DiffieHellmans
                .Include(d => d.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (diffieHellman == null)
            {
                return View("NotFound");
            }

            return View(diffieHellman);
        }

        // GET: DiffieHellmans/Create
        public IActionResult Create()
        {
            return View();
        }
        
        public IActionResult Generate()
        {
            DiffieHellman diffieHellman = new DiffieHellman();
            diffieHellman.IdentityUserId = User.GetUserId();
            
            if (ModelState.IsValid)
            {
                diffieHellman.P = (ulong)HW2.Utils.PrimesList[HW2.Utils.RandomObject.Next(0, HW2.Utils.PrimesList.Count - 1)];
                diffieHellman.Q = (ulong)HW2.Utils.PrimesList[HW2.Utils.RandomObject.Next(0, HW2.Utils.PrimesList.Count - 1)];
                diffieHellman.UserAInput = (ulong)HW2.Utils.RandomObject.Next(0, 1000000);
                diffieHellman.UserBInput = (ulong)HW2.Utils.RandomObject.Next(0, 1000000);
                var dhObject = new HW2.DiffieHellman(diffieHellman.P, diffieHellman.Q, diffieHellman.UserAInput, diffieHellman.UserBInput);

                if (!dhObject.isConfigured)
                {
                    ViewData["Error"] = "The provided input is not suitable for Encryption";
                    return View("../Home/Output");
                }
                var (isOkay, sharedKey) = dhObject.GetSharedKey();
                if (!isOkay)
                {
                    ViewData["Error"] = "The provided input is not suitable for Encryption";
                    return View("../Home/Output");
                }
                
                diffieHellman.SharedKey = sharedKey;
                _context.Add(diffieHellman);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: DiffieHellmans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,P,Q,UserAInput,UserBInput,SharedKey,IdentityUserId")] DiffieHellman diffieHellman)
        {
            ViewData["Header"] = "Diffie-Hellman Encryption";
            
            diffieHellman.IdentityUserId = User.GetUserId();
         
            if (!HW2.Utils.IsPrime(diffieHellman.P) || !HW2.Utils.IsPrime(diffieHellman.Q))
            {
                ViewData["Error"] = "The provided input was empty or not suitable for Encryption";
                return View("../Home/Output");
            }
            
            if (ModelState.IsValid)
            {
                var dhObject = new HW2.DiffieHellman(diffieHellman.P, diffieHellman.Q, diffieHellman.UserAInput, diffieHellman.UserBInput);

                if (!dhObject.isConfigured)
                {
                    ViewData["Error"] = "The provided input is not suitable for Encryption";
                    return View("../Home/Output");
                }
                var (isOkay, sharedKey) = dhObject.GetSharedKey();
                if (!isOkay)
                {
                    ViewData["Error"] = "The provided input is not suitable for Encryption";
                    return View("../Home/Output");
                }
                
                diffieHellman.SharedKey = sharedKey;
                _context.Add(diffieHellman);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", diffieHellman.IdentityUserId);
            return RedirectToAction(nameof(Index));
        }

        // GET: Diffie-Hellman/Decrypt
        public IActionResult Decrypt()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrypt([Bind("Id,P,Q,UserAInput,UserBInput,SharedKey,IdentityUserId")] DiffieHellman diffieHellman)
        {
            ViewData["Header"] = "Diffie-Hellman Decryption";
            ViewData["OutputDescription"] = "shared key";
            
            if (diffieHellman.Id != 0)
            {
                var isOwner = await _context.DiffieHellmans.AnyAsync(e => e.Id == diffieHellman.Id && e.IdentityUserId == User.GetUserId());
                if (!isOwner)
                {
                    return View("NotFound");
                }
                diffieHellman = await _context.DiffieHellmans
                    .Include(c => c.IdentityUser)
                    .FirstOrDefaultAsync(m => m.Id == diffieHellman.Id);
            }

            var dhObject = new HW2.DiffieHellman(diffieHellman.P, diffieHellman.Q, diffieHellman.UserAInput, diffieHellman.UserBInput);

            var (isOkay, sharedKey) = dhObject.GetSharedKey();
            if (!isOkay)
            {
                ViewData["Error"] = "The provided input is not suitable for Decryption";
                return View("../Home/Output");
            }
            
            ViewData["OutputText"] = sharedKey;
            return View("../Home/Output");
        }
        
        /*
        // GET: DiffieHellmans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.DiffieHellmans.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var diffieHellman = await _context.DiffieHellmans.FindAsync(id);
            if (diffieHellman == null)
            {
                return View("NotFound");
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", diffieHellman.IdentityUserId);
            return View(diffieHellman);
        }

        // POST: DiffieHellmans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,P,Q,UserAInput,UserBInput,SharedKey,IdentityUserId")] DiffieHellman diffieHellman)
        {
            if (id != diffieHellman.Id)
            {
                return View("NotFound");
            }

            var isOwner = await _context.DiffieHellmans.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    diffieHellman.IdentityUserId = User.GetUserId();
                    _context.Update(diffieHellman);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiffieHellmanExists(diffieHellman.Id))
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
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", diffieHellman.IdentityUserId);
            return View(diffieHellman);
        }
*/
        // GET: DiffieHellmans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }
            
            var isOwner = await _context.DiffieHellmans.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }

            var diffieHellman = await _context.DiffieHellmans
                .Include(d => d.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (diffieHellman == null)
            {
                return View("NotFound");
            }

            return View(diffieHellman);
        }

        // POST: DiffieHellmans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isOwner = await _context.DiffieHellmans.AnyAsync(e => e.Id == id && e.IdentityUserId == User.GetUserId());
            if (!isOwner)
            {
                return View("NotFound");
            }
            
            var diffieHellman = await _context.DiffieHellmans.FindAsync(id);
            _context.DiffieHellmans.Remove(diffieHellman);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DiffieHellmanExists(int id)
        {
            return _context.DiffieHellmans.Any(e => e.Id == id);
        }
    }
}
