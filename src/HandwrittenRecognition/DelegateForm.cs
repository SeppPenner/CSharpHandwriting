// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegateForm.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The delegate form class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandwrittenRecognition;

/// <summary>
/// The delegate form class.
/// </summary>
public class DelegateForm : Form
{
    /// <summary>
    /// The delegate to handle the add object method.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="objectLocal">The object.</param>
    public delegate void DelegateAddObject(int index, object objectLocal);

    /// <summary>
    /// The delegate to handle the thread finished method.
    /// </summary>
    public delegate void DelegateThreadFinished();
}
