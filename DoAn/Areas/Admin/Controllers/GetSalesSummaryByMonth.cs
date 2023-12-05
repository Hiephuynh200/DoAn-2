﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GetSalesSummaryByMonth : Controller
    {
        private readonly DlctContext _dbContext;

        public GetSalesSummaryByMonth(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetSale()
        {

            var salesData = await _dbContext.Bills
                .Include(b => b.Billdetails)
                .ThenInclude(d => d.Product)
                .Where(b => b.CreatedAt.HasValue)
                .ToListAsync();

            await GetTotalRevenue();

            var bestSellingProducts = salesData
                .SelectMany(b => b.Billdetails)
                .GroupBy(d => d.ProductId)
                .Select(x => new
                {
                    ProductId = x.Key,
                    ProductName = x.First().Product?.Name ?? "Unknown",
                    TotalQuantity = x.Sum(d => d.Quantity ?? 0),
                    TotalRevenue = x.Sum(d => (d.Quantity ?? 0) * (d.Price ?? 0)),
                    Date = x.First().Bill?.Date
                })
                .OrderByDescending(entry => entry.TotalQuantity)
                .ToList();

            foreach (var product in bestSellingProducts)
            {
                Console.WriteLine($"Product ID: {product.ProductId}, Product Name: {product.ProductName}, Total Revenue: {product.TotalRevenue}, Date: {product.Date}");
            }

            ViewBag.BestSellingProducts = bestSellingProducts;

            return View();
        }

        public async Task<IActionResult> GetTotalRevenue()
        {
            var totalRevenue = await _dbContext.Billdetails
                .Where(d => d.Bill.CreatedAt.HasValue)
                .SumAsync(d => (d.Quantity ?? 0) * (d.Price ?? 0));

            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }
    }
}
