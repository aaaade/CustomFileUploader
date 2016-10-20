using jQuery_File_Upload.MVC5.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using DataModel;

namespace jQuery_File_Upload.MVC5.Helpers
{
    public class FilesHelper
    {

        private DataContext _db = new DataContext();

        String DeleteURL = null;
        String DeleteType = null;
        String StorageRoot = null;
        String UrlBase = null;
        String tempPath = null;
        //ex:"~/Files/something/";
        String serverMapPath = null;
        public FilesHelper(String DeleteURL, String DeleteType, String StorageRoot, String UrlBase, String tempPath, String serverMapPath)
        {
            this.DeleteURL = DeleteURL;
            this.DeleteType = DeleteType;
            this.StorageRoot = StorageRoot;
            this.UrlBase = UrlBase;
            this.tempPath = tempPath;
            this.serverMapPath = serverMapPath;
        }

        public void DeleteFiles(String pathToDelete)
        {

            string path = HostingEnvironment.MapPath(pathToDelete);

            System.Diagnostics.Debug.WriteLine(path);
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.GetFiles())
                {
                    System.IO.File.Delete(fi.FullName);
                    System.Diagnostics.Debug.WriteLine(fi.Name);
                }

                di.Delete(true);
            }
        }

        public String DeleteFile(String file)
        {
            System.Diagnostics.Debug.WriteLine("DeleteFile");
            //    var req = HttpContext.Current;
            System.Diagnostics.Debug.WriteLine(file);

            String fullPath = Path.Combine(StorageRoot, file);
            System.Diagnostics.Debug.WriteLine(fullPath);
            System.Diagnostics.Debug.WriteLine(System.IO.File.Exists(fullPath));
            String thumbPath = "/" + file + "_80x80.jpg";
            String partThumb1 = Path.Combine(StorageRoot, "thumbs");
            String partThumb2 = Path.Combine(partThumb1, file + "_80x80.jpg");

            System.Diagnostics.Debug.WriteLine(partThumb2);
            System.Diagnostics.Debug.WriteLine(System.IO.File.Exists(partThumb2));
            if (System.IO.File.Exists(fullPath))
            {
                //delete thumb 
                if (System.IO.File.Exists(partThumb2))
                {
                   
                    // delete thumbs
                    System.IO.File.Delete(partThumb2);
                }

                // delete file
                System.IO.File.Delete(fullPath);

                System.IO.File.Delete(fullPath);
                String succesMessage = "Ok";
                return succesMessage;
            }
            String failMessage = "Error Delete";
            return failMessage;
        }
        public JsonFiles GetFileList(string id)
        {

            var r = new List<ViewDataUploadFilesResult>();

            String fullPath = Path.Combine(StorageRoot);
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo dir = new DirectoryInfo(fullPath);
                foreach (FileInfo file in dir.GetFiles().Where(fn => fn.Name.StartsWith(id)))
                {
                    int SizeInt = unchecked((int)file.Length);
                    r.Add(UploadResult(file.Name, SizeInt, file.FullName));
                }

            }
            JsonFiles files = new JsonFiles(r);

            return files;
        }

        public void UploadAndShowResults(HttpContextBase ContentBase, List<ViewDataUploadFilesResult> resultList, string idd)
        {
            var httpRequest = ContentBase.Request;
            System.Diagnostics.Debug.WriteLine(Directory.Exists(tempPath));

            String fullPath = Path.Combine(StorageRoot);
            Directory.CreateDirectory(fullPath);
            // Create new folder for thumbs
            Directory.CreateDirectory(fullPath + "/thumbs/");

            // do something only if files count less than or equal to 5
            if (httpRequest.Files.Count <= 5)
            {
                foreach (String inputTagName in httpRequest.Files)
                {

                    var headers = httpRequest.Headers;

                    var file = httpRequest.Files[inputTagName];
                    // System.Diagnostics.Debug.WriteLine(file.FileName);

                    if (string.IsNullOrEmpty(headers["X-File-Name"]))
                    {

                        UploadWholeFile(ContentBase, resultList, idd);
                        // SaveFile(ContentBase, inputTagName, resultList, idd);
                    }
                    else
                    {

                        UploadPartialFile(headers["X-File-Name"], ContentBase, resultList, idd);
                    }

                    Thread.Sleep(2000); // sleep for 2 seconds
                }
            }

        }
        
        private void UploadWholeFile(HttpContextBase requestContext, List<ViewDataUploadFilesResult> statuses, string idd)
        {

            var request = requestContext.Request;
            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                String pathOnServer = Path.Combine(StorageRoot);

                string fileExt = file.FileName.Split('.')[1]; // file extension

                string fname = idd + "_" + fg.GenerateUniqueCode() + "." + fileExt;
                var fullPath = Path.Combine(pathOnServer, Path.GetFileName(fname));

                // only create if file doesnt exist
                if (!File.Exists(fullPath))
                {
                    try
                    {
                        file.SaveAs(fullPath);

                    }
                    catch (Exception ex)
                    {
                        string err = ex.ToString();
                    }

                    //Create thumb
                    string[] imageArray = file.FileName.Split('.');
                    if (imageArray.Length != 0)
                    {
                        var ThumbfullPath = Path.Combine(pathOnServer, "thumbs");

                        using (MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(fullPath)))
                        {
                            String fileThumb = fname + ".80x80." + fileExt; // file.FileName + ".80x80.jpg";
                            var ThumbfullPath2 = Path.Combine(ThumbfullPath, fileThumb);


                            var thumbnail = new WebImage(stream).Resize(80, 80);
                            thumbnail.Save(ThumbfullPath2, fileExt);

                            // Save to db
                            // get doctypeId for current file
                            //int iDocTypeId = new DataContext().DocumentTypes.Where(dt => dt.Name.Trim() == "Student Doc").Select(dt => dt.Id).FirstOrDefault();
                            //SaveToDb(fname, idd, file.ContentLength, fileExt, ThumbfullPath2, iDocTypeId);

                            // sleep for 1 sec
                            Thread.Sleep(1000);
                        }
                    }
                    statuses.Add(UploadResult(fname, file.ContentLength, fullPath));

                }

            }
        }

        public void SaveToDb(string fileName, string idd, double docSize, string fileExt, string fileLocation, int docTypeId)
        {
            Document document = new Document()
            {
                OwnerName = idd,
                FileName = fileName,
                FileExtension = fileExt,
                DocumentSize = docSize,
                DocumentTypeId = docTypeId,
                Location = fileLocation
            };
            _db.Documents.Add(document);
            _db.SaveChanges();
        }

        private void UploadPartialFile(string fileName, HttpContextBase requestContext, List<ViewDataUploadFilesResult> statuses, string idd)
        {
            var request = requestContext.Request;
            if (request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var file = request.Files[0];
            var inputStream = file.InputStream;
            String patchOnServer = Path.Combine(StorageRoot);
            string fileExt = file.FileName.Split('.')[1]; // file extension

            string fname = idd + "_" + fg.GenerateUniqueCode() + "." + fileExt;

            var fullName = Path.Combine(patchOnServer, Path.GetFileName(fname));
            var ThumbfullPath = Path.Combine(fullName, Path.GetFileName(fname + ".80x80." + file.FileName.Split('.')[0]));

            ImageHandler handler = new ImageHandler();

            var ImageBit = ImageHandler.LoadImage(fullName);
            handler.Save(ImageBit, 80, 80, 10, ThumbfullPath);
            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            statuses.Add(UploadResult(fname, file.ContentLength, fullName));
        }

        public ViewDataUploadFilesResult UploadResult(String FileName, int fileSize, String FileFullPath)
        {
            String getType = System.Web.MimeMapping.GetMimeMapping(FileFullPath);
            var result = new ViewDataUploadFilesResult()
            {
                name = FileName,
                size = fileSize,
                type = getType,
                url = UrlBase + FileName,
                deleteUrl = DeleteURL + FileName,
                thumbnailUrl = CheckThumb(getType, FileName),
                deleteType = DeleteType,
            };
            return result;
        }

        public String CheckThumb(String type, String FileName)
        {
            var splited = type.Split('/');
            if (splited.Length == 2)
            {
                string extansion = splited[1];
                if (extansion.Equals("jpeg") || extansion.Equals("jpg") || extansion.Equals("png") || extansion.Equals("gif"))
                {
                    String thumbnailUrl = UrlBase + "/thumbs/" + FileName + ".80x80.jpg";
                    return thumbnailUrl;
                }
                else
                {
                    if (extansion.Equals("octet-stream")) //Fix for exe files
                    {
                        return "/Content/Free-file-icons/48px/exe.png";

                    }
                    if (extansion.Contains("zip")) //Fix for exe files
                    {
                        return "/Content/Free-file-icons/48px/zip.png";
                    }
                    String thumbnailUrl = "/Content/Free-file-icons/48px/" + extansion + ".png";
                    return thumbnailUrl;
                }
            }
            else
            {
                return UrlBase + "/thumbs/" + FileName + ".80x80.jpg";
            }

        }
        public List<String> FilesList()
        {

            List<String> Filess = new List<String>();
            string path = HostingEnvironment.MapPath(serverMapPath);
            System.Diagnostics.Debug.WriteLine(path);
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.GetFiles())
                {
                    Filess.Add(fi.Name);
                    System.Diagnostics.Debug.WriteLine(fi.Name);
                }

            }
            return Filess;
        }

        public List<ViewDataUploadFilesResult> SaveFile(HttpPostedFileBase file, List<ViewDataUploadFilesResult> statuses, string idd)
        {
            String pathOnServer = Path.Combine(StorageRoot);

            statuses = new List<Helpers.ViewDataUploadFilesResult>();

            // HttpPostedFileBase file = requestContext.Request.Files[fileName] as HttpPostedFileBase;
            if (file.ContentLength != 0)
            {
                string fileExt = file.FileName.Split('.')[1]; // file extension

                string fname = idd + "_" + fg.GenerateUniqueCode() + "." + fileExt;
                var fullPath = Path.Combine(pathOnServer, Path.GetFileName(fname));

                // only create if file doesnt exist
                if (!File.Exists(fullPath))
                {
                    try
                    {
                        file.SaveAs(fullPath);

                        //Create thumb
                        string[] imageArray = file.FileName.Split('.');
                        if (imageArray.Length != 0)
                        {
                            var ThumbfullPath = Path.Combine(pathOnServer, "thumbs");

                            using (MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(fullPath)))
                            {
                                String fileThumb = fname + "_80x80." + fileExt; // file.FileName + ".80x80.jpg";
                                var ThumbfullPath2 = Path.Combine(ThumbfullPath, fileThumb);


                                var thumbnail = new WebImage(stream).Resize(80, 80);
                                thumbnail.Save(ThumbfullPath2, fileExt);

                                // Save to db
                                // get doctypeId for current file
                                //int iDocTypeId = new DataContext().DocumentTypes.Where(dt => dt.Name.Trim() == "Student Doc").Select(dt => dt.Id).FirstOrDefault();
                                //SaveToDb(fname, idd, file.ContentLength, fileExt, ThumbfullPath2, iDocTypeId);

                                // sleep for 1 sec
                                Thread.Sleep(1000);
                            }
                        }

                        statuses.Add(UploadResult(fname, file.ContentLength, fullPath));

                    }
                    catch (Exception ex)
                    {
                        string err = ex.ToString();
                    }
                                       
                }
            }

            return statuses;
        }

    }
    public class ViewDataUploadFilesResult
    {
        public string name { get; set; }
        public int size { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string deleteUrl { get; set; }
        public string thumbnailUrl { get; set; }
        public string deleteType { get; set; }
    }
    public class JsonFiles
    {
        public ViewDataUploadFilesResult[] files;
        public string TempFolder { get; set; }
        public JsonFiles(List<ViewDataUploadFilesResult> filesList)
        {
            files = new ViewDataUploadFilesResult[filesList.Count];
            for (int i = 0; i < filesList.Count; i++)
            {
                files[i] = filesList.ElementAt(i);
            }

        }
    }
}

