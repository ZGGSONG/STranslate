using Xunit;
using Assert = Xunit.Assert;

namespace STranslate.ViewModels.Preference.OCR.Tests
{
    public class PaddleOCRTest
    {
        [Fact()]
        public async Task ExecuteAsyncTest()
        {
            //try
            //{
            //    var ocr = new PaddleOCR();
            //    using FileStream filestream = new("D:\\CodeHub\\self\\STranslate\\src\\STranslate.Tests\\Resources\\ocr_test.png", FileMode.Open);
            //    byte[] bytes = new byte[filestream.Length];
            //    //调用read读取方法
            //    filestream.Read(bytes, 0, bytes.Length);
            //    var ret = await ocr.ExecuteAsync(bytes, CancellationToken.None);
            //    System.Diagnostics.Debug.WriteLine(ret.Text);
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex);
            //    Assert.Fail(ex.Message);
            //}
        }
    }
}
