using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vh_project.Application;
using vh_project.Application.Contracts.Persistence;
using vh_project.Domain.Models;

using vh_project.Infrastructure.Common;


namespace vh_project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = CustomRole.MasterAdmin + "," + CustomRole.Admin)]
    public class BrandController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        //private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BrandController> _ilogger ;
        public BrandController(IUnitofWork unitofWork, IWebHostEnvironment webHostEnvironment, ILogger<BrandController> ilogger)
        {
            _unitofWork = unitofWork;
            _webHostEnvironment = webHostEnvironment;
            _ilogger = ilogger; 
        }
        [HttpGet]
        public async Task<IActionResult> Brand()
        {

            try
            {
                List<BrandDataModel> brands = await _unitofWork.Brand.GetAllAsync();

                _ilogger.LogInformation("Brand Data fetched Successfully");
                //throw new ArgumentException();
                return View(brands);
                
            }
            catch(Exception ex)
            {
                _ilogger.LogError("Something went wrong");
                return View();
            }
            
       
        }
        [HttpGet]
        public IActionResult Create()
        {


            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(BrandDataModel Brand)
        {

            string Webrootpath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;


            if (files.Count > 0)
            {


                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(Webrootpath, @"images\brand");
                string FileExtension = Path.GetExtension(files[0].FileName);


                using (var filestream = new FileStream(Path.Combine(upload, newFileName + FileExtension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                Brand.BrandLogo = @"\images\brand\" + newFileName + FileExtension;

            }

            if (ModelState.IsValid)
            {
                //_dbContext.Brand.Add(Brand);
                //_dbContext.SaveChanges();
                await _unitofWork.Brand.Create(Brand);
                await _unitofWork.SaveAsAsync();
                TempData["success"] = Common.rec_created;
                return RedirectToAction(nameof(Brand));
            }
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            //var brands = _dbContext.Brand.FirstOrDefault(x => x.Id == id);
            BrandDataModel brands = await _unitofWork.Brand.GetByIdAsync(id);
            return View(brands);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            BrandDataModel brands = await _unitofWork.Brand.GetByIdAsync(id);
            return View(brands);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(BrandDataModel Brand)
        {
            string Webrootpath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;


            if (files.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(Webrootpath, @"images\brand");
                string FileExtension = Path.GetExtension(files[0].FileName);
                //var brands = _dbContext.Brand.AsNoTracking().FirstOrDefault(x => x.Id == Brand.Id);
                var brands = await _unitofWork.Brand.GetByIdAsync(Brand.Id);
                if (brands.BrandLogo != null)
                {
                    var oldimgpath = Path.Combine(Webrootpath, brands.BrandLogo.Trim('\\'));
                    if (System.IO.File.Exists(oldimgpath))
                    {
                        System.IO.File.Delete(oldimgpath);
                    }
                }
                using (var filestream = new FileStream(Path.Combine(upload, newFileName + FileExtension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                Brand.BrandLogo = @"\images\brand\" + newFileName + FileExtension;

            }

            if (ModelState.IsValid)
            {
                //var brands = _dbContext.Brand.AsNoTracking().FirstOrDefault(x => x.Id == Brand.Id);
                //if (Brand.BrandLogo != null)
                //{
                //    brands.BrandLogo = Brand.BrandLogo;
                //}
                //brands.Name = Brand.Name;
                //brands.Description = Brand.Description;
                //brands.Publishedyear = Brand.Publishedyear;
                //_dbContext.Update(brands);
                //_dbContext.SaveChanges();
                await _unitofWork.Brand.Update(Brand);
                await _unitofWork.SaveAsAsync();
                TempData["Update"] = Common.rec_updated;
                return RedirectToAction(nameof(Brand));

            }
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var brands = await _unitofWork.Brand.GetByIdAsync(id);

            return PartialView(brands);
        }
        [HttpPost]
        public async Task<ActionResult> Deletedata(Guid id)
        {
            var brand = await _unitofWork.Brand.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            await _unitofWork.Brand.Delete(brand);
            await _unitofWork.SaveAsAsync();
            TempData["Delete"] = ClsGlobalVariables.rec_deleted;
            return RedirectToAction("Brand");
        }
    }
}
