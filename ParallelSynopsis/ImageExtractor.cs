using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using AngleSharp.Parser.Html;

namespace ParallelSynopsis
{
    public class ImageExtractor
        : IWorker
    {
        private readonly ConcurrentQueue<Page> _input;

        public ImageExtractor(
            ConcurrentQueue<Page> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            _input = input;
        }


        public void Run(CancellationToken cancellationToken)
        {
            var parser = new HtmlParser();

            using (WebClient client = new WebClient())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Page page = null;

                    if (_input.TryDequeue(out page))
                    {
                        var document = parser.Parse(page.Html);

                        foreach (var image in document.QuerySelectorAll("img"))
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {

                                var src = image.GetAttribute("src");

                                if (src != null)
                                {

                                    Uri uri = null;

                                    if (IsAbsoluteUrl(src))
                                    {
                                        uri = new Uri(src);
                                    }
                                    else
                                    {
                                        uri = new Uri(new Uri(page.Url), src);
                                    }

                                    if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                                    {

                                        var extension = System.IO.Path.GetExtension(uri.AbsoluteUri);

                                        if (!string.IsNullOrWhiteSpace(extension))
                                        {

                                            var path = @"c:\temp\" + Guid.NewGuid().ToString() +
                                                       extension.Split('?')[0];

                                            client.DownloadFile(new Uri(uri.AbsoluteUri), path);

                                            Console.WriteLine("Downloaded image: " + path);
                                        }
                                    }


                                    Thread.Sleep(1000);
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }
    }
}
