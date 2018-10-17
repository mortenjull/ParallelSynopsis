using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using AngleSharp;

namespace ParallelSynopsis
{
    public class Crawler
        : IWorker
    {
        private readonly ConcurrentQueue<Uri> _input;
        private readonly ConcurrentQueue<Page> _output;
        private readonly ConcurrentQueue<Page> _output2;

        public Crawler(
            ConcurrentQueue<Uri> input, 
            ConcurrentQueue<Page> output,
            ConcurrentQueue<Page> output2)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (output2 == null)
                throw new ArgumentNullException(nameof(output2));

            _input = input;
            _output = output;
            _output2 = output2;
        }

        public void Run(CancellationToken cancellationToken)
        {
            // Setup the configuration to support document loading
            var config = Configuration.Default.WithDefaultLoader();

            var context = BrowsingContext.New(config);

            while (!cancellationToken.IsCancellationRequested)
            {
                Uri uri = null;

                if (_input.TryDequeue(out uri))
                {
                    Console.WriteLine("Visiting url: " + uri.AbsoluteUri);

                    // Load the names of all The Big Bang Theory episodes from Wikipedia
                    var address = uri.AbsoluteUri;

                    // Asynchronously get the document in a new context using the configuration
                    var document = context.OpenAsync(address).Result;

                    var page = new Page()
                    {
                        Url = document.Url,
                        Html = document.Source.Text
                    };
                    
                    _output.Enqueue(page);
                    _output2.Enqueue(page);
                }

                Thread.Sleep(2000);
            }
        }
    }
}
