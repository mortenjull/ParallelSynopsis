using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AngleSharp.Parser.Html;

namespace ParallelSynopsis
{
    public class LinkExtractor
        : IWorker
    {
        private readonly ConcurrentQueue<Page> _input;
        private readonly ConcurrentQueue<Uri> _output;
        private readonly ConcurrentDictionary<string, int> _visited;

        public LinkExtractor(ConcurrentQueue<Page> input, ConcurrentQueue<Uri> output, ConcurrentDictionary<string, int> visited)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (visited == null)
                throw new ArgumentNullException(nameof(visited));

            _input = input;
            _output = output;
            _visited = visited;
        }

        public void Run(CancellationToken cancellationToken)
        {
            var parser = new HtmlParser();

            while (!cancellationToken.IsCancellationRequested)
            {
                Page page = null;

                if (_input.TryDequeue(out page))
                {
                    var document = parser.Parse(page.Html);

                    foreach (var element in document.QuerySelectorAll("a"))
                    {
                        var href = element.GetAttribute("href");

                        Uri uri = null;
                        
                        if (IsAbsoluteUrl(href))
                        {
                           uri = new Uri(href);
                        }
                        else
                        {
                            uri = new Uri(new Uri(page.Url), href);
                        }

                        if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                        {
                            if (!_visited.ContainsKey(uri.AbsoluteUri))
                                _output.Enqueue(uri);

                            _visited.AddOrUpdate(uri.AbsoluteUri, 1, (key, oldValue) => oldValue + 1);
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
