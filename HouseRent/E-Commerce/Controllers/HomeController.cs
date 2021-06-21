using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using E_Commerce.Models;
using Microsoft.AspNetCore.Http;
using E_Commerce.Data;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        //private ISession _session;
      

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
          //  _session = session;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(await _context.House.ToListAsync());
        }
        [HttpGet]
        public IActionResult UserLogin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UserLoginAsync(string email, string password)
        {

            var userModel = await _context.User.ToListAsync();
            if (userModel != null) 
            {
                foreach (var user in userModel)
                {
                    if (user.Password == password && user.Email== email)
                    {
                        HttpContext.Session.SetString("id", (user.Id).ToString());
                        return View("Index", await _context.House.ToListAsync());
                    }   
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View();
        }
        [HttpGet]
        public IActionResult Admin()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Admin(string name,string password)
        {
            if(name=="rbk@gmail.com" && password == "rbk12345")
            {
                return View("Manage");
            }
            return View();
        }

        public async Task<IActionResult> AddCart(int id)
        {
            int x = id;
            if (HttpContext.Session.GetString("id") == null)
            {
                return RedirectToAction("UserLogin");
            }
            var productModel = await _context.House.FindAsync(id);
            Cart c = new Cart();
            c.HouseId = x;
            c.UserId = Int32.Parse(HttpContext.Session.GetString("id"));
            c.Categery = productModel.Categery;
            c.Name = productModel.Name;
            c.ImageUrl = productModel.ImageUrl;
            c.Price = productModel.Price;
            c.Details = productModel.Details;
            _context.cart.Add(c);
            await _context.SaveChangesAsync();
            return View("Index", await _context.House.ToListAsync());
        }

        public async Task<IActionResult> AddFavourite(int id)
        {
            int x = id;
            if (HttpContext.Session.GetString("id") == null)
            {
                return RedirectToAction("UserLogin");
            }
            var productModel = await _context.House.FindAsync(id);
            Favourite c = new Favourite();
            c.HouseId = x;
            c.UserId = Int32.Parse(HttpContext.Session.GetString("id"));
            c.Categery = productModel.Categery;
            c.Name = productModel.Name;
            c.ImageUrl = productModel.ImageUrl;
            c.Price = productModel.Price;
            c.Details = productModel.Details;
            _context.favourite.Add(c);
            await _context.SaveChangesAsync();
            return View("Index", await _context.House.ToListAsync());
        }
        public async Task<IActionResult> UserProfile()
        {

            if (HttpContext.Session.GetString("id") == null)
            {
                return RedirectToAction("UserLogin");
            }

            var userModel = await _context.User
                .FirstOrDefaultAsync(m => m.Id == Int32.Parse(HttpContext.Session.GetString("id")));
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userModel = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        // POST: UserModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            HttpContext.Session.Remove("id");
            var userModel = await _context.User.FindAsync(id);
            _context.User.Remove(userModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Cart()
        {
            if (HttpContext.Session.GetString("id") == null)
            {
                return RedirectToAction("UserLogin");
            }
            float amount = 0;
            int id = Int32.Parse(HttpContext.Session.GetString("id"));
            var groupedData = _context.cart.Where(d => d.UserId== id).ToList();
            foreach(var u in groupedData)
            {
                amount = amount + u.Price;
            }
            ViewBag.amount = amount.ToString();
            return View(groupedData);
        }
        public async Task<IActionResult> DeleteCart(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int uid = Int32.Parse(HttpContext.Session.GetString("id"));
            var c =await _context.cart.FirstOrDefaultAsync(m => m.HouseId == id);

            if (c != null)
            {
                _context.cart.Remove(c);
                await _context.SaveChangesAsync();
            }
            float amount = 0;
            
            var groupedData = _context.cart.Where(d => d.UserId == uid).ToList();
            foreach (var u in groupedData)
            {
                amount = amount + u.Price;
            }
            ViewBag.amount = amount.ToString();
            return View("Cart",groupedData);
            
        }
        [HttpPost]
        public async Task<IActionResult> OrderAsync()
        {
            int uid = Int32.Parse(HttpContext.Session.GetString("id"));
            float amount = 0;
            var groupedData = _context.cart.Where(d => d.UserId == uid).ToList();
            foreach (var u in groupedData)
            {
                amount = amount + u.Price;
            }
            ViewBag.amount = amount.ToString();
            var userModel = await _context.User.FindAsync(uid);
            ViewBag.address = userModel.Address;
            return View(groupedData);
        }

        public async Task<IActionResult> PlaceOrderAsync()
        {
            float amount = 0;
            List<string> temp=new List<string>();
            int uid = Int32.Parse(HttpContext.Session.GetString("id"));
            var user = await _context.User
                 .FirstOrDefaultAsync(m => m.Id == uid);
            var groupedData = _context.cart.Where(d => d.UserId == uid).ToList();
            foreach (var u in groupedData)
            {
                var productModel = await _context.House
                  .FirstOrDefaultAsync(m => m.Id == u.HouseId);
                temp.Add(productModel.Name);
                amount = amount + productModel.Price;
                _context.House.Update(productModel);
                await _context.SaveChangesAsync();
            }
            string combindedString = string.Join(",", temp);
            OrderModel o = new OrderModel();
            o.UserName = user.Name;
            o.HouseName = combindedString;
            o.Amount = amount;
            o.Address = user.Address;
            o.UserId = uid;
            o.date= DateTime.Now;
            _context.Order.Add(o);
            await _context.SaveChangesAsync();
            while (true)
            {
                var c = await _context.cart.FirstOrDefaultAsync(m => m.UserId == uid);

                if (c == null)
                {
                    break;
                }
                _context.cart.Remove(c);
                await _context.SaveChangesAsync();
            }
           
            return View("Index",await _context.House.ToListAsync());
        }
        public IActionResult Favourite()
        {
            if (HttpContext.Session.GetString("id") == null)
            {
                return RedirectToAction("UserLogin");
            }
            int id = Int32.Parse(HttpContext.Session.GetString("id"));
            var groupedData = _context.favourite.Where(d => d.UserId == id).ToList();
            
            
            return View(groupedData);
        }
        [HttpPost]
        public async Task<IActionResult> SearchAsync(string text)
        {
            if(text=="All")
            {
                return View("Index", await _context.House.ToListAsync());
            }
            var groupedData = _context.House.Where(d => d.Categery== text).ToList();
            return View("Index", groupedData);
        }
        
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("id");
            return View("UserLogin");
        }
        public async Task<IActionResult> DeleteFavourite(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int uid = Int32.Parse(HttpContext.Session.GetString("id"));
            var c = await _context.favourite.FirstOrDefaultAsync(m => m.HouseId == id);

            if (c != null)
            {
                _context.favourite.Remove(c);
                await _context.SaveChangesAsync();
            }
       

            var groupedData = _context.favourite.Where(d => d.UserId == uid).ToList();
            
            return View("Favourite", groupedData);

        }

        public async Task<IActionResult> AdminOrderAsync()
        {
            return View(await _context.Order.ToListAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
