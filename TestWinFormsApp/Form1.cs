using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AlienJust.Support.Concurrent;
using AlienJust.Support.Concurrent.Contracts;

namespace TestWinFormsApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}
		private IWorker<Action> _asyncWorker;
		private IThreadNotifier _uiNotifier;
		private void QueueBackgroundWorkerTest()
		{
			Log("QueueBackgroundWorkerTest");
			var bw = new BackgroundQueueWorker<Action>(action => action());
			_asyncWorker = bw;
			_uiNotifier = bw;

			for (int i = 0; i < 10; ++i) {
				var i1 = i;
				_asyncWorker.AddWork(() => {
				                                 	Log("Hello" + i1);
				                                 	_uiNotifier.Notify(() => Log("World" + i1));
				                                 });
			}
		}

		private void Button1Click(object sender, EventArgs e) {
			QueueBackgroundWorkerTest();
		}

		private void Log(object content) {
			textBox1.Text += Thread.CurrentThread.ManagedThreadId + content.ToString() + Environment.NewLine;
		}
	}
}
