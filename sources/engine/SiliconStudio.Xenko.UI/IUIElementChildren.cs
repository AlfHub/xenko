using System.Collections.Generic;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Interfaces representing an <see cref="UIElement"/> containing child elements.
    /// </summary>
    public interface IUIElementChildren
    {
        /// <summary>
        /// Gets the children of this element.
        /// </summary>
        IEnumerable<IUIElementChildren> Children { get; }
    }
}