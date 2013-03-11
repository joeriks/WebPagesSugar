using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebPages;
using System.IO;
namespace RazorShoulds
{
    public class ShouldModule
    {
        public string Description {get;set;}
        public ShouldModule(string description)
        {
            this.Description = description;
        }
        public class TestResult
        {
            public string Message { get; set; }
            public bool Success { get; set; }
            public string AdditionalMessage { get; set; }
            public object ActualResult { get; set; }
        }
        public class ShouldDescription
        {
            public string Description { get; set; }
            public Func<TestResult> TestingFunction { get; set; }
            public TestResult TestResult { get; set; }
            public object ExpectedResult { get; set; }
        }
        public List<ShouldDescription> Shoulds = new List<ShouldDescription>();
        public void Should(string describe, Func<TestResult> should)
        {
            Shoulds.Add(new ShouldDescription
            {
                Description = describe,
                TestingFunction = should
            });
        }
        public void ShouldEqual(string describe, object expected, object actual, Exception exception = null)
        {
            var shouldDescription = new ShouldDescription
            {
                Description = describe,
                ExpectedResult = expected,
                TestingFunction = new Func<TestResult>(() =>
                {

                    var testResult = new TestResult();
                    testResult.ActualResult = actual;
                    if (exception != null)
                    {
                        testResult.Success = false;
                        testResult.Message = exception.Message;
                        testResult.AdditionalMessage = exception.StackTrace.ToString();
                        return testResult;
                    }
                    if (object.Equals(expected,testResult.ActualResult))
                    {
                        testResult.Success = true;
                    }
                    else
                    {
                        testResult.Success = false;
                        testResult.Message = "not equal";
                        testResult.AdditionalMessage = "";
                    }
                    return testResult;
                })
            };
            Shoulds.Add(shouldDescription);

        }
        public void ShouldEqual(string describe, object expected, Func<object> actual)
        {
            object actualResult = null;
            Exception exception = null;
            try
            {
                actualResult = actual();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            ShouldEqual(describe, expected, actualResult, exception);
        }
        public bool Test()
        {
            Shoulds.ForEach(f => { f.TestResult = f.TestingFunction(); });
            return Shoulds.All(t => t.TestResult.Success);
        }

        public void ShouldApproveAll()
        {
            var currentPath = HttpContext.Current.Server.MapPath(WebPageContext.Current.Page.VirtualPath);
            var virtualPath = VirtualPathUtility.GetDirectory(WebPageContext.Current.Page.VirtualPath);
            var physicalPath = Path.GetDirectoryName(currentPath);

            foreach (var file in new System.IO.DirectoryInfo(physicalPath).EnumerateFiles())
            {
                if (file.FullName != currentPath)
                    ShouldApprove(virtualPath + "/" + file.Name);
            }
        }

        public void ShouldApprove(string renderVirtualFileName)
        {
            var renderFileName = HttpContext.Current.Server.MapPath(renderVirtualFileName);
            var fileName = Path.GetFileNameWithoutExtension(renderFileName);

            var approvedPath = Path.GetDirectoryName(renderFileName) + "/approved/";
            var renderedPath = Path.GetDirectoryName(renderFileName) + "/rendered/";

            if (!Directory.Exists(approvedPath)) Directory.CreateDirectory(approvedPath);
            if (!Directory.Exists(renderedPath)) Directory.CreateDirectory(renderedPath);

            var approvedFileName = approvedPath + fileName + ".html";
            var renderedFileName = renderedPath + fileName + ".html";

            string actualResult = null;
            string approvedResult = null;
            Exception exception = null;
            try
            {
                actualResult = WebPageContext.Current.Page.RenderPage(renderVirtualFileName).ToString().Trim();
                System.IO.File.WriteAllText(renderedFileName, actualResult);

                if (File.Exists(approvedFileName))
                {
                    approvedResult = File.ReadAllText(approvedFileName).ToString().Trim();
                }
                else
                {
                    throw new Exception("No approval file exists, check folder /rendered/ and and move to /approved/ after they have been approved");
                }


            }
            catch (Exception ex)
            {
                exception = ex;
            }

            ShouldEqual("File: " + fileName, approvedResult, actualResult, exception);

        }

        public void ShouldRenderAs(string describe, string expected, HelperResult helperResult)
        {
            object actualResult = null;
            Exception exception = null;
            try
            {
                actualResult = helperResult.ToHtmlString();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            ShouldEqual(describe, expected, actualResult, exception);

        }
    }
}
