using System;

namespace nl.FvH.NEAT.Util
{
    public static class Activations
    {
        /// <summary>
        /// Standard Sigmoid Logistic Function
        /// <para>
        /// S(x) = exp(x) / (exp(x) + 1)
        /// </para>
        /// <para>
        /// https://en.wikipedia.org/wiki/Sigmoid_function
        /// </para>
        /// </summary>
        /// <param name="value">Input for Function</param>
        /// <returns>Output from Function</returns>
        public static double Sigmoid(double value)
        {
            // S(x) = exp(x) / (exp(x) + 1)
            double k = System.Math.Exp(value);
            return k / (k + 1);
        }
        /// <summary>
        /// Binary Cut-Off
        /// <para>
        /// G(x) = x >= 0 ? 1 : 0
        /// </para>
        /// </summary>
        /// <param name="value">Input for Function</param>
        /// <returns>Output from Function</returns>
        public static double Step(double value)
        {
            return value >= 0 ? 1 : 0;
        }
        /// <summary>
        /// Tan-H
        /// </summary>
        /// <param name="value">Input for Function</param>
        /// <returns>Output from Function</returns>
        public static double TanH(double value)
        {
            double k = Math.Exp(-2 * value);
            return 2 / (1.0f + k) - 1;
        }
        /// <summary>
        /// Rectified Linear Unit
        /// R(x) = Max(0, x)
        /// </summary>
        /// <param name="value">Input for Function</param>
        /// <returns>Output from Function</returns>
        public static double ReLu(double value)
        {
            return Math.Max(0, value);
        }
        /// <summary>
        /// Leaky ReLu
        /// R(x) = Max(0,x) for x > 0
        /// R(x) = 0.01x    for x < 0
        /// </summary>
        /// <param name="value">Input for Function</param>
        /// <returns>Output from Function</returns>
        public static double LeakyReLu(double value)
        {
            return value < 0 ? value * 0.01d : value;
        }
        /// <summary>
        /// ArcTan
        /// A(x) = ATan(x)
        /// </summary>
        /// <param name="value">Input for Function</param>
        /// <returns>Output from Function</returns>
        public static double ArcTan(double value)
        {
            return Math.Atan(value);
        }
        /// <summary>
        /// SoftSign
        /// S(x) = x / (1 + |x|)
        /// </summary>
        /// <param name="value">Input for Function</param>
        /// <returns>Output from Function</returns>
        public static double SoftSign(double value)
        {
            return value / (1 + Math.Abs(value));
        }
        /// <summary>
        /// Sinusoid
        /// S(x) = Sin(x)
        /// </summary>
        /// <param name="value">Input for Function</param>
        /// <returns>Output from Function</returns>
        public static double Sinusoid(double value)
        {
            return Math.Sin(value);
        }
    }
}
