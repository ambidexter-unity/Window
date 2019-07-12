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

		/// <summary>
		/// В этот метод передаются аргументы, полученные при вызове метода ShowWindow() WindowManager-а.
		/// </summary>
		/// <param name="args">Список аргументов.</param>
		void SetArgs(object[] args);
	}
}