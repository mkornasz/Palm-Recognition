namespace PalmRecognizer.Model
{
	using System;
	using System.Drawing;

	using PalmRecognizer.ViewModels;

	/// <summary>
	/// Represents the defect detected by Open CV.
	/// </summary>
	internal class Defect : ViewModelBase
	{
		#region Private Members

		private Point2d _start;

		private Point2d _end;

		private Point2d _far;

		private string _id;

		#endregion Private Members

		#region Public Properties

		public Point2d Start
		{
			get
			{
				return _start;
			}
			set
			{
				if (_start == value) return;
				_start = value;
				OnPropertyChanged("Start");
			}
		}

		public Point2d Far
		{
			get
			{
				return _far;
			}
			set
			{
				if (_far == value) return;
				_far = value;
				OnPropertyChanged("Far");
			}
		}

		public Point2d End
		{
			get
			{
				return _end;
			}
			set
			{
				if (_end == value) return;
				_end = value;
				OnPropertyChanged("End");
			}
		}

		public string Id
		{
			get
			{
				return _id;
			}
			set
			{
				if (_id == value) return;
				_id = value;
				OnPropertyChanged("Id");
			}
		}

		#endregion Public Properties

		#region Constructors

		public Defect(Point ptStart, Point ptEnd, Point ptFar, double width, double height)
		{
			Start = new Point2d(ptStart.X / width, ptStart.Y / height);
			End = new Point2d(ptEnd.X / width, ptEnd.Y / height);
			Far = new Point2d(ptFar.X / width, ptFar.Y / height);
			Id = Guid.NewGuid().ToString();
		}

		public Defect(Point2d ptStart, Point2d ptEnd, Point ptFar, double width, double height)
		{
			Start = ptStart;
			End = ptEnd;
			Far = new Point2d(ptFar.X / width, ptFar.Y / height);
			Id = Guid.NewGuid().ToString();
		}

		public Defect(Point2d ptStart, Point2d ptEnd, Point2d ptFar)
		{
			Start = ptStart;
			End = ptEnd;
			Far = ptFar;
			Id = Guid.NewGuid().ToString();
		}

		#endregion Constructors
	}
}