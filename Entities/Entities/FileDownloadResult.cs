using System;

namespace Core.Entities
{
    public class FileDownloadResult
    {
        private String fileName;
        private String result;
        private String error;
        private int retries;

        public FileDownloadResult(string fileName, string result, string error = null)
        {
            this.fileName = fileName;
            this.result = result;
            this.error = error;
            retries = 0;
        }

        public string GetFileName()
        {
            return fileName;
        }

        public string GetResult()
        {
            return result;
        }

        public string GetError()
        {
            return error;
        }

        public int GetRetries()
        {
            return retries;
        }

        public void AddRetries()
        {
            retries++;
        }
    }
}
