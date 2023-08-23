﻿using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;
        public StaffController()
        {
            _httpClient = new HttpClient();
        }

        //button choose image
        [HttpPost]
        public IActionResult ProcessUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json("No file uploaded");
            }

            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Json("/images/" + fileName);
        }

      
        //View List
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/AdminApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var staff = JsonConvert.DeserializeObject<List<Staff>>(responseContent);

                return View(staff);
            }
            else
            {
                var staffList = await db.Staff
                   .Include(s => s.Branch)
                   .Include(s => s.Role)
                   .ToListAsync();
                return View(staffList);
            }
        }


        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.TryGetValue("RoleId", out byte[] roleIdBytes))
            {
                int roleId = BitConverter.ToInt32(roleIdBytes, 0);
                ViewBag.UserRole = roleId;
                // Nếu bạn muốn thực hiện thêm xử lý, ví dụ như truy vấn thông tin Role từ CSDL dựa trên roleId, thì bạn có thể thực hiện ở đây.
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Staff loginModel)
        {
            var apiUrl = "https://localhost:7109/api/AdminApi/login";

            var json = JsonConvert.SerializeObject(loginModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<Role>(responseContent);

                // Store the user's role ID in session
                HttpContext.Session.SetInt32("RoleId", user.RoleId);

                // Set UserRole in ViewBag
                TempData["UserRole"] = user.RoleId;

                return RedirectToAction("Index", "Staff"); // Redirect to Index action of Home controller
            }
            else
            {
                ViewData["ErrorMessage"] = "Invalid username or password";
                return View("Login", loginModel); // Return the Login view with the error message
            }
        }



        //register
        public IActionResult Register()
        {
            var roles = db.Roles.ToList(); 
            var branches = db.Branches.ToList(); 

            ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
            ViewBag.Branches = new SelectList(branches, "BranchId", "Address");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(Staff registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/AdminApi/register";

            if (string.IsNullOrEmpty(registrationModel.Name))
            {
                ModelState.AddModelError("Name", "Name is required.");
            }

            if (string.IsNullOrEmpty(registrationModel.Username))
            {
                ModelState.AddModelError("Username", "Username is required.");
            }
            if (string.IsNullOrEmpty(registrationModel.Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }
            if (string.IsNullOrEmpty(registrationModel.Avatar))
            {
                ModelState.AddModelError("Avatar", "Avatar is required.");
            }
            if (string.IsNullOrEmpty(registrationModel.Phone))
            {
                ModelState.AddModelError("Phone", "Phone is required.");
            }
            if (string.IsNullOrEmpty(registrationModel.Address))
            {
                ModelState.AddModelError("Address", "Address is required.");
            }
            if (string.IsNullOrEmpty(registrationModel.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            if (string.IsNullOrEmpty(registrationModel.Phone))
            {
                ModelState.AddModelError("Phone", "Phone is required.");
            }
            else
            {
                var phoneRegex = new Regex(@"^(03|05|07|08|09|01[2|6|8|9])(?!84)[0-9]{8}$");
                if (!phoneRegex.IsMatch(registrationModel.Phone) || registrationModel.Phone.Length > 10)
                {
                    ModelState.AddModelError("Phone", "Invalid Vietnamese phone number");
                }
            }
            if (string.IsNullOrEmpty(Request.Form["BranchId"]))
            {
                ModelState.AddModelError("BranchId", "Branch is required.");
            }

            if (string.IsNullOrEmpty(Request.Form["RoleId"]))
            {
                ModelState.AddModelError("RoleId", "Role is required.");
            }


            if (ModelState.IsValid)
            {
                // Set the "Status" property to true by default if not explicitly set
                registrationModel.Status = Request.Form["Status"] == "true";
                var json = JsonConvert.SerializeObject(registrationModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
           
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("API Response Content: " + responseContent);

                    dynamic errorResponse = JsonConvert.DeserializeObject(responseContent);

                    if (errorResponse != null)
                    {
                        ModelState.AddModelError("", errorResponse.Message);

                        if (errorResponse.Errors != null)
                        {
                            foreach (var error in errorResponse.Errors)
                            {
                                ModelState.AddModelError("", error.ToString());
                            }
                        }
                    }

                    var roles = db.Roles.ToList();
                    var branches = db.Branches.ToList();

                    ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
                    ViewBag.Branches = new SelectList(branches, "BranchId", "Address");

                    return View(registrationModel);
                }
            }
            else
            {
                var roles = db.Roles.ToList();
                var branches = db.Branches.ToList();

                ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
                ViewBag.Branches = new SelectList(branches, "BranchId", "Address");

                return View(registrationModel);
            }
        }


        //Delete
        public async Task<IActionResult> Delete(int staffId)
        {
            var apiUrl = $"https://localhost:7109/api/AdminApi/delete/{staffId}";

            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index"); 
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API Response Content: " + responseContent);

                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

                ModelState.AddModelError("", errorResponse.ToString());
                return RedirectToAction("Index");
            }
        }

        //edit
        [HttpGet]
        public IActionResult Edit(int staffId)
        {
            var staff = db.Staff.Find(staffId);
            var roles = db.Roles.ToList();
            var branches = db.Branches.ToList();

            ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
            ViewBag.Branches = new SelectList(branches, "BranchId", "Address");
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int staffId, Staff updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
            updateModel.Status = Request.Form["Status"] == "true";
            var apiUrl = $"https://localhost:7109/api/AdminApi/update/{staffId}";

            var json = JsonConvert.SerializeObject(updateModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API Response Content: " + responseContent);

                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

                ModelState.AddModelError("", errorResponse.ToString());
                return View(updateModel);
            }
        }

        //detail
        [HttpGet]
        public async Task<IActionResult> Detail(int staffId)
        {
            var apiUrl = $"https://localhost:7109/api/AdminApi/detail/{staffId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var StaffDetail = JsonConvert.DeserializeObject<Staff>(responseContent);

                return View(StaffDetail);
            }
            else
            {
                return RedirectToAction("Index"); 
            }
        }
    }
}