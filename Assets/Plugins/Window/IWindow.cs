using Common.Activatable;
using UnityEngine.Events;

namespace Common.Window
{
	public interface IWindow : IActivatable
	{
		/// <summary>
		/// Событие закрытия окна.
		/// </summary>
		UnityEvent CloseEvent { get; }
	}
}