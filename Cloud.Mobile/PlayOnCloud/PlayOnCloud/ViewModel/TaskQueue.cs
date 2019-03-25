using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayOnCloud
{
	public class TaskQueue
	{
		private SemaphoreSlim semaphore;

		public TaskQueue()
		{
			semaphore = new SemaphoreSlim(1);
		}

		public bool Running { get; set; }

		public async Task<T> EnqueueAsync<T>(Func<Task<T>> taskGenerator)
		{
			await semaphore.WaitAsync();
			try
			{
				Running = true;
				return await taskGenerator();
			}
			finally
			{
				semaphore.Release();
				Running = false;
			}
		}

		public async void Enqueue(Func<Task> taskGenerator)
		{
			await EnqueueAsync(taskGenerator);
		}

		public async Task EnqueueAsync(Func<Task> taskGenerator)
		{
			await semaphore.WaitAsync();
			try
			{
				Running = true;
				await taskGenerator();
			}
			finally
			{
				semaphore.Release();
				Running = false;
			}
		}

		public async Task WaitAllTasks()
		{
			await semaphore.WaitAsync();
			semaphore.Release();
		}
	}
}
