using DataModel;
using jQuery_File_Upload.MVC5.Helpers;
using jQuery_File_Upload.MVC5.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace jQuery_File_Upload.MVC5.Controllers
{
    public class FileUploadController : Controller
    {
        FilesHelper filesHelper;
        String tempPath = "~/somefiles/";
        String serverMapPath = "~/Files/somefiles/";
        private string StorageRoot
        {
            get { return Path.Combine(HostingEnvironment.MapPath(serverMapPath)); }
        }
        private string UrlBase = "/Files/somefiles/";
        String DeleteURL = "/FileUpload/DeleteFile/?file=";
        String DeleteType = "GET";
        public FileUploadController()
        {
           filesHelper = new FilesHelper(DeleteURL, DeleteType, StorageRoot, UrlBase, tempPath, serverMapPath);
        }
      
        public ActionResult Index(string id = "")
        {
            // set id if empty
            if (string.IsNullOrWhiteSpace(id))
            {
                id = DateTime.Now.ToString("ddMMMyyyyhhmmss");
            }

            // pass identifier to view
            ViewBag.Id = id;

            return View();
        }
        public ActionResult Show()
        {
            JsonFiles ListOfFiles = filesHelper.GetFileList("");
            var model = new FilesViewModel()
            {
                Files = ListOfFiles.files
            };
          
            return View(model);
        }

        public ActionResult Edit(string id = "")
        {

            ViewBag.Id = id;

            return View();
        }

        [HttpPost]
        public JsonResult Upload()
        {
            var resultList = new List<ViewDataUploadFilesResult>();
           
            var CurrentContext = HttpContext;
            string idd = Request.Form["idd"];

            filesHelper.UploadAndShowResults(CurrentContext, resultList, idd);
            JsonFiles files = new JsonFiles(resultList);

            bool isEmpty = !resultList.Any();
            if (isEmpty)
            {
                return Json("Error ");
            }
            else
            {
                return Json(files);
            }
        }
        public JsonResult GetFileList(string id = "")
        {

            // set id if empty
            if (string.IsNullOrWhiteSpace(id))
            {
                id = DateTime.Now.ToString("ddMMMyyyyhhmmss");
            }

            // pass identifier to view
            ViewBag.Id = id;

            var list=filesHelper.GetFileList(id);
            return Json(list,JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteFile(string file)
        {
            filesHelper.DeleteFile(file);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }
       
    }
}