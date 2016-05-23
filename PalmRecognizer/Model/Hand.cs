namespace PalmRecognizer.Model
{
	using System.Collections.Generic;

	using Emgu.CV;

	internal class Hand
	{
		#region Public Properties

		public List<Finger> Fingers { get; set; }

		#endregion Public Properties

		public Hand(Mat m, List<Defect> defects)
		{
			Fingers = new List<Finger>();

			defects.Sort((x, y) => x.Far.X < y.Far.X ? -1 : (x.Far.X > y.Far.X ? 1 : 0));

			Fingers.Add(new Finger(m, defects[0], defects[1]));
			Fingers.Add(new Finger(m, defects[1], defects[2]));
		}
	}
}
