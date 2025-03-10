using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using vh_project.Application.Contracts.Persistence;
using vh_project.Domain.Models;
using vh_project.Domain.ViewModel;
using vh_project.Application.ExtensionMethods;
using System.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authorization;



namespace vh_project.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _unitofWork;

        public HomeController(ILogger<HomeController> logger,IUnitofWork unitofWork)
        {
            _logger = logger;
            _unitofWork = unitofWork;   
        }

        [HttpGet]

        public async Task<IActionResult> Index(int? page,bool resetFilter = false)
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
            List<Post> posts;


            if (resetFilter)
            {

                TempData.Remove("FilteredPosts");
                TempData.Remove("SelectedBrandId");
                TempData.Remove("SelectedVehicleTypeId");
            }

            if (TempData.ContainsKey("FilteredPosts"))
            {
                posts = TempData.Get<List<Post>>("FilteredPosts");
                TempData.Keep("FilteredPosts");

            }
            else
            {
                posts = await _unitofWork.Post.GetAllPosts();
            }
            int pageSize = 3;
            int pageNumber = page ?? 1;

            int totalItem = posts.Count;
            int totalPages = (int)Math.Ceiling((double)totalItem / pageSize);


            ///ViewBag
            ViewBag.totalPages = totalPages;
            ViewBag.Currentpage = pageNumber;

            //PaginationRecord
            var pagedPosts =  posts.Skip((pageNumber - 1) *  pageSize).Take(pageSize).ToList();

            //Http Session Url;
            HttpContext.Session.SetString("PreviousUrl", HttpContext.Request.Path);

            HomePostVM homePostVM = new HomePostVM
            {
                Posts = pagedPosts,
                BrandList = BrandList,
                VehicleTypeList = VehicleTypeList,
                BrandId = (Guid?)TempData["SelectedBrandId"],
                VehicleTypeId = (Guid?)TempData["SelectedVehicleTypeId"]
            };

            return View(homePostVM);
        }


        [HttpPost]
        public async Task<IActionResult> Index(HomePostVM homePostVM)
        {
            var posts = await _unitofWork.Post.GetAllPosts(homePostVM.searchBox,homePostVM.BrandId,homePostVM.VehicleTypeId);

            TempData.Put("FilteredPosts", posts);
            TempData["SelectedBrandId"] = homePostVM.BrandId;
            TempData["SelectedVehicleTypeId"] = homePostVM.VehicleTypeId;

            return RedirectToAction("Index", new {page = 1,resetFilter = false});

        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id , int? page)

        {
            Post post = await _unitofWork.Post.GetPostById(id);

            List<Post> ListOfPosts = new List<Post>();
            if(post != null)
            {
                ListOfPosts = await _unitofWork.Post.GetAllPosts(post.Id, post.BrandId);
            }
            ViewBag.Currentpage = page;

            CustomerDetailsVM customerDetailsVM = new CustomerDetailsVM
            {
                Post = post,
                ListOfPosts = ListOfPosts
            };
            return View(customerDetailsVM);
        }

        public IActionResult GoBack(int? page)
        {
            string? previousUrl = HttpContext.Session.GetString("PreviousUrl");
            if (!string.IsNullOrEmpty(previousUrl))
            {
                if (page.HasValue)
                {
                    previousUrl = QueryHelpers.AddQueryString(previousUrl, "page", page.Value.ToString());
                }
                HttpContext.Session.Remove("PreviousUrl");

                return Redirect(previousUrl);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
