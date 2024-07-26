namespace SqlMigrate.Helpers
{

    public class LoadingHelper : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _animationTask;

        public LoadingHelper()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _animationTask = Task.Run(() => Animate(_cancellationTokenSource.Token));
        }

        private void Animate(CancellationToken cancellationToken)
        {
            var animationChars = new[] { '|', '/', '-', '\\' };
            int animationIndex = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write(animationChars[animationIndex]);
                animationIndex = (animationIndex + 1) % animationChars.Length;
                Thread.Sleep(100);
                Console.Write("\b");
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _animationTask.Wait();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
