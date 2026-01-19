using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ButceTakipSistemi.Data;
using ButceTakipSistemi.Models;
using Microsoft.AspNetCore.Authorization;

namespace ButceTakipSistemi.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        // Parametreler ekledik: Arama metni ve Tarih aralığı
        public async Task<IActionResult> Index(string searchString, DateTime? startDate, DateTime? endDate)
        {
            // 1. Önce Veritabanı sorgusunu "Taslak" olarak oluşturuyoruz (Hemen çekmiyoruz)
            var transactionsQuery = _context.Transactions
                .Include(t => t.Category)
                .AsQueryable(); // Sorgulanabilir hale getirdik

            // 2. FİLTRELEME İŞLEMLERİ

            // Eğer arama kutusu doluysa (Kelimeye göre filtrele)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Açıklamada VEYA Kategori adında geçenleri bul
                transactionsQuery = transactionsQuery.Where(t =>
                    t.Description.Contains(searchString) ||
                    t.Category.Name.Contains(searchString));
            }

            // Başlangıç tarihi seçildiyse
            if (startDate.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.Date >= startDate.Value);
            }

            // Bitiş tarihi seçildiyse
            if (endDate.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.Date <= endDate.Value);
            }

            // 3. Sıralama ve Listeyi Çekme
            var transactions = await transactionsQuery.OrderByDescending(t => t.Date).ToListAsync();


            // --- HESAPLAMA BÖLÜMÜ (DASHBOARD) ---
            // NOT: Dashboard hesaplamalarını veritabanındaki TÜM veriye göre yapıyoruz.
            // Böylece kullanıcı filtrelese bile "Toplam Servetini" doğru görür.
            // Eğer filtrelenen sonuca göre değişsin istersen burayı güncelleyebiliriz.

            var allTransactions = await _context.Transactions.Include(t => t.Category).ToListAsync();

            // A) Genel Toplam Bakiye
            decimal totalIncome = allTransactions.Where(t => t.Category != null && t.Category.Type == 1).Sum(t => t.Amount);
            decimal totalExpense = allTransactions.Where(t => t.Category != null && t.Category.Type == 0).Sum(t => t.Amount);
            ViewBag.TotalBalance = totalIncome - totalExpense;

            // B) Bu Ayki Durum
            var thisMonthTrans = allTransactions.Where(t => t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year);
            ViewBag.IncomeMonth = thisMonthTrans.Where(t => t.Category != null && t.Category.Type == 1).Sum(t => t.Amount);
            ViewBag.ExpenseMonth = thisMonthTrans.Where(t => t.Category != null && t.Category.Type == 0).Sum(t => t.Amount);

            // C) Bu Haftaki Durum
            var last7DaysTrans = allTransactions.Where(t => t.Date >= DateTime.Today.AddDays(-7));
            ViewBag.IncomeWeek = last7DaysTrans.Where(t => t.Category != null && t.Category.Type == 1).Sum(t => t.Amount);
            ViewBag.ExpenseWeek = last7DaysTrans.Where(t => t.Category != null && t.Category.Type == 0).Sum(t => t.Amount);

            // D) Bugünkü Durum
            var todayTrans = allTransactions.Where(t => t.Date.Date == DateTime.Today);
            ViewBag.IncomeToday = todayTrans.Where(t => t.Category != null && t.Category.Type == 1).Sum(t => t.Amount);
            ViewBag.ExpenseToday = todayTrans.Where(t => t.Category != null && t.Category.Type == 0).Sum(t => t.Amount);

            // Arama kutusunda yazılanlar silinmesin diye geri gönderiyoruz
            ViewData["CurrentFilter"] = searchString;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(transactions);
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Date,CategoryId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,CategoryId")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
