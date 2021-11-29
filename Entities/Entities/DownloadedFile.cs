using System;

namespace Core.Entities
{
    public class DownloadedFile
    {
        public String fileName;
        public String decompressedFileName;
        public double percentage;
        public int totalEntries;
        public int processedEntries;

        public DownloadedFile(string fileName)
        {
            this.fileName = fileName;
            this.totalEntries = 0;
            this.decompressedFileName = null;
            this.processedEntries = 0;
            this.percentage = 0;
        }

        public void AddPercentage()
        {
            percentage = Math.Round(((double)(processedEntries++ * 100) / totalEntries), 2);
        }
    }
}
