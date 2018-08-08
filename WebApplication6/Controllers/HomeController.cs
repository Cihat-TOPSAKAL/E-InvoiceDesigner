using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using System.Web.UI.WebControls;
using System.IO.Compression;
using Ionic.Zip;
using System.Text;
using Saxon.Api;
using System.Xml.Linq;

namespace WebApplication6.Controllers
{
    public class HomeController : Controller
    {
        public static string localPathXml = "";
        public static string localPathXslt = "";
        public static string SaveExtension = "";
        public static string localXmlString;
        public static string localXsltString;

        // GET: Home
        public ActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> fileUpload)
        {
            foreach (var file in fileUpload)
            {
                if (Request.Files.Count > 0)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Files/"), fileName);
                        var extension = Path.GetExtension(file.FileName);//dosya uzantısını aldım
                        switch (extension) // Uzantıya göre texareaya yönlendirdim
                        {
                            case ".xml":
                                ViewBag.mesaj = System.IO.File.ReadAllText(path);

                                localPathXml = path;
                                break;
                            case ".xslt":
                                ViewBag.mesaj2 = System.IO.File.ReadAllText(path);

                                localPathXslt = path;
                                break;
                        }
                        SaveExtension = extension;
                    }
                }
            }
            //Dosyalar upload edildikten sonra Saxon HE ile tranform işlemi yap
            Processor xsltProcessor = new Processor();
            DocumentBuilder documentBuilder = xsltProcessor.NewDocumentBuilder();
            documentBuilder.BaseUri = new Uri("file://");
            XdmNode xdmNode = documentBuilder.Build(new StringReader(ViewBag.mesaj));

            XsltCompiler xsltCompiler = xsltProcessor.NewXsltCompiler();
            XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(ViewBag.mesaj2));
            XsltTransformer xsltTransformer = xsltExecutable.Load();
            xsltTransformer.InitialContextNode = xdmNode;

            using (StringWriter stringWriter = new StringWriter())
            {
                Serializer serializer = new Serializer();
                serializer.SetOutputWriter(stringWriter);
                xsltTransformer.Run(serializer);
                ViewBag.cikti = stringWriter;
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult SaveFiles()
        {

            var XmlString = localXmlString;
            var XsltSring = localXsltString;

            if (XmlString == null || XsltSring == null)
            {
                Response.Write("<script lang='JavaScript'>alert('Dosyaları Yüklemeden Veya Dosyalar Üzerinde İşlem Yapmadan İndirme İşlemi Gerçekleşmez...');</script>");
            }
            else //textarealardaki xmml ve xslt kodlarını yeni açılan xml ve xslt uxantılı dosyalara kaydet ve aynı zamana o dosylar yeni oluşturusun ziplenip download edilsin..
            {
                byte[] bytesXml = Encoding.ASCII.GetBytes(XmlString);
                byte[] bytesXslt = Encoding.ASCII.GetBytes(XsltSring);

                MemoryStream zipstr = new MemoryStream();

                using (ZipArchive zip = new ZipArchive(zipstr, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry Xml = zip.CreateEntry("XmlFile.xml");
                    Stream Xmlstr = Xml.Open();
                    Xmlstr.Write(bytesXml, 0, bytesXml.Length);
                    Xmlstr.Flush();
                    Xmlstr.Close();
                    ZipArchiveEntry Xslt = zip.CreateEntry("XsltFile.xslt");
                    Stream Xsltstr = Xslt.Open();
                    Xsltstr.Write(bytesXslt, 0, bytesXslt.Length);
                    Xsltstr.Flush();
                    Xsltstr.Close();
                }

                byte[] zipBytes = zipstr.ToArray();

                Response.ContentType = "application/zip";
                Response.AddHeader("Content-Lenght", zipBytes.Length.ToString());
                Response.AddHeader("content-disposition", "attachment: filename = MyZipFile.zip");
                Response.BinaryWrite(zipBytes);
            }
            return View("Index");
        }

        [HttpPost]
        public JsonResult AjaxTransform(string code, string code2)
        {
            string html = InvoiceTransform(code, code2);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        public string InvoiceTransform(string code, string code2)
        {
            string invoiceHtml = String.Empty;

            if (code == "" || code2 == "")
            {
                Response.Write("<script lang='JavaScript'>alert('Dosyaları Yüklemeden Kaydetme İşlemi Gerçekleşmez');</script>");
            }
            else
            {
                //textarealardaki değişikliklerini hafızaya kaydet
                MemoryStream Xml = new MemoryStream(Encoding.UTF8.GetBytes(code));
                Xml.Write(System.Text.Encoding.UTF8.GetBytes(code), 0, code.Length);
                ViewBag.mesaj = code;


                MemoryStream Xslt = new MemoryStream(Encoding.UTF8.GetBytes(code2));
                Xslt.Write(System.Text.Encoding.UTF8.GetBytes(code2), 0, code2.Length);
                ViewBag.mesaj2 = code2;

                localXmlString = ViewBag.mesaj;
                localXsltString = ViewBag.mesaj2;


                Processor xsltProcessor = new Processor();
                DocumentBuilder documentBuilder = xsltProcessor.NewDocumentBuilder();
                documentBuilder.BaseUri = new Uri("file://");
                XdmNode xdmNode = documentBuilder.Build(new StringReader(code));

                XsltCompiler xsltCompiler = xsltProcessor.NewXsltCompiler();
                XsltExecutable xsltExecutable = xsltCompiler.Compile(new StringReader(code2));
                XsltTransformer xsltTransformer = xsltExecutable.Load();
                xsltTransformer.InitialContextNode = xdmNode;

                using (StringWriter stringWriter = new StringWriter())
                {
                    Serializer serializer = new Serializer();
                    serializer.SetOutputWriter(stringWriter);
                    xsltTransformer.Run(serializer);
                    ViewBag.cikti = stringWriter;
                    invoiceHtml = stringWriter.ToString();
                }
            }
            return invoiceHtml;
        }

    }
}

