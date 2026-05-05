using InzV3.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace InzV3.Controllers
{
    [Authorize (Roles = "Admin, Pracownik Serwisu")]
    public class DevicesController : Controller
    {
        private ApplicationDbContext context;
        public DevicesController()
        {
            context = new ApplicationDbContext();
        }
        protected override void Dispose(bool disposing)
        {
            context.Dispose();
        }
        // GET: Devices
        // wyszukiwarka filtry i sama tabela urządzeń
        public ActionResult Index(string searchString, int? categoryFilter, List<int> featureFilters, int? minRating, string assignFilter,string statusFilter, string sortOrder)
        {
            var devicesQuery = context.Devices.Include(d=>d.User).Include(d => d.Charachteristics.Select(c => c.DevFeature)).AsQueryable();
            if(!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.ToLower();
                devicesQuery = devicesQuery.Where(d =>
                    d.brand.ToLower().Contains(searchString) ||
                    d.model.ToLower().Contains(searchString) ||
                    d.serial_num.ToLower().Contains(searchString) ||
                    d.id_device.ToString().Contains(searchString));
            }
            // Kategorie
            if (categoryFilter.HasValue)
            {
                devicesQuery = devicesQuery.Where(d => d.Charachteristics.Any(c => c.id_category == categoryFilter.Value));
            }
            // cechy szczególne
            if (featureFilters != null && featureFilters.Any())
            {
                foreach (var featureId in featureFilters)
                {
                    devicesQuery = devicesQuery.Where(d => d.Charachteristics.Any(c => c.id_dev_feature == featureId));
                }
            }
            // Oceny - najniższa ocena musi być przynajmiej taka jak minRating
            if (minRating.HasValue)
            {
                var devicesWithHigherRatings = context.DeviceRating
                    .GroupBy(r => r.id_device)
                    .Where(g => g.Min(r => r.rating_value) >= minRating.Value)
                    .Select(g => g.Key)
                    .ToList();
                devicesQuery=devicesQuery.Where(d=> devicesWithHigherRatings.Contains(d.id_device));
            }
            // Przypisanie do konkretnego użytkownika
            if(!string.IsNullOrEmpty(assignFilter))
            {
                if (assignFilter=="Nieprzypisane")
                {
                    devicesQuery = devicesQuery.Where(d => string.IsNullOrEmpty(d.id_user));
                }
                else
                {
                    devicesQuery = devicesQuery.Where(d => d.id_user== assignFilter);
                }
            }
            // Filtrowanie po statusie
            if (!string.IsNullOrEmpty(statusFilter))
            {
                devicesQuery = devicesQuery.Where(d => d.status == statusFilter);
            }

            // Sortowanie
            switch (sortOrder)
            {
                case "id_desc":
                    devicesQuery = devicesQuery.OrderByDescending(d => d.id_device);
                    break;
                case "warranty_asc":
                    devicesQuery = devicesQuery.OrderBy(d => d.warranty);
                    break;
                case "warranty_desc":
                    devicesQuery = devicesQuery.OrderByDescending(d => d.warranty);
                    break;
                default:
                    devicesQuery = devicesQuery.OrderBy(d => d.id_device);
                    break;
            }
            ViewBag.Categories=new SelectList(context.DevCategories.ToList(), "id_category", "category_name");
            ViewBag.Features=new SelectList(context.DevFeatures.ToList(), "id_dev_feature", "dev_feature_name");
            var users = context.Users.ToList().Select(u => new
            {
                Id = u.Id,
                Name = $"{u.Email} | {u.FirstName} {u.LastName}"
            }).ToList();
            ViewBag.Users = new SelectList(users, "Id", "Name");
            ViewBag.Statuses= new SelectList(context.Devices.Select(d=>d.status).Distinct().ToList());

            ViewBag.CurrentCategory = categoryFilter;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentMinRating = minRating;
            ViewBag.CurrentAssign = assignFilter;
            ViewBag.CurrentStatus = statusFilter;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFeatureFilters = featureFilters;
            return View("Index", devicesQuery.ToList());
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var device = context.Devices.Include(d => d.Charachteristics.Select(c => c.DevFeature))
                .Include(d => d.User).SingleOrDefault(d => d.id_device == id);
            if(device == null)
            {
                return HttpNotFound();
            }
            //Przypisane oprogramowanie
            ViewBag.Software = context.Software.Where(s => s.assigned_device == id).ToList();
            //kategorie ocen
            ViewBag.Ratings = context.DeviceRating.Where(r => r.id_device == id).ToList();
            return View("Details", device);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRatings(FormCollection form)
        {
            int id_device = int.Parse(form["id_device"]);
            var ratings=context.DeviceRating.Where(r => r.id_device == id_device).ToList();

            foreach(var rating in ratings)
            {
                string val = form["rating_" + rating.id_rating];
                if(!string.IsNullOrEmpty(val))
                {
                    rating.rating_value = int.Parse(val);
                }
            }
            context.SaveChanges();
            return RedirectToAction("Details", new { id = id_device });
        }




        [HttpGet]
        public ActionResult CreateOne()
        {
            //Łączenie z danymi z tabel od kategorii
            ViewBag.Categories = new SelectList(context.DevCategories, "id_category", "category_name");
            ViewBag.Features = context.DevFeatures.ToList();

            var users=context.Users.ToList().Select(u => new
            {
                RealId=u.Id,
                DisplayText=$"{u.Email} | {u.FirstName} {u.LastName}"
            }).ToList();
            ViewBag.Users = new SelectList(users, "RealId", "DisplayText");

            return View("DeviceCreate", new DeviceModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        //zarządzanie tworzeniem i edycją urządzenia
        public ActionResult ProcessCreateOne(DeviceModel deviceModel, int? id_category, FormCollection form)
        {
            string userIdStr = deviceModel.id_user?.ToString();
            bool isUserAssigned = !string.IsNullOrWhiteSpace(userIdStr) && userIdStr != "0" && userIdStr != "-1";

            deviceModel.status = isUserAssigned ? "W użyciu" : "Nieprzypisany";
            ModelState.Remove("status");
            if (!isUserAssigned)
            {
                ModelState.Remove("id_user");
            }

            //Tworzenie wielu lub jednego z urządzeń.
            if (deviceModel.id_device == -1 || deviceModel.id_device == 0)
            {
                ModelState.Remove("serial_num");
                string[] serialNumbers = form.GetValues("serial_numbers");
                if (serialNumbers == null || serialNumbers.Length == 0 || serialNumbers.All(string.IsNullOrWhiteSpace))
                {
                    ModelState.AddModelError("", "Podaj przynajmniej jeden numer seryjny.");
                }
                else
                {
                    var nonEmptySerials= serialNumbers.Where(sn => !string.IsNullOrWhiteSpace(sn)).Select(sn => sn.Trim()).ToList();
                    if(nonEmptySerials.Count != nonEmptySerials.Distinct().Count())
                    {
                        ModelState.AddModelError("", "Numery seryjne nie mogą się powtarzać.");
                    }

                    foreach (var sn in serialNumbers)
                    {
                        if (string.IsNullOrWhiteSpace(sn)) continue;
                        string cleanSn = sn.Trim();
                        if (context.Devices.Any(d => d.serial_num == cleanSn))
                        {
                            ModelState.AddModelError("", $"Urządzenie o numerze seryjnym {cleanSn} już istnieje.");
                        }
                    }
                }
                if (!ModelState.IsValid)
                {
                    PrepareViewBags();
                    return View("DeviceCreate", deviceModel);
                }

                string[] featuresIds = form.GetValues("feature_ids");
                string[] featureValues = form.GetValues("feature_values");

                foreach (var sn in serialNumbers)
                {
                    if (string.IsNullOrWhiteSpace(sn)) continue;
                    DeviceModel newDevice = new DeviceModel
                    {
                        brand = deviceModel.brand,
                        model = deviceModel.model,
                        serial_num = sn.Trim(),
                        status = deviceModel.status,
                        warranty = deviceModel.warranty,
                        id_user = deviceModel.id_user
                    };
                    context.Devices.Add(newDevice);
                    context.SaveChanges();
                    if (featuresIds != null && featureValues != null && id_category.HasValue)
                    {
                        for (int i = 0; i < featuresIds.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(featuresIds[i]) && !string.IsNullOrWhiteSpace(featureValues[i]))
                            {
                                DevCharacteristic newChar = new DevCharacteristic
                                {
                                    id_device = newDevice.id_device,
                                    id_category = id_category.Value,
                                    id_dev_feature = int.Parse(featuresIds[i]),
                                    dev_feature_value = featureValues[i]
                                };
                                context.DevCharacteristics.Add(newChar);
                            }
                        }
                        context.SaveChanges();
                    }
                    string[] ratingCategories = form.GetValues("rating_categories");
                    if (ratingCategories != null)
                    {
                        foreach (var rc in ratingCategories)
                        {
                            if (!string.IsNullOrWhiteSpace(rc))
                            {
                                DeviceRating newRating = new DeviceRating
                                {
                                    id_device = newDevice.id_device,
                                    category_name = rc.Trim(),
                                    rating_value = 5
                                };
                                context.DeviceRating.Add(newRating);
                            }
                        }
                        context.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            else
            {
                bool isSerialDuplicate = context.Devices.Any(d => d.serial_num == deviceModel.serial_num && d.id_device != deviceModel.id_device);
                if (isSerialDuplicate)
                {
                    ModelState.AddModelError("serial_num", "Urządzenie o podanym numerze seryjnym już istnieje w bazie.");
                }
                if (!ModelState.IsValid)
                {
                    PrepareViewBags();
                    return View("DeviceCreate", deviceModel);
                }
                DeviceModel device = context.Devices.SingleOrDefault(d => d.id_device == deviceModel.id_device);
                if (device != null)
                {
                    device.brand = deviceModel.brand;
                    device.model = deviceModel.model;
                    device.serial_num = deviceModel.serial_num;
                    device.status = deviceModel.status;
                    device.warranty = deviceModel.warranty;
                    device.id_user = deviceModel.id_user;
                    context.SaveChanges();
                    return RedirectToAction("Details", new { id = device.id_device });
                }
                return HttpNotFound();
            }

        }
        private void PrepareViewBags()
        {
            ViewBag.Categories = new SelectList(context.DevCategories, "id_category", "category_name");
            ViewBag.Features = context.DevFeatures.ToList();
            var users = context.Users.ToList().Select(u => new
            {
                RealId = u.Id,
                DisplayText = $"{u.Email} | {u.FirstName} {u.LastName}"
            }).ToList();
            ViewBag.Users = new SelectList(users, "RealId", "DisplayText");
        }
        public ActionResult Edit(int id)
        {
            DeviceModel device = context.Devices.SingleOrDefault(d => d.id_device == id);
            ViewBag.Categories = new SelectList(context.DevCategories, "id_category", "category_name");
            ViewBag.Features = context.DevFeatures.ToList();
            var users = context.Users.ToList().Select(u => new
            {
                RealId = u.Id,
                DisplayText = $"{u.Email} | {u.FirstName} {u.LastName}"
            }).ToList();
            ViewBag.Users = new SelectList(users, "RealId", "DisplayText");
            return View("DeviceCreate", device);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DeviceModel device = context.Devices.SingleOrDefault(d => d.id_device == id);
            if (device != null)
            {
                if(!string.IsNullOrEmpty(device.id_user))
                {
                    TempData["ErrorMessage"] = $"Nie można usunąć urządzenia (ID: {device.id_device}), ponieważ jest przypisane do użytkownika";
                    return RedirectToAction("Index");
                }
                context.Devices.Remove(device);
                context.SaveChanges();
                TempData["SuccessMessage"] = $"Urządzenie zostało pomyślnie usunięte.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult SearchName(string searchPh)
        {
            if (string.IsNullOrWhiteSpace(searchPh))
                return RedirectToAction("Index");
            searchPh = searchPh.ToLower();
            var devices = context.Devices.Where(g =>
            g.brand.ToLower().Contains(searchPh) ||
            g.model.ToLower().Contains(searchPh) ||
            g.id_user.ToString().Contains(searchPh) ||
            g.id_device.ToString().Contains(searchPh)
            );
            return View("Index",devices);
        }
        
        // Pobieranie etykiety urządzenia
        [HttpGet]
        public ActionResult GetDeviceLabel(int id, bool download = false)
        {
            var device = context.Devices.SingleOrDefault(d => d.id_device == id);
            if (device == null)
            {
                return HttpNotFound("Nie ma takiego urzązenia");
            }
            // generujemy adres url do szczegółów urządzenia, który będzie potem użyty do tworzenia QR kodu
            string deviceUrl = Url.Action("Details", "Devices", new { id = device.id_device }, protocol: Request.Url.Scheme);


            // Tworzenie Qr kodu
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(deviceUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            Bitmap qrBitmap = qrCode.GetGraphic(10);

            // Dodanie id sprzętu do etykiety
            int textPadding = 40;
            Bitmap labelBitmap = new Bitmap(qrBitmap.Width, qrBitmap.Height + textPadding);
            using (Graphics g = Graphics.FromImage(labelBitmap))
            {
                g.Clear(Color.White);
                g.DrawImage(qrBitmap, 0, 0);
                using (Font font = new Font("Arial", 12, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    RectangleF textRect = new RectangleF(0, qrBitmap.Height, labelBitmap.Width, textPadding);
                    g.DrawString($"ID: {device.id_device}", font, brush, textRect, format);
                }

            }
            using (MemoryStream ms = new MemoryStream())
            {
                labelBitmap.Save(ms, ImageFormat.Png);
                if (download)
                {
                    return File(ms.ToArray(), "image/png", $"Etykieta_id_{device.id_device}.png");
                }
                else
                {
                    return File(ms.ToArray(), "image/png");
                }
            }
        }
    }
}
