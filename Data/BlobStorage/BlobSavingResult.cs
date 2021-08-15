using System;

namespace Data.BlobStorage
{
    public class BlobSavingResult
    {
        private BlobSavingResult()
        {
        }

        public bool Success { get; private set; }
        public string Path { get; private set; }
        public string Name { get; private set; }
        public Exception InnerException { get; private set; }
        public string ErrorMessage { get; private set; }

        public static BlobSavingResult Done(string container, string path)
        {
            return new BlobSavingResult
                { Path = $"{container}/{path}", Name = path, Success = true };
        }

        public static BlobSavingResult Failure(string errorMessage)
        {
            return new BlobSavingResult { ErrorMessage = errorMessage, Success = false };
        }

        public static BlobSavingResult Failure(Exception e)
        {
            return new BlobSavingResult { ErrorMessage = e.Message, Success = false, InnerException = e };
        }
    }
}