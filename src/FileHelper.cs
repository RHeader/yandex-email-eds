using MailClientApp.Services.EMailService.Models;
using MailClientApp.Services.Models;

namespace MailClientApp;

public static class FileHelper
{
     public static FileAttachmentModel ConvertIFormFile(IFormFile file)
     {
          if (file.Length > 0)
          {
               var fileModel = new FileAttachmentModel();
               using (MemoryStream memoryStream = new MemoryStream())
               {
                    file.CopyTo(memoryStream);
                    fileModel.FileObject = memoryStream.ToArray();
               }

               fileModel.ContentType = file.ContentType;
               fileModel.FileName = file.FileName;
               return fileModel;
          }

          throw new ArgumentOutOfRangeException(nameof(file), "File length is out of range");
     }
}