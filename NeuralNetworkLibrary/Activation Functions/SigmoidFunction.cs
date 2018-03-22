using System;

namespace NeuralNetworkLibrary.Activation_Functions
{
    /// <summary>
    ///     Sigmoid activation function
    /// </summary>
    /// <remarks>
    ///     The class represents sigmoid activation function with
    ///     the next expression:<br />
    ///     <code>
    ///                 1
    ///  f(x) = ------------------
    ///         1 + exp(-alpha * x)
    /// 
    ///            alpha * exp(-alpha * x )
    ///  f'(x) = ---------------------------- = alpha * f(x) * (1 - f(x))
    ///            (1 + exp(-alpha * x))^2
    ///  </code>
    ///     Output range of the function: <b>[0, 1]</b><br /><br />
    ///     Functions graph:<br />
    ///     <img src="sigmoid.bmp" width="242" height="172" />
    /// </remarks>
    public class SigmoidFunction : IActivationFunction
    {
        /// <summary>
        ///     //Sigmoid function
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Sigmoid(double x)
        {
            return 1.7159 * Math.Tanh(0.66666667 * x);
        }

        /// <summary>
        ///     // // derivative of the sigmoid as a function of the sigmoid's output
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double Dsigmoid(double s)
        {
            return 0.66666667 / 1.7159 * (1.7159 + s) * (1.7159 - s);
        }
    }
}