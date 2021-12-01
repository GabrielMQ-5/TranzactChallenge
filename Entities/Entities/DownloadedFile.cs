using System;

namespace Core.Entities
{
    public class DownloadedFile
    {
        private String fileName;
        private String decompressedFileName;
        private double percentage;
        private int totalEntries;
        private int processedEntries;

        public DownloadedFile(string fileName, int processedEntries = 0)
        {
            this.fileName = fileName;
            this.decompressedFileName = null;
            this.percentage = 0;
            this.totalEntries = 0;
            this.processedEntries = processedEntries;
        }

        public string GetFileName()
        {
            return fileName;
        }

        public string GetDecompressedFileName()
        {
            return decompressedFileName;
        }

        public void SetDecompressedFileName(string decompressedFileName)
        {
            this.decompressedFileName = decompressedFileName;
        }

        public double GetPercentage()
        {
            return percentage;
        }

        public void AddPercentage()
        {
            if (processedEntries == totalEntries) return;
            percentage = Math.Round(((double)(++processedEntries * 100) / totalEntries), 2);
        }

        public int GetTotalEntries()
        {
            return totalEntries;
        }

        public void SetTotalEntries(int totalEntries)
        {
            this.totalEntries = totalEntries;
        }
    }
}
