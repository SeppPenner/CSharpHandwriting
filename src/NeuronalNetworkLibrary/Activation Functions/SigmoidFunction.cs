// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SigmoidFunction.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The Sigmoid activation function.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.Activation_Functions
{
    using System;

    /// <inheritdoc cref="IActivationFunction"/>
    /// <summary>
    ///     Sigmoid activation function.
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
    /// <seealso cref="IActivationFunction"/>
    public class SigmoidFunction : IActivationFunction
    {
        /// <summary>
        ///     The Sigmoid function.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <returns>The value of the Sigmoid function.</returns>
        public static double Sigmoid(double x)
        {
            return 1.7159 * Math.Tanh(0.66666667 * x);
        }

        /// <summary>
        ///     Derivative of the sigmoid as a function of the Sigmoid's output.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <returns>The value of the derivative Sigmoid function.</returns>
        public static double DeSigmoid(double x)
        {
            return 0.66666667 / 1.7159 * (1.7159 + x) * (1.7159 - x);
        }
    }
}