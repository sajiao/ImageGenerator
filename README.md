# ImageGenerator
Html to image used wkhtmltoimage.exe

  string filePath = Path.Combine(path, fileName);
  Stream outStream = null;
  try
   {
      if (File.Exists(filePath)) File.Delete(filePath);
      outStream = System.IO.File.Create(filePath);
      var htmlToImageConv = new ImageGenerator.HtmlToImageConverter();
      htmlToImageConv.GenerateImage(htmls, ImageFormat.Png.ToString(), outStream);
   }
 catch (Exception ex)
  { 
        
  }
finally
  {
      if (outStream != null) outStream.Close();
  }
