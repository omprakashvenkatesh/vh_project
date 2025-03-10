using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using vh_project.Application;
using vh_project.Application.Contracts.Persistence;
using vh_project.Application.Services;
using vh_project.Application.Services.Interface;
using vh_project.Domain.Models;
using vh_project.Domain.ViewModel;
using vh_project.Infrastructure.Common;



namespace vh_project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = CustomRole.MasterAdmin + "," + CustomRole.Admin)]
    public class PostController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        //private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserNameService _userNameService;
        public PostController(IUnitofWork unitofWork, IWebHostEnvironment webHostEnvironment, IUserNameService userNameService)
        {
            _userNameService = userNameService; 
            _unitofWork = unitofWork;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Post> Post = await _unitofWork.Post.GetAllPosts();
            return View(Post);
        }
        [HttpGet]
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> BrandList = _unitofWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });
            IEnumerable<SelectListItem> VehicleTypeList = _unitofWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.VehicleName.ToUpper(),
                Value = x.Id.ToString()
            });
            IEnumerable<SelectListItem> EngineandFuelTypeList  = Enum.GetValues(typeof(EngineAndFuelType)).Cast<EngineAndFuelType>().Select(
                x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString()
                }
                );
            IEnumerable<SelectListItem> TransmissionList = Enum.GetValues(typeof(Transmission)).Cast<Transmission>().Select(
               x => new SelectListItem
               {
                   Text = x.ToString().ToUpper(),
                   Value = ((int)x).ToString()
               }
               );

            PostVM postVM = new PostVM
            {
               post = new Post(),
               BrandList = BrandList,
               VehicleTypeList = VehicleTypeList,
               TransmissionList = TransmissionList,
               EngineandFuelTypeList= EngineandFuelTypeList


            };

            return View(postVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(PostVM postVM)
        {

            string Webrootpath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;


            if (files.Count > 0)
            {


                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(Webrootpath, @"images\post");
                string FileExtension = Path.GetExtension(files[0].FileName);


                using (var filestream = new FileStream(Path.Combine(upload, newFileName + FileExtension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                postVM.post.VehiceImage = @"\images\post\" + newFileName + FileExtension;

            }

            if (ModelState.IsValid)
            {
                //_dbContext.Post.Add(Post);
                //_dbContext.SaveChanges();
                await _unitofWork.Post.Create(postVM.post);
                await _unitofWork.SaveAsAsync();
                TempData["success"] = Common.rec_created;
                return RedirectToAction(nameof(Index));
            }
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            //var brands = _dbContext.Post.FirstOrDefault(x => x.Id == id);
            Post post = await _unitofWork.Post.GetPostById(id);
            post.CreatedBy = await _userNameService.GetUserName(post.CreatedBy);
            post.LastModifiedBy = await  _userNameService.GetUserName(post.LastModifiedBy);
            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Post post = await _unitofWork.Post.GetPostById(id);


            IEnumerable<SelectListItem> BrandList = _unitofWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });
            IEnumerable<SelectListItem> VehicleTypeList = _unitofWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.VehicleName.ToUpper(),
                Value = x.Id.ToString()
            });
            IEnumerable<SelectListItem> EngineandFuelTypeList = Enum.GetValues(typeof(EngineAndFuelType)).Cast<EngineAndFuelType>().Select(
                x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString()
                }
                );
            IEnumerable<SelectListItem> TransmissionList = Enum.GetValues(typeof(Transmission)).Cast<Transmission>().Select(
               x => new SelectListItem
               {
                   Text = x.ToString().ToUpper(),
                   Value = ((int)x).ToString()
               }
               );

            PostVM postVM = new PostVM
            {
                post = post,
                BrandList = BrandList,
                VehicleTypeList = VehicleTypeList,
                TransmissionList = TransmissionList,
                EngineandFuelTypeList = EngineandFuelTypeList


            };
            return View(postVM);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(PostVM PostVM)
        {
            string Webrootpath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;


            if (files.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(Webrootpath, @"images\post");
                string FileExtension = Path.GetExtension(files[0].FileName);
                //var brands = _dbContext.Post.AsNoTracking().FirstOrDefault(x => x.Id == Post.Id);
                var post = await _unitofWork.Post.GetByIdAsync(PostVM.post.Id);
                if (post.VehiceImage != null)
                {
                    var oldimgpath = Path.Combine(Webrootpath, post.VehiceImage.Trim('\\'));
                    if (System.IO.File.Exists(oldimgpath))
                    {
                        System.IO.File.Delete(oldimgpath);
                    }
                }
                using (var filestream = new FileStream(Path.Combine(upload, newFileName + FileExtension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                PostVM.post.VehiceImage = @"\images\post\" + newFileName + FileExtension;

            }

            if (ModelState.IsValid)
            {
                //var brands = _dbContext.Post.AsNoTracking().FirstOrDefault(x => x.Id == Post.Id);
                //if (Post.PostLogo != null)
                //{
                //    brands.PostLogo = Post.PostLogo;
                //}
                //brands.Name = Post.Name;
                //brands.Description = Post.Description;
                //brands.Publishedyear = Post.Publishedyear;
                //_dbContext.Update(brands);
                //_dbContext.SaveChanges();
                await _unitofWork.Post.Update(PostVM.post);
                await _unitofWork.SaveAsAsync();
                TempData["Update"] = Common.rec_updated;
                return RedirectToAction(nameof(Index));

            }
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _unitofWork.Post.GetPostById(id);
            IEnumerable<SelectListItem> BrandList = _unitofWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });
            IEnumerable<SelectListItem> VehicleTypeList = _unitofWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.VehicleName.ToUpper(),
                Value = x.Id.ToString()
            });
            IEnumerable<SelectListItem> EngineandFuelTypeList = Enum.GetValues(typeof(EngineAndFuelType)).Cast<EngineAndFuelType>().Select(
                x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString()
                }
                );
            IEnumerable<SelectListItem> TransmissionList = Enum.GetValues(typeof(Transmission)).Cast<Transmission>().Select(
               x => new SelectListItem
               {
                   Text = x.ToString().ToUpper(),
                   Value = ((int)x).ToString()
               }
               );

            PostVM postVM = new PostVM
            {
                post = post,
                BrandList = BrandList,
                VehicleTypeList = VehicleTypeList,
                TransmissionList = TransmissionList,
                EngineandFuelTypeList = EngineandFuelTypeList


            };
            return PartialView(postVM);
        }
        [HttpPost]
        public async Task<ActionResult> Deletedata(Post Post)
        {
            string Webrootpath = _webHostEnvironment.WebRootPath;
           
                var post = await _unitofWork.Post.GetByIdAsync(Post.Id);
                if (post.VehiceImage != null)
                {
                    var oldimgpath = Path.Combine(Webrootpath, post.VehiceImage.Trim('\\'));
                    if (System.IO.File.Exists(oldimgpath))
                    {
                        System.IO.File.Delete(oldimgpath);
                    }
                }
        
           

            await _unitofWork.Post.Delete(Post);
            await _unitofWork.SaveAsAsync();
            TempData["Delete"] = ClsGlobalVariables.rec_deleted;
            return RedirectToAction(nameof(Index));
        }
    }
}
