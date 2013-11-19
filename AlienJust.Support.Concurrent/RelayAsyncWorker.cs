﻿using System;
using System.ComponentModel;
using AlienJust.Support.Concurrent.Contracts;

namespace AlienJust.Support.Concurrent
{
	public sealed class RelayAsyncWorker : IAsyncWorker
	{
		private readonly Action _run;
		private readonly Action<int> _progress;
		private readonly BackgroundWorker _worker;
		private bool _wasLaunched;
		private readonly Action<Exception> _complete;

		public RelayAsyncWorker(Action run, Action<int> progress, Action<Exception> complete)
		{
			_wasLaunched = false;
			_run = run;
			_progress = progress;
			_complete = complete;

			_worker = new BackgroundWorker();
			_worker.DoWork += (sender, args) => _run();
			_worker.ProgressChanged += (sender, args) => _progress(args.ProgressPercentage);
			_worker.RunWorkerCompleted += (sender, args) => _complete(args.Error);
		}

		public void Run()
		{
			if (_wasLaunched)
				throw new Exception("Was allready launched, this worker is ontime worker");
			if (_run == null)
				throw new NullReferenceException("Run action is null");
			if (_progress == null)
				throw new NullReferenceException("Progress action is null");
			if (_complete == null)
				throw new NullReferenceException("Complete action is null");

			_wasLaunched = true;
			_worker.RunWorkerAsync();
		}
	}
}
