﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ColmanAppStore.Data;
using ColmanAppStore.Models;

namespace ColmanAppStore.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ColmanAppStoreContext _context;

        public PaymentsController(ColmanAppStoreContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var colmanAppStoreContext = _context.Payment.Include(p => p.App).Include(p => p.PaymentMethod);
            return View(await colmanAppStoreContext.ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment
                .Include(p => p.App)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create(int id)
        {
            var usr = _context.User.Include(u => u.PaymentMethods).Include(u => u.AppListUser);
            ViewData["AppId"] = id;
            foreach (var item in _context.Apps)
            {
                if (item.Id == id)
                {
                    ViewData["App"] = item;
                    break;
                }
            }

            String userName = User.Identity.Name;
            User connectedUser = null;
            foreach (var item in _context.User)
            {
                if (item.Name.Equals(userName))
                {
                    connectedUser = item;
                    break;
                }
            }

            List<PaymentMethod> pm = new List<PaymentMethod>();
            foreach (var item in usr)
            {
                if (item.Equals(connectedUser))
                {
                    foreach (var us in item.PaymentMethods)
                    {
                        pm.Add(us);
                    }
                    break;
                }
            }

            ViewData["UserId"] = new SelectList(_context.User, "Id", "Name");
            ViewData["PaymentMethodId"] = new SelectList(pm, "Id", "CardNumber"); 

            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,City,PaymentMethodId,AppId")] Payment payment, string userName)
        {
            if (ModelState.IsValid)
            {
                App purchasedApp = null;
                foreach(var item in _context.Apps)
                {
                    if(item.Id==payment.AppId)
                    {
                        purchasedApp = item;
                        break;
                    }
                }


                var usr = _context.User.Include(u => u.PaymentMethods).Include(u => u.AppListUser);
                foreach (var item in usr)
                {
                    if(item.Name.Equals(userName))
                    {
                        item.AppListUser = new List<App>();
                        if (item.AppListUser == null)
                            item.AppListUser = new List<App>();
                        item.AppListUser.Add(purchasedApp);
                        _context.Update(item);
                        break;
                    }
                }

                payment.Id = 0; //be updated after added to DB
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppId"] = new SelectList(_context.Apps, "Id", "Name", payment.AppId);
            return View(payment);
        }


        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["AppId"] = new SelectList(_context.Apps, "Id", "Name", payment.AppId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethod, "Id", "ExpiredDate", payment.PaymentMethodId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,City,PaymentMethodId,AppId")] Payment payment)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
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
            ViewData["AppId"] = new SelectList(_context.Apps, "Id", "Name", payment.AppId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethod, "Id", "ExpiredDate", payment.PaymentMethodId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment
                .Include(p => p.App)
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payment.FindAsync(id);

            App purchasedApp = null;
            foreach (var item in _context.Apps)
            {
                if (item.Id == payment.AppId)
                {
                    purchasedApp = item;
                    break;
                }
            }

            String userName = User.Identity.Name;
            var usr = _context.User.Include(u => u.PaymentMethods).Include(u => u.AppListUser);
            foreach(var item in usr)
            {
                if(item.Name.Equals(userName))
                {
                    item.AppListUser.Remove(purchasedApp);
                    _context.Update(item);
                    break;
                }
            }

            _context.Payment.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payment.Any(e => e.Id == id);
        }
    }
}
