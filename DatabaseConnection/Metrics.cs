using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConnection
{
    public enum MetricType
    {
        Euclidean,
        Manhattan,
        Canberra,
        Hamming
    }


    public static class Metrics
    {
        public static double EvaluateDistance(double[] x, double[] y, MetricType metricType, double hamingDistancePresicion = 0)
        {
            switch(metricType)
            {
                case MetricType.Euclidean:
                    return EuclideanDistance(x, y);
                case MetricType.Manhattan:
                    return ManhattanDistance(x, y);
                case MetricType.Canberra:
                    return CanberraDistance(x, y);
                case MetricType.Hamming:
                    return HammingDistance(x, y, hamingDistancePresicion);
            }
            return -1;
        }


        /// <summary>
        /// Euclidean distance
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>-1 if x and y have different lengths, otherwise euclidean distance between x and y</returns>
        public static double EuclideanDistance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                return -1;
            double sum = 0;
            for(int i=0; i<x.Length; i++)
                sum += (Math.Pow((x[i] - y[i]),2));
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Manhattan distance
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>-1 if x and y have different lengths, otherwise manhattan distance between x and y</returns>
        public static double ManhattanDistance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                return -1;
            double distance = 0;
            for (int i = 0; i < x.Length; i++)
                distance += (Math.Abs(x[i] - y[i]));
            return distance;
        }

        /// <summary>
        /// Canberra distance
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>-1 if x and y have different lengths or x[i] + y[i] = 0, otherwise canberra distance between x and y</returns>
        public static double CanberraDistance(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                return -1;
            double distance = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] + y[i] == 0) // u nas są same dodatnie wartości, więc to nie zajdzie i tak
                    return -1;
                distance += (Math.Abs(x[i] - y[i]) / Math.Abs(x[i] + y[i]));
            }
            return distance;
        }

        /// <summary>
        /// To jest trochę taki mój twór: podobne do odległości hamminga ale z dodaną precyzją
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="presicion"></param>
        /// <returns></returns>
        public static double HammingDistance(double[] x, double[] y, double presicion)
        {
            if (x.Length != y.Length)
                return -1;
            double distance = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (Math.Abs(x[i] - y[i]) > presicion)
                    distance += 1;
            }
            return distance;
        }
    }
}
