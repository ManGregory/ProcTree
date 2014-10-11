using System.Collections;

namespace ProcTreeGUI.ViewModel
{
  /// <summary>
  /// Provides a virtual folder data structure for arbitrary
  /// child items.
  /// </summary>
  public class FolderItem
  {
    #region Name

      /// <summary>
      /// The name that can be displayed or used as an
      /// ID to perform more complex styling.
      /// </summary>
      public string Name { get; set; }

      #endregion

    #region Items

      /// <summary>
      /// The child items of the folder.
      /// </summary>
      public IEnumerable Items { get; set; }

      #endregion

      /// <summary>
    /// This method is invoked by WPF to render the object if
    /// no data template is available.
    /// </summary>
    /// <returns>Returns the value of the <see cref="Name"/>
    /// property.</returns>
    public override string ToString()
    {
      return string.Format("{0}: {1}", GetType().Name, Name);
    }

  }
}