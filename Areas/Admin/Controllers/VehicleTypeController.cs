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
    public class VehicleTypeController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        //private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VehicleTypeController(IUnitofWork unitofWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitofWork = unitofWork;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Brand()
        {
            List<VehicleTypeDataModel> Vehicles = await _unitofWork.VehicleType.GetAllAsync();
            return View(Vehicles);
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(VehicleTypeDataModel VehicleType)
        {
            if (ModelState.IsValid)
            {
                //_dbContext.Brand.Add(Brand);
                //_dbContext.SaveChanges();
                await _unitofWork.VehicleType.Create(VehicleType);
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
            VehicleTypeDataModel vehicleType = await _unitofWork.VehicleType.GetByIdAsync(id);
            return View(vehicleType);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            VehicleTypeDataModel vehicleType = await _unitofWork.VehicleType.GetByIdAsync(id);
            return View(vehicleType);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(VehicleTypeDataModel vehicleTypes)
        {
          
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
                await _unitofWork.VehicleType.Update(vehicleTypes);
                await _unitofWork.SaveAsAsync();
                TempData["Update"] = Common.rec_updated;
                return RedirectToAction(nameof(Brand));

            }
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var vehicletype = await _unitofWork.VehicleType.GetByIdAsync(id);

            return PartialView(vehicletype);
        }
        [HttpPost]
        public async Task<ActionResult> Deletedata(Guid id)
        {
            var vehicletype = await _unitofWork.VehicleType.GetByIdAsync(id);
            if (vehicletype == null)
            {
                return NotFound();
            }

            await _unitofWork.VehicleType.Delete(vehicletype);
            //await _unitofWork.SaveAsAsync();
            TempData["Delete"] = ClsGlobalVariables.rec_deleted;
            return RedirectToAction(nameof(Brand));
        }
    }
}
