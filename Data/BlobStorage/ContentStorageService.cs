using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Data.BlobStorage
{
    public interface IContentStorageService
    {
        Task<BlobSavingResult> SaveContentAsync(string content, Encoding encoding, string path,
            CancellationToken cancellationToken);

        Task<string> DownloadContentAsync(string path, Encoding encoding, CancellationToken cancellationToken);
        IEnumerable<string> ListItemsAtPath(string directoryName);
    }

    public class ContentStorageService : IContentStorageService
    {
        private readonly BlobContainerClient BlobContainerClient;

        public ContentStorageService(IOptions<StorageConfig> storageConfig)
        {
            var settings = storageConfig.Value;

            BlobContainerClient = new BlobContainerClient(
                settings.ConnectionString,
                settings.ArticlesContainerName,
                new BlobClientOptions
                    { Retry = { Delay = TimeSpan.FromSeconds(1), MaxRetries = 3, Mode = RetryMode.Fixed } });
            BlobContainerClient.CreateIfNotExists();
        }

        public async Task<BlobSavingResult> SaveContentAsync(string content, Encoding encoding, string path,
            CancellationToken cancellationToken)
        {
            try
            {
                var utcNow = DateTime.UtcNow;
                await using var contentStream = new StringReaderStream(content, encoding);
                var response = await BlobContainerClient.UploadBlobAsync(path, contentStream, cancellationToken);
                return response.Value.LastModified.ToUniversalTime() > utcNow
                    ? BlobSavingResult.Done(BlobContainerClient.Name, path)
                    : BlobSavingResult.Failure("Article has not been modified.");
            }
            catch (Exception e)
            {
                return BlobSavingResult.Failure(e);
            }
        }

        public async Task<string> DownloadContentAsync(string path, Encoding encoding,
            CancellationToken cancellationToken)
        {
            var blob = BlobContainerClient.GetBlobClient(path);
            await using var memory = new MemoryStream();
            await blob.DownloadToAsync(memory, cancellationToken);
            memory.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memory, encoding);
            var content = await reader.ReadToEndAsync();
            return content;
        }


        public IEnumerable<string> ListItemsAtPath(string directoryName)
        {
            var resultSegment = BlobContainerClient.GetBlobsByHierarchy(prefix: directoryName);

            var x = resultSegment.Where(c => c.IsBlob).Select(blobHierarchyItem => blobHierarchyItem.Blob.Name);
            return x;
        }

        private class StringReaderStream : Stream
        {
            private readonly Encoding Encoding;
            private readonly string input;
            private readonly int InputLength;
            private readonly int MaxBytesPerChar;
            private int InputPosition;
            private long position;

            public StringReaderStream(string input)
                : this(input, Encoding.UTF8)
            {
            }

            public StringReaderStream(string input, Encoding encoding)
            {
                Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
                this.input = input;
                InputLength = input?.Length ?? 0;
                if (!string.IsNullOrEmpty(input))
                    Length = encoding.GetByteCount(input);
                MaxBytesPerChar = Equals(encoding, Encoding.ASCII) ? 1 : encoding.GetMaxByteCount(1);
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length { get; }

            public override long Position
            {
                get => position;
                set => throw new NotImplementedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (InputPosition >= InputLength)
                    return 0;
                if (count < MaxBytesPerChar)
                    throw new ArgumentException("count has to be greater or equal to max encoding byte count per char");
                var charCount = Math.Min(InputLength - InputPosition, count / MaxBytesPerChar);
                var byteCount = Encoding.GetBytes(input, InputPosition, charCount, buffer, offset);
                InputPosition += charCount;
                position += byteCount;
                return byteCount;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }
}