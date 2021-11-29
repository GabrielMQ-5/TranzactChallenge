using System;

namespace Core.Entities
{
    public class FileDownloadResult
    {
        public String fileName;
        public String result;
        public String error;
        public int retries;

        public FileDownloadResult(string fileName, string result, string error)
        {
            this.fileName = fileName;
            this.result = result;
            this.error = error;
            retries = 0;
        }
    }
}
