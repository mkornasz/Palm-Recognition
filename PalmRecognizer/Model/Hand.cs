namespace PalmRecognizer.Model
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	using Emgu.CV;
	using Emgu.CV.Util;

	internal class Hand
	{
		#region Private Members

		private List<Finger> _fingers;

		#endregion Private Members

		public Hand(Mat m, ObservableCollection<Defect> defects)
		{
			_fingers = new List<Finger>();

			foreach (var defect in defects)
			{
				var closeDefect = defects.FirstOrDefault(d => (defect.End - d.Start).Length < 0.01);
				if (closeDefect != null)
				{
					_fingers.Add(new Finger(m, defect, closeDefect));
				}
			}
		}
	}
}
